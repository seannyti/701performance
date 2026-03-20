<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useCategoryStore } from '@/stores/category.store'
import { useToast } from 'primevue/usetoast'
import { useConfirm } from 'primevue/useconfirm'
import AdminShell from '@/components/layout/AdminShell.vue'
import Button from 'primevue/button'
import InputText from 'primevue/inputtext'
import ConfirmDialog from 'primevue/confirmdialog'
import Dialog from 'primevue/dialog'

const store = useCategoryStore()
const toast = useToast()
const confirm = useConfirm()

const showDialog = ref(false)
const editingId = ref<number | null>(null)
const formName = ref('')

onMounted(() => store.fetchCategories())

function openCreate() {
  editingId.value = null
  formName.value = ''
  showDialog.value = true
}

function openEdit(id: number, name: string) {
  editingId.value = id
  formName.value = name
  showDialog.value = true
}

async function handleSave() {
  if (!formName.value.trim()) return
  try {
    if (editingId.value) {
      await store.updateCategory(editingId.value, { name: formName.value.trim() })
      toast.add({ severity: 'success', summary: 'Updated', detail: 'Category updated.', life: 3000 })
    } else {
      await store.createCategory({ name: formName.value.trim() })
      toast.add({ severity: 'success', summary: 'Created', detail: 'Category created.', life: 3000 })
    }
    showDialog.value = false
  } catch {
    toast.add({ severity: 'error', summary: 'Error', detail: 'Failed to save category.', life: 4000 })
  }
}

async function handleToggle(id: number) {
  await store.toggleCategory(id)
}

function confirmDelete(id: number, name: string, vehicleCount: number) {
  if (vehicleCount > 0) {
    toast.add({ severity: 'warn', summary: 'Cannot Delete', detail: `"${name}" has ${vehicleCount} vehicle(s). Remove them first.`, life: 5000 })
    return
  }
  confirm.require({
    message: `Delete "${name}"? This cannot be undone.`,
    header: 'Confirm Delete',
    icon: 'pi pi-exclamation-triangle',
    acceptClass: 'p-button-danger',
    accept: async () => {
      try {
        await store.deleteCategory(id)
        toast.add({ severity: 'success', summary: 'Deleted', detail: `${name} deleted.`, life: 3000 })
      } catch {
        toast.add({ severity: 'error', summary: 'Error', detail: 'Could not delete category.', life: 4000 })
      }
    }
  })
}
</script>

<template>
  <AdminShell>
    <ConfirmDialog />
    <div class="categories-view">
      <div class="categories-view__header">
        <h2>Categories</h2>
        <Button label="Add Category" icon="pi pi-plus" @click="openCreate" />
      </div>

      <div v-if="store.loading" class="loading">Loading...</div>

      <div v-else class="categories-grid">
        <div
          v-for="cat in store.categories"
          :key="cat.id"
          class="category-card"
          :class="{ 'category-card--inactive': !cat.isActive }"
        >
          <div class="category-card__top">
            <span class="category-card__name">{{ cat.name }}</span>
            <span class="category-card__badge" :class="cat.isActive ? 'badge--active' : 'badge--inactive'">
              {{ cat.isActive ? 'Active' : 'Inactive' }}
            </span>
          </div>
          <div class="category-card__count">{{ cat.vehicleCount }} vehicle{{ cat.vehicleCount !== 1 ? 's' : '' }}</div>
          <div class="category-card__actions">
            <Button label="Edit" icon="pi pi-pencil" size="small" severity="secondary" @click="openEdit(cat.id, cat.name)" />
            <Button
              :label="cat.isActive ? 'Disable' : 'Enable'"
              :icon="cat.isActive ? 'pi pi-eye-slash' : 'pi pi-eye'"
              size="small"
              :severity="cat.isActive ? 'warn' : 'success'"
              @click="handleToggle(cat.id)"
            />
            <Button
              label="Delete"
              icon="pi pi-trash"
              size="small"
              severity="danger"
              :disabled="cat.vehicleCount > 0"
              @click="confirmDelete(cat.id, cat.name, cat.vehicleCount)"
            />
          </div>
        </div>

        <div v-if="store.categories.length === 0" class="empty-state">
          No categories yet. Add one to get started.
        </div>
      </div>
    </div>

    <Dialog v-model:visible="showDialog" :header="editingId ? 'Edit Category' : 'Add Category'" modal style="width: 380px">
      <div class="dialog-form">
        <div class="field">
          <label>Category Name</label>
          <InputText v-model="formName" placeholder="e.g. ATV" @keydown.enter="handleSave" />
        </div>
        <div class="dialog-actions">
          <Button :label="editingId ? 'Save Changes' : 'Create'" :loading="store.saving" @click="handleSave" />
          <Button label="Cancel" severity="secondary" @click="showDialog = false" />
        </div>
      </div>
    </Dialog>
  </AdminShell>
</template>

<style scoped>
.categories-view__header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-bottom: 24px;
}
.categories-view__header h2 { font-size: 1.5rem; font-weight: 700; }

.categories-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(260px, 1fr));
  gap: 16px;
}

.category-card {
  background: #111;
  border: 1px solid #2a2a2a;
  border-radius: 10px;
  padding: 18px;
  display: flex;
  flex-direction: column;
  gap: 12px;
  transition: border-color 0.2s;
}
.category-card:hover { border-color: #444; }
.category-card--inactive { opacity: 0.65; }

.category-card__top {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 8px;
}

.category-card__name {
  font-size: 1rem;
  font-weight: 700;
  color: #f0f0f0;
}

.category-card__badge {
  font-size: 0.7rem;
  font-weight: 700;
  padding: 3px 8px;
  border-radius: 999px;
  text-transform: uppercase;
  letter-spacing: 0.04em;
}
.badge--active { background: rgba(45, 198, 83, 0.15); color: #2dc653; }
.badge--inactive { background: rgba(255, 255, 255, 0.07); color: #777; }

.category-card__count {
  font-size: 0.825rem;
  color: #777;
}

.category-card__actions {
  display: flex;
  gap: 8px;
  flex-wrap: wrap;
}

.empty-state {
  grid-column: 1 / -1;
  text-align: center;
  color: #555;
  padding: 48px;
  font-size: 0.9rem;
}

.loading { color: #777; padding: 24px; }

.dialog-form { display: flex; flex-direction: column; gap: 16px; padding-top: 8px; }
.field { display: flex; flex-direction: column; gap: 6px; }
.field label { font-size: 0.75rem; font-weight: 600; color: #9a9a9a; text-transform: uppercase; }
.dialog-actions { display: flex; gap: 10px; }
</style>
