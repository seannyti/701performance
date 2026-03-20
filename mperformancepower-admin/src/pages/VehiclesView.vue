<script setup lang="ts">
import { onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useVehicleStore } from '@/stores/vehicle.store'
import { useToast } from 'primevue/usetoast'
import { useConfirm } from 'primevue/useconfirm'
import AdminShell from '@/components/layout/AdminShell.vue'
import DataTable from 'primevue/datatable'
import Column from 'primevue/column'
import Button from 'primevue/button'
import ConfirmDialog from 'primevue/confirmdialog'
import Tag from 'primevue/tag'

const store = useVehicleStore()
const toast = useToast()
const confirm = useConfirm()
const router = useRouter()

const apiBase = import.meta.env.VITE_API_URL.replace('/api', '')

onMounted(() => store.fetchVehicles())

function imageUrl(fileName: string | null) {
  return fileName ? `${apiBase}/uploads/${fileName}` : null
}

function formatPrice(price: number) {
  return new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD', maximumFractionDigits: 0 }).format(price)
}

function editVehicle(id: number) {
  router.push(`/catalog/vehicles/${id}/edit`)
}

function confirmDelete(id: number, name: string) {
  confirm.require({
    message: `Delete "${name}"? This cannot be undone.`,
    header: 'Confirm Delete',
    icon: 'pi pi-exclamation-triangle',
    acceptClass: 'p-button-danger',
    accept: async () => {
      await store.deleteVehicle(id)
      toast.add({ severity: 'success', summary: 'Deleted', detail: `${name} removed.`, life: 3000 })
    }
  })
}
</script>

<template>
  <AdminShell>
    <ConfirmDialog />
    <div class="vehicles-view">
      <div class="vehicles-view__header">
        <h2>Vehicles</h2>
        <Button label="Add Vehicle" icon="pi pi-plus" @click="router.push('/catalog/vehicles/new')" />
      </div>

      <DataTable
        :value="store.vehicles"
        :loading="store.loading"
        paginator
        :rows="20"
        stripedRows
        class="vehicles-table"
      >
        <Column header="Image" style="width: 80px">
          <template #body="{ data }">
            <img v-if="imageUrl(data.primaryImage)" :src="imageUrl(data.primaryImage)!" style="width:60px;height:45px;object-fit:cover;border-radius:4px;" />
            <span v-else class="no-img">—</span>
          </template>
        </Column>
        <Column field="year" header="Year" sortable style="width: 80px" />
        <Column field="make" header="Make" sortable />
        <Column field="model" header="Model" sortable />
        <Column field="category" header="Category" sortable>
          <template #body="{ data }">
            <Tag :value="data.category" severity="info" />
          </template>
        </Column>
        <Column field="condition" header="Cond." sortable>
          <template #body="{ data }">
            <Tag :value="data.condition" :severity="data.condition === 'New' ? 'success' : 'secondary'" />
          </template>
        </Column>
        <Column field="price" header="Price" sortable>
          <template #body="{ data }">{{ formatPrice(data.price) }}</template>
        </Column>
        <Column field="stock" header="Stock" sortable style="width: 80px" />
        <Column field="featured" header="Featured" style="width: 90px">
          <template #body="{ data }">
            <i :class="data.featured ? 'pi pi-star-fill' : 'pi pi-star'" :style="{ color: data.featured ? '#f4a261' : '#555' }" />
          </template>
        </Column>
        <Column header="Actions" style="width: 120px">
          <template #body="{ data }">
            <div style="display:flex;gap:6px;">
              <Button icon="pi pi-pencil" size="small" severity="secondary" @click="editVehicle(data.id)" />
              <Button icon="pi pi-trash" size="small" severity="danger" @click="confirmDelete(data.id, `${data.year} ${data.make} ${data.model}`)" />
            </div>
          </template>
        </Column>
      </DataTable>
    </div>
  </AdminShell>
</template>

<style scoped>
.vehicles-view__header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-bottom: 20px;
}

.vehicles-view__header h2 { font-size: 1.5rem; font-weight: 700; }
.no-img { color: #555; }
</style>
