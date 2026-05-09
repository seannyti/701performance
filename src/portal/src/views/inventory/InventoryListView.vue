<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useToast } from 'primevue/usetoast'
import { useConfirm } from 'primevue/useconfirm'
import DataTable from 'primevue/datatable'
import Column from 'primevue/column'
import Button from 'primevue/button'
import InputText from 'primevue/inputtext'
import Select from 'primevue/select'
import Tag from 'primevue/tag'
import ToggleSwitch from 'primevue/toggleswitch'
import inventoryService, { type Vehicle } from '../../services/inventory.service'

const router = useRouter()
const toast = useToast()
const confirm = useConfirm()

const vehicles = ref<Vehicle[]>([])
const loading = ref(true)
const total = ref(0)
const search = ref('')

const activeTab = ref<'vehicles' | 'gear'>('vehicles')
const filters = ref({
  type: null as string | null,
  condition: null as string | null,
  status: null as string | null,
})

const allTypes = ref(inventoryService.getTypes())
const typeOptionsForTab = computed(() => {
  const list = activeTab.value === 'gear'
    ? allTypes.value.filter(t => inventoryService.isGearType(t.value))
    : allTypes.value.filter(t => !inventoryService.isGearType(t.value))
  return [{ label: 'All Types', value: null as string | null }, ...list]
})
const allConditionOptions = [
  { label: 'All Conditions', value: null as string | null },
  { label: 'New', value: 'new' },
  { label: 'Used', value: 'used' },
  { label: 'Consignment', value: 'consignment' },
]
const conditionOptions = computed(() =>
  activeTab.value === 'gear' ? allConditionOptions.filter(c => c.value !== 'consignment') : allConditionOptions
)
const statusOptions = [
  { label: 'All Statuses', value: null },
  ...inventoryService.getStatuses().map(s => ({ label: s.label, value: s.value }))
]

const tabCounts = computed(() => ({
  vehicles: vehicles.value.filter(v => !inventoryService.isGearType(v.type)).length,
  gear: vehicles.value.filter(v => inventoryService.isGearType(v.type)).length,
}))

const filtered = computed(() => {
  let list = vehicles.value.filter(v =>
    activeTab.value === 'gear' ? inventoryService.isGearType(v.type) : !inventoryService.isGearType(v.type)
  )
  if (search.value) {
    const q = search.value.toLowerCase()
    list = list.filter(v =>
      `${v.year} ${v.make} ${v.model} ${v.stockNumber} ${v.vin ?? ''}`.toLowerCase().includes(q)
    )
  }
  if (filters.value.type) list = list.filter(v => v.type === filters.value.type)
  if (filters.value.condition) list = list.filter(v => v.condition === filters.value.condition)
  if (filters.value.status) list = list.filter(v => v.status === filters.value.status)
  return list
})

function switchTab(tab: 'vehicles' | 'gear') {
  activeTab.value = tab
  filters.value.type = null
  if (tab === 'gear' && filters.value.condition === 'consignment') {
    filters.value.condition = null
  }
}

function addItem() {
  const path = activeTab.value === 'gear' ? '/inventory/new?type=apparel' : '/inventory/new'
  router.push(path)
}

async function load() {
  loading.value = true
  inventoryService.getTypesAsync().then(t => {
    allTypes.value = t
  })
  try {
    const res = await inventoryService.getAll({ pageSize: 500 })
    vehicles.value = res.data
    total.value = res.total
  } finally {
    loading.value = false
  }
}

async function toggleFeatured(vehicle: Vehicle) {
  try {
    const result = await inventoryService.toggleFeatured(vehicle.id)
    vehicle.isFeatured = result.isFeatured
    toast.add({ severity: 'success', summary: result.isFeatured ? 'Marked as Featured' : 'Removed from Featured', life: 2500 })
  } catch {
    toast.add({ severity: 'error', summary: 'Failed to update', life: 3000 })
  }
}

function confirmDelete(vehicle: Vehicle) {
  const label = inventoryService.isGearType(vehicle.type)
    ? `${vehicle.make} ${vehicle.model}`
    : `${vehicle.year} ${vehicle.make} ${vehicle.model}`
  confirm.require({
    message: `Delete ${label} (${vehicle.stockNumber})?`,
    header: 'Confirm Delete',
    icon: 'pi pi-exclamation-triangle',
    rejectLabel: 'Cancel',
    acceptLabel: 'Delete',
    accept: () => deleteVehicle(vehicle)
  })
}

async function deleteVehicle(vehicle: Vehicle) {
  try {
    await inventoryService.delete(vehicle.id)
    vehicles.value = vehicles.value.filter(v => v.id !== vehicle.id)
    toast.add({ severity: 'success', summary: 'Item deleted', life: 2500 })
  } catch {
    toast.add({ severity: 'error', summary: 'Failed to delete item', life: 3000 })
  }
}

function exportCsv() {
  const headers = ['Stock #', 'Year', 'Make', 'Model', 'Type', 'Condition', 'Status', 'Cost', 'MSRP', 'Sale Price', 'Days on Lot']
  const rows = filtered.value.map(v => [
    v.stockNumber, v.year, v.make, v.model, v.type, v.condition, v.status,
    v.cost, v.msrp, v.salePrice, inventoryService.daysOnLot(v.createdAt)
  ])
  const csv = [headers, ...rows].map(r => r.join(',')).join('\n')
  const blob = new Blob([csv], { type: 'text/csv' })
  const url = URL.createObjectURL(blob)
  const a = document.createElement('a'); a.href = url; a.download = 'inventory.csv'; a.click()
  URL.revokeObjectURL(url)
}

function formatCurrency(n: number) {
  return n.toLocaleString('en-US', { style: 'currency', currency: 'USD', maximumFractionDigits: 0 })
}

function getStatusSeverity(status: string) {
  return inventoryService.getStatuses().find(s => s.value === status)?.severity ?? 'secondary'
}

function clearFilters() {
  search.value = ''
  filters.value = { type: null, condition: null, status: null }
}

const hasFilters = computed(() => search.value || filters.value.type || filters.value.condition || filters.value.status)

onMounted(load)
</script>

<template>
  <div class="inventory-list">
    <div class="page-header">
      <div>
        <h1>Inventory</h1>
        <p class="page-sub">{{ total }} total items</p>
      </div>
      <div class="header-actions">
        <Button icon="pi pi-download" label="Export CSV" severity="secondary" outlined @click="exportCsv" />
        <Button
          icon="pi pi-plus"
          :label="activeTab === 'gear' ? 'Add Apparel & Gear' : 'Add Vehicle'"
          @click="addItem"
        />
      </div>
    </div>

    <!-- Tabs -->
    <div class="tabs">
      <button
        class="tab-btn"
        :class="{ active: activeTab === 'vehicles' }"
        @click="switchTab('vehicles')"
      >
        <i class="pi pi-car" />
        Vehicles
        <span class="tab-count">{{ tabCounts.vehicles }}</span>
      </button>
      <button
        class="tab-btn"
        :class="{ active: activeTab === 'gear' }"
        @click="switchTab('gear')"
      >
        <i class="pi pi-shopping-bag" />
        Apparel & Gear
        <span class="tab-count">{{ tabCounts.gear }}</span>
      </button>
    </div>

    <!-- Filters -->
    <div class="filters-bar">
      <div class="search-wrap">
        <i class="pi pi-search search-icon" />
        <InputText v-model="search" :placeholder="activeTab === 'gear' ? 'Search stock #, brand, model...' : 'Search stock #, VIN, year, make, model...'" class="search-input" />
      </div>
      <Select v-model="filters.type" :options="typeOptionsForTab" optionLabel="label" optionValue="value" placeholder="Type" class="filter-select" />
      <Select v-model="filters.condition" :options="conditionOptions" optionLabel="label" optionValue="value" placeholder="Condition" class="filter-select" />
      <Select v-model="filters.status" :options="statusOptions" optionLabel="label" optionValue="value" placeholder="Status" class="filter-select" />
      <Button v-if="hasFilters" icon="pi pi-times" label="Clear" text severity="secondary" @click="clearFilters" />
    </div>

    <!-- Table -->
    <div class="table-wrap">
      <DataTable
        :value="filtered"
        :loading="loading"
        dataKey="id"
        class="pp-table inventory-table"
        paginator
        :rows="25"
        :rowsPerPageOptions="[25, 50, 100]"
        stripedRows
        scrollable
      >
        <Column header="" style="width: 64px; padding: 0.5rem;">
          <template #body="{ data }">
            <div class="thumb-wrap">
              <img
                v-if="data.images?.length"
                :src="data.images.find((i: any) => i.isPrimary)?.thumbnailUrl || data.images[0]?.thumbnailUrl"
                class="thumb"
              />
              <div v-else class="thumb-empty"><i class="pi pi-image" /></div>
            </div>
          </template>
        </Column>

        <Column field="stockNumber" header="Stock #" sortable style="min-width: 100px" />

        <Column :header="activeTab === 'gear' ? 'Item' : 'Vehicle'" sortable sortField="make" style="min-width: 220px">
          <template #body="{ data }">
            <div class="vehicle-cell">
              <span class="vehicle-name">
                <template v-if="inventoryService.isGearType(data.type)">{{ data.make }} {{ data.model }}</template>
                <template v-else>{{ data.year }} {{ data.make }} {{ data.model }}</template>
              </span>
              <span v-if="data.trim" class="vehicle-trim">{{ data.trim }}</span>
            </div>
          </template>
        </Column>

        <Column field="type" header="Type" sortable style="min-width: 100px">
          <template #body="{ data }">
            <span class="type-label">{{ data.type.toUpperCase() }}</span>
          </template>
        </Column>

        <Column field="condition" header="Condition" sortable style="min-width: 120px">
          <template #body="{ data }">
            <Tag
              :value="data.condition"
              :severity="data.condition === 'new' ? 'success' : data.condition === 'used' ? 'info' : 'secondary'"
            />
          </template>
        </Column>

        <Column field="status" header="Status" sortable style="min-width: 110px">
          <template #body="{ data }">
            <Tag :value="data.status" :severity="getStatusSeverity(data.status)" />
          </template>
        </Column>

        <Column header="Days on Lot" sortable sortField="createdAt" style="min-width: 120px">
          <template #body="{ data }">
            <span class="days-badge" :style="{ color: inventoryService.agingColor(inventoryService.daysOnLot(data.createdAt)) }">
              {{ inventoryService.daysOnLot(data.createdAt) }}d
            </span>
          </template>
        </Column>

        <Column field="cost" header="Cost" sortable style="min-width: 110px">
          <template #body="{ data }">{{ formatCurrency(data.cost) }}</template>
        </Column>

        <Column field="salePrice" header="Sale Price" sortable style="min-width: 120px">
          <template #body="{ data }">{{ formatCurrency(data.salePrice) }}</template>
        </Column>

        <Column header="Featured" style="min-width: 90px; text-align: center">
          <template #body="{ data }">
            <ToggleSwitch :modelValue="data.isFeatured" @update:modelValue="() => toggleFeatured(data)" />
          </template>
        </Column>

        <Column header="Actions" style="min-width: 120px">
          <template #body="{ data }">
            <div class="action-btns">
              <Button icon="pi pi-pencil" text rounded size="small" v-tooltip.top="'Edit'" @click="router.push(`/inventory/${data.id}/edit`)" />
              <Button icon="pi pi-external-link" text rounded size="small" severity="secondary" v-tooltip.top="'View on site'" @click="router.push(`/inventory/${data.id}/edit`)" />
              <Button icon="pi pi-trash" text rounded size="small" severity="danger" v-tooltip.top="'Delete'" @click="confirmDelete(data)" />
            </div>
          </template>
        </Column>

        <template #empty>
          <div class="empty-state">
            <i :class="activeTab === 'gear' ? 'pi pi-shopping-bag' : 'pi pi-car'" style="font-size: 2rem; color: #555;" />
            <p>
              No {{ activeTab === 'gear' ? 'apparel & gear' : 'vehicles' }} found.
              <span class="link" @click="addItem">Add your first item.</span>
            </p>
          </div>
        </template>
      </DataTable>
    </div>
  </div>
</template>

<style scoped>
.inventory-list { display: flex; flex-direction: column; gap: 1.5rem; }

.page-header { display: flex; align-items: flex-start; justify-content: space-between; flex-wrap: wrap; gap: 1rem; }
.page-header h1 { font-size: 1.75rem; font-weight: 800; color: white; }
.page-sub { color: #9e9e9e; font-size: 0.875rem; margin-top: 0.25rem; }
.header-actions { display: flex; gap: 0.75rem; align-items: center; }

.tabs { display: flex; gap: 0; border-bottom: 1px solid #2a2a2a; }
.tab-btn {
  display: flex; align-items: center; gap: 0.5rem;
  padding: 0.75rem 1.25rem; background: none; border: none;
  border-bottom: 2px solid transparent; margin-bottom: -1px;
  color: #9e9e9e; font-size: 0.9rem; font-weight: 600; cursor: pointer;
  transition: color 0.15s, border-color 0.15s;
}
.tab-btn:hover { color: #ccc; }
.tab-btn.active { color: #e53935; border-bottom-color: #e53935; }
.tab-count {
  font-size: 0.7rem; padding: 0.1rem 0.5rem; border-radius: 999px;
  background: #1e1e1e; color: #888;
}
.tab-btn.active .tab-count { background: rgba(229,57,53,0.15); color: #e53935; }

.filters-bar { display: flex; align-items: center; gap: 0.75rem; flex-wrap: wrap; background: #141414; border: 1px solid #2a2a2a; border-radius: 10px; padding: 0.875rem 1rem; }
.search-wrap { position: relative; flex: 1; min-width: 240px; }
.search-icon { position: absolute; left: 0.75rem; top: 50%; transform: translateY(-50%); color: #555; font-size: 0.875rem; pointer-events: none; z-index: 1; }
.search-input { width: 100%; padding-left: 2.25rem !important; }
.filter-select { min-width: 140px; }

.table-wrap { background: #141414; border: 1px solid #2a2a2a; border-radius: 10px; overflow: hidden; }

.thumb-wrap { width: 48px; height: 36px; border-radius: 6px; overflow: hidden; background: #1e1e1e; flex-shrink: 0; }
.thumb { width: 100%; height: 100%; object-fit: cover; }
.thumb-empty { width: 100%; height: 100%; display: flex; align-items: center; justify-content: center; color: #444; font-size: 0.75rem; }

.vehicle-cell { display: flex; flex-direction: column; gap: 2px; }
.vehicle-name { font-weight: 600; color: white; font-size: 0.875rem; }
.vehicle-trim { font-size: 0.75rem; color: #9e9e9e; }

.type-label { font-size: 0.7rem; font-weight: 700; letter-spacing: 0.5px; color: #e53935; }
.days-badge { font-weight: 700; font-size: 0.875rem; }
.action-btns { display: flex; gap: 0.25rem; }

.empty-state { text-align: center; padding: 3rem; color: #9e9e9e; display: flex; flex-direction: column; align-items: center; gap: 0.75rem; }
.link { color: #e53935; cursor: pointer; text-decoration: underline; }
</style>
