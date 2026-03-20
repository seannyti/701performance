<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useOrderStore } from '@/stores/order.store'
import AdminShell from '@/components/layout/AdminShell.vue'
import Button from 'primevue/button'
import InputText from 'primevue/inputtext'
import Select from 'primevue/select'
import DataTable from 'primevue/datatable'
import Column from 'primevue/column'
import Tag from 'primevue/tag'
import IconField from 'primevue/iconfield'
import InputIcon from 'primevue/inputicon'

const store = useOrderStore()
const router = useRouter()
const search = ref('')
const statusFilter = ref<string | null>(null)
let searchTimeout: ReturnType<typeof setTimeout> | null = null

const statusOptions = [
  { label: 'Pending', value: 'Pending' },
  { label: 'Completed', value: 'Completed' },
  { label: 'Delivered', value: 'Delivered' },
  { label: 'Cancelled', value: 'Cancelled' },
]

const statusSeverity: Record<string, string> = {
  Pending: 'warn',
  Completed: 'success',
  Delivered: 'info',
  Cancelled: 'danger',
}

const methodSeverity: Record<string, string> = {
  Cash: 'success',
  Financed: 'info',
  TradeIn: 'secondary',
}

onMounted(() => store.fetchOrders())

function applySearch() {
  if (searchTimeout) clearTimeout(searchTimeout)
  searchTimeout = setTimeout(() => {
    store.fetchOrders(1, 20, statusFilter.value ?? undefined, search.value || undefined)
  }, 300)
}

function applyStatus() {
  store.fetchOrders(1, 20, statusFilter.value ?? undefined, search.value || undefined)
}

function clearFilters() {
  search.value = ''
  statusFilter.value = null
  store.fetchOrders()
}

function formatDate(d: string) {
  return new Date(d).toLocaleDateString('en-US', { month: 'short', day: 'numeric', year: 'numeric' })
}

function formatPrice(n: number) {
  return new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD', maximumFractionDigits: 0 }).format(n)
}
</script>

<template>
  <AdminShell>
    <div class="orders-view">
      <div class="page-header">
        <div>
          <h1>Orders</h1>
          <p>Manage all vehicle sales and orders.</p>
        </div>
        <Button label="New Order" icon="pi pi-plus" severity="warn" @click="router.push('/sales/orders/new')" />
      </div>

      <!-- Filters -->
      <div class="filter-bar">
        <IconField style="flex:1; max-width:320px;">
          <InputIcon class="pi pi-search" />
          <InputText v-model="search" placeholder="Customer name, email, or vehicle..." style="width:100%" @input="applySearch" />
        </IconField>
        <Select
          v-model="statusFilter"
          :options="statusOptions"
          option-label="label"
          option-value="value"
          placeholder="All Statuses"
          show-clear
          style="min-width:160px"
          @change="applyStatus"
        />
        <Button label="Clear" severity="secondary" outlined @click="clearFilters" />
      </div>

      <!-- Table -->
      <DataTable
        :value="store.orders"
        :loading="store.loading"
        class="orders-table"
      >
        <template #empty>No orders found.</template>
        <Column field="id" header="#" style="width:60px; color:#555; font-size:0.8rem;" />
        <Column header="Customer">
          <template #body="{ data }">
            <div class="customer-cell">
              <span class="customer-name">{{ data.customerName }}</span>
              <span class="customer-email">{{ data.customerEmail }}</span>
            </div>
          </template>
        </Column>
        <Column field="vehicleName" header="Vehicle" style="color:#9a9a9a;" />
        <Column header="Sale Price">
          <template #body="{ data }">
            <span style="font-weight:600;">{{ formatPrice(data.salePrice) }}</span>
          </template>
        </Column>
        <Column header="Method">
          <template #body="{ data }">
            <Tag :value="data.paymentMethod" :severity="methodSeverity[data.paymentMethod] ?? 'secondary'" />
          </template>
        </Column>
        <Column header="Status">
          <template #body="{ data }">
            <Tag :value="data.status" :severity="statusSeverity[data.status] ?? 'secondary'" />
          </template>
        </Column>
        <Column header="Date">
          <template #body="{ data }">
            <span style="color:#777; font-size:0.8rem;">{{ formatDate(data.createdAt) }}</span>
          </template>
        </Column>
        <Column header="" style="width:60px;">
          <template #body="{ data }">
            <Button icon="pi pi-pencil" severity="secondary" text rounded @click="router.push(`/sales/orders/${data.id}/edit`)" />
          </template>
        </Column>
      </DataTable>

      <!-- Row count -->
      <div v-if="store.orders.length > 0" class="row-count">
        {{ store.orders.length }} of {{ store.totalCount }} orders
      </div>
    </div>
  </AdminShell>
</template>

<style scoped>
.orders-view { display: flex; flex-direction: column; gap: 20px; }

.page-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
}
.page-header h1 { font-size: 1.5rem; font-weight: 700; }
.page-header p { font-size: 0.875rem; color: #9a9a9a; margin-top: 4px; }

.filter-bar {
  display: flex;
  align-items: center;
  gap: 12px;
  flex-wrap: wrap;
  background: #111;
  border: 1px solid #222;
  border-radius: 10px;
  padding: 14px 18px;
}

.customer-cell { display: flex; flex-direction: column; gap: 2px; }
.customer-name { color: #f0f0f0; font-weight: 500; font-size: 0.875rem; }
.customer-email { font-size: 0.775rem; color: #777; }

.row-count { font-size: 0.8rem; color: #555; text-align: right; }
</style>
