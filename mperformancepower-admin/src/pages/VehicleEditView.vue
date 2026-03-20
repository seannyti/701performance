<script setup lang="ts">
import { ref, reactive, onMounted, computed } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useVehicleStore } from '@/stores/vehicle.store'
import { useCategoryStore } from '@/stores/category.store'
import { useToast } from 'primevue/usetoast'
import { uploadImages, deleteImage, setPrimaryImage } from '@/services/vehicle.service'
import type { VehicleSpec } from '@/types/vehicle.types'
import AdminShell from '@/components/layout/AdminShell.vue'
import InputText from 'primevue/inputtext'
import InputNumber from 'primevue/inputnumber'
import Textarea from 'primevue/textarea'
import Select from 'primevue/select'
import ToggleSwitch from 'primevue/toggleswitch'
import Button from 'primevue/button'
import type { CreateVehicleDto, VehicleImage } from '@/types/vehicle.types'

const route = useRoute()
const router = useRouter()
const store = useVehicleStore()
const categoryStore = useCategoryStore()
const toast = useToast()

const isEdit = computed(() => !!route.params.id)
const vehicleId = computed(() => isEdit.value ? Number(route.params.id) : null)

const conditions = ['New', 'Used']

const form = reactive<CreateVehicleDto>({
  make: '',
  model: '',
  year: new Date().getFullYear(),
  categoryId: 0,
  price: 0,
  mileage: null,
  condition: 'New',
  description: '',
  stock: 1,
  featured: false,
})

const images = ref<VehicleImage[]>([])
const specs = ref<VehicleSpec[]>([])
const apiBase = import.meta.env.VITE_API_URL.replace('/api', '')

onMounted(async () => {
  await categoryStore.fetchCategories()

  if (isEdit.value && vehicleId.value) {
    await store.fetchVehicle(vehicleId.value)
    const v = store.selectedVehicle
    if (v) {
      Object.assign(form, {
        make: v.make, model: v.model, year: v.year,
        categoryId: v.categoryId, price: v.price, mileage: v.mileage,
        condition: v.condition, description: v.description,
        stock: v.stock, featured: v.featured,
      })
      images.value = [...v.images]
      specs.value = [...(v.specs ?? [])]
    }
  } else if (categoryStore.categories.length > 0) {
    form.categoryId = categoryStore.categories[0].id
  }
})

async function handleSubmit() {
  try {
    const payload = { ...form, specs: specs.value.filter(s => s.label.trim()) }
    if (isEdit.value && vehicleId.value) {
      await store.updateVehicle(vehicleId.value, payload)
      toast.add({ severity: 'success', summary: 'Saved', detail: 'Vehicle updated.', life: 3000 })
    } else {
      const v = await store.createVehicle(payload)
      toast.add({ severity: 'success', summary: 'Created', detail: 'Vehicle added.', life: 3000 })
      router.replace(`/catalog/vehicles/${v.id}/edit`)
    }
  } catch {
    toast.add({ severity: 'error', summary: 'Error', detail: 'Failed to save vehicle.', life: 4000 })
  }
}

async function handleImageUpload(event: Event) {
  const files = (event.target as HTMLInputElement).files
  if (!files || !vehicleId.value) {
    toast.add({ severity: 'warn', summary: 'Save first', detail: 'Save the vehicle before uploading images.', life: 3000 })
    return
  }
  try {
    const newImages = await uploadImages(vehicleId.value, files)
    images.value.push(...newImages)
    toast.add({ severity: 'success', summary: 'Uploaded', detail: `${newImages.length} image(s) uploaded.`, life: 3000 })
  } catch {
    toast.add({ severity: 'error', summary: 'Upload failed', detail: 'Could not upload images.', life: 4000 })
  }
}

async function handleDeleteImage(img: VehicleImage) {
  await deleteImage(img.id)
  images.value = images.value.filter(i => i.id !== img.id)
}

async function handleSetPrimary(img: VehicleImage) {
  await setPrimaryImage(img.id)
  images.value = images.value.map(i => ({ ...i, isPrimary: i.id === img.id }))
}
</script>

<template>
  <AdminShell>
    <div class="vehicle-edit">
      <div class="vehicle-edit__header">
        <Button icon="pi pi-arrow-left" severity="secondary" size="small" @click="router.push('/catalog/vehicles')" />
        <h2>{{ isEdit ? 'Edit Vehicle' : 'Add Vehicle' }}</h2>
      </div>

      <div class="vehicle-edit__layout">
        <form @submit.prevent="handleSubmit" class="vehicle-form">
          <div class="form-grid">
            <div class="field">
              <label>Make *</label>
              <InputText v-model="form.make" required />
            </div>
            <div class="field">
              <label>Model *</label>
              <InputText v-model="form.model" required />
            </div>
            <div class="field">
              <label>Year *</label>
              <InputNumber v-model="form.year" :min="1900" :max="2100" :use-grouping="false" />
            </div>
            <div class="field">
              <label>Category</label>
              <Select
                v-model="form.categoryId"
                :options="categoryStore.categories.filter(c => c.isActive)"
                optionLabel="name"
                optionValue="id"
                placeholder="Select category"
              />
            </div>
            <div class="field">
              <label>Price ($) *</label>
              <InputNumber v-model="form.price" mode="currency" currency="USD" locale="en-US" />
            </div>
            <div class="field">
              <label>Mileage</label>
              <InputNumber v-model="form.mileage" :min="0" placeholder="Leave blank if new" />
            </div>
            <div class="field">
              <label>Condition</label>
              <Select v-model="form.condition" :options="conditions" />
            </div>
            <div class="field">
              <label>Stock</label>
              <InputNumber v-model="form.stock" :min="0" />
            </div>
          </div>

          <div class="field" style="margin-top: 16px;">
            <label>Description</label>
            <Textarea v-model="form.description" rows="5" />
          </div>

          <div class="field-inline">
            <label>Featured on Homepage</label>
            <ToggleSwitch v-model="form.featured" />
          </div>

          <div class="form-actions">
            <Button type="submit" :label="isEdit ? 'Save Changes' : 'Create Vehicle'" :loading="store.saving" />
            <Button type="button" label="Cancel" severity="secondary" @click="router.push('/catalog/vehicles')" />
          </div>
        </form>

        <div class="image-panel" v-if="isEdit">
          <h3>Images</h3>
          <label class="upload-btn">
            <i class="pi pi-upload" /> Upload Images
            <input type="file" accept="image/*" multiple @change="handleImageUpload" style="display:none;" />
          </label>

          <div class="image-list">
            <div v-for="img in images" :key="img.id" class="image-item">
              <img :src="`${apiBase}/uploads/${img.fileName}`" :alt="img.fileName" />
              <div class="image-item__actions">
                <span v-if="img.isPrimary" class="primary-badge">Primary</span>
                <Button v-else icon="pi pi-star" severity="secondary" size="small" v-tooltip.top="'Set as Primary'" @click="handleSetPrimary(img)" />
                <Button icon="pi pi-trash" severity="danger" size="small" @click="handleDeleteImage(img)" />
              </div>
            </div>
            <div v-if="images.length === 0" class="no-images">No images yet.</div>
          </div>
        </div>

        <div class="image-panel" v-else>
          <p style="color:#9a9a9a; font-size:0.875rem;">Save the vehicle first to upload images.</p>
        </div>
      </div>

      <!-- Specs editor -->
      <div class="specs-panel">
        <div class="specs-panel__header">
          <h3>Specs</h3>
          <button type="button" class="add-spec-btn" @click="specs.push({ label: '', value: '' })">+ Add Spec</button>
        </div>
        <div v-if="specs.length === 0" class="specs-empty">No specs yet. Click "+ Add Spec" to add engine size, top speed, weight, etc.</div>
        <div class="specs-list">
          <div v-for="(spec, i) in specs" :key="i" class="spec-row">
            <input v-model="spec.label" type="text" placeholder="Label (e.g. Engine)" class="spec-input" />
            <input v-model="spec.value" type="text" placeholder="Value (e.g. 150cc)" class="spec-input" />
            <button type="button" class="spec-remove" @click="specs.splice(i, 1)" title="Remove">×</button>
          </div>
        </div>
      </div>
    </div>
  </AdminShell>
</template>

<style scoped>
.vehicle-edit__header {
  display: flex;
  align-items: center;
  gap: 12px;
  margin-bottom: 24px;
}
.vehicle-edit__header h2 { font-size: 1.5rem; font-weight: 700; }

.vehicle-edit__layout {
  display: grid;
  grid-template-columns: 1fr 320px;
  gap: 24px;
}

.form-grid {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 16px;
}

.field { display: flex; flex-direction: column; gap: 6px; }
.field label { font-size: 0.75rem; font-weight: 600; color: #9a9a9a; text-transform: uppercase; }

.field-inline {
  display: flex;
  align-items: center;
  gap: 12px;
  margin-top: 16px;
}
.field-inline label { font-size: 0.875rem; }

.form-actions { display: flex; gap: 12px; margin-top: 24px; }

.specs-panel {
  background: #111;
  border: 1px solid #222;
  border-radius: 8px;
  padding: 16px;
  margin-top: 24px;
  margin-bottom: 24px;

  &__header {
    display: flex;
    align-items: center;
    justify-content: space-between;
    margin-bottom: 12px;
    h3 { font-size: 0.9rem; font-weight: 600; }
  }
}

.add-spec-btn {
  font-size: 0.78rem;
  padding: 4px 12px;
  background: #1a1a1a;
  border: 1px solid #333;
  border-radius: 6px;
  color: #f0f0f0;
  cursor: pointer;
  &:hover { border-color: #e63946; }
}

.specs-empty { font-size: 0.8rem; color: #555; }

.specs-list { display: flex; flex-direction: column; gap: 8px; }

.spec-row {
  display: grid;
  grid-template-columns: 1fr 1fr auto;
  gap: 8px;
  align-items: center;
}

.spec-input {
  background: #0f0f0f;
  border: 1px solid #2a2a2a;
  border-radius: 6px;
  padding: 7px 10px;
  color: #f0f0f0;
  font-size: 0.85rem;
  &:focus { outline: none; border-color: #e63946; }
}

.spec-remove {
  width: 28px;
  height: 28px;
  background: none;
  border: 1px solid #333;
  border-radius: 6px;
  color: #555;
  cursor: pointer;
  font-size: 1.1rem;
  line-height: 1;
  display: flex;
  align-items: center;
  justify-content: center;
  &:hover { border-color: #e63946; color: #e63946; }
}

.image-panel {
  background: #111;
  border: 1px solid #222;
  border-radius: 8px;
  padding: 16px;
}
.image-panel h3 { font-size: 0.9rem; font-weight: 600; margin-bottom: 12px; }

.upload-btn {
  display: inline-flex;
  align-items: center;
  gap: 8px;
  padding: 8px 16px;
  background: #1a1a1a;
  border: 1px solid #333;
  border-radius: 6px;
  font-size: 0.8rem;
  cursor: pointer;
  transition: border-color 0.2s;
  margin-bottom: 16px;
}
.upload-btn:hover { border-color: #e63946; }

.image-list { display: flex; flex-direction: column; gap: 8px; }
.image-item {
  display: flex;
  align-items: center;
  gap: 10px;
  padding: 6px;
  background: #0f0f0f;
  border-radius: 6px;
}
.image-item img { width: 60px; height: 45px; object-fit: cover; border-radius: 4px; }
.image-item__actions { display: flex; align-items: center; gap: 8px; margin-left: auto; }
.primary-badge { font-size: 0.65rem; background: #2dc653; color: #000; padding: 2px 6px; border-radius: 999px; font-weight: 700; }
.no-images { color: #555; font-size: 0.875rem; }
</style>
