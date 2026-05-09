<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useToast } from 'primevue/usetoast'
import Button from 'primevue/button'
import InputText from 'primevue/inputtext'
import InputNumber from 'primevue/inputnumber'
import Select from 'primevue/select'
import Textarea from 'primevue/textarea'
import ToggleSwitch from 'primevue/toggleswitch'
import Panel from 'primevue/panel'
import Divider from 'primevue/divider'
import FileUpload from 'primevue/fileupload'
import ProgressSpinner from 'primevue/progressspinner'
import Tag from 'primevue/tag'
import inventoryService, { type Vehicle, type VehicleImage } from '../../services/inventory.service'

const route = useRoute()
const router = useRouter()
const toast = useToast()

const isEdit = computed(() => !!route.params.id)
const vehicleId = computed(() => isEdit.value ? Number(route.params.id) : null)
const loading = ref(false)
const saving = ref(false)
const uploadingImages = ref(false)

const initialType = (route.query.type as string) || 'atv'
const form = ref({
  vin: '',
  year: new Date().getFullYear(),
  make: '',
  model: '',
  trim: '',
  type: initialType,
  condition: 'new',
  color: '',
  mileage: null as number | null,
  cost: 0,
  msrp: 0,
  salePrice: 0,
  status: 'available',
  description: '',
  isFeatured: false,
})

const isGear = computed(() => inventoryService.isGearType(form.value.type))

const images = ref<VehicleImage[]>([])
// VIN decoder
const decodingVin = ref(false)
async function decodeVin() {
  const vin = form.value.vin.trim()
  if (vin.length !== 17) {
    toast.add({ severity: 'warn', summary: 'Enter a full 17-character VIN first', life: 2500 })
    return
  }
  decodingVin.value = true
  try {
    const res = await fetch(`https://vpic.nhtsa.dot.gov/api/vehicles/decodevin/${vin}?format=json`)
    const data = await res.json()
    const get = (varName: string) => data.Results?.find((r: any) => r.Variable === varName)?.Value ?? ''
    const year = parseInt(get('Model Year'))
    const make = get('Make')
    const model = get('Model')
    const trim = get('Trim')
    if (year && year > 1900) form.value.year = year
    if (make) form.value.make = make.charAt(0) + make.slice(1).toLowerCase()
    if (model) form.value.model = model
    if (trim && trim !== 'null' && trim !== '') form.value.trim = trim
    toast.add({ severity: 'success', summary: 'VIN decoded — review and adjust fields as needed', life: 3000 })
  } catch {
    toast.add({ severity: 'error', summary: 'VIN decode failed', life: 3000 })
  } finally {
    decodingVin.value = false
  }
}

const specSheetUrl = ref<string | null>(null)
const specsParsed = ref<{ label: string; value: string }[]>([])
const specModelHint = ref('')
const parsingSpec = ref(false)
const uploadingSpec = ref(false)

const typeOptions = ref(inventoryService.getTypes())
const makeOptions = ref(inventoryService.getMakes().map(m => ({ label: m, value: m })))
const statusOptions = inventoryService.getStatuses()
const allConditionOptions = [
  { label: 'New', value: 'new' },
  { label: 'Used', value: 'used' },
  { label: 'Consignment', value: 'consignment' },
]
const conditionOptions = computed(() =>
  isGear.value ? allConditionOptions.filter(c => c.value !== 'consignment') : allConditionOptions
)

const years = computed(() => {
  const current = new Date().getFullYear() + 1
  return Array.from({ length: 30 }, (_, i) => current - i)
})

const frontGross = computed(() => form.value.salePrice - form.value.cost)

async function load() {
  loading.value = true
  // Load types and makes from settings
  inventoryService.getTypesAsync().then(t => { typeOptions.value = t })
  inventoryService.getMakesAsync().then(m => { makeOptions.value = m.map(x => ({ label: x, value: x })) })

  if (isEdit.value && vehicleId.value) {
    try {
      const vehicle = await inventoryService.getById(vehicleId.value)
      form.value = {
        vin: vehicle.vin ?? '',
        year: vehicle.year,
        make: vehicle.make,
        model: vehicle.model,
        trim: vehicle.trim ?? '',
        type: vehicle.type,
        condition: vehicle.condition,
        color: vehicle.color ?? '',
        mileage: vehicle.mileage ?? null,
        cost: vehicle.cost,
        msrp: vehicle.msrp,
        salePrice: vehicle.salePrice,
        status: vehicle.status,
        description: vehicle.description ?? '',
        isFeatured: vehicle.isFeatured,
      }
      images.value = [...vehicle.images].sort((a, b) => a.sortOrder - b.sortOrder)
      specSheetUrl.value = (vehicle as any).specSheetUrl ?? null
      try {
        const raw = (vehicle as any).specs
        specsParsed.value = raw ? JSON.parse(raw) : []
      } catch { specsParsed.value = [] }
    } catch {
      toast.add({ severity: 'error', summary: 'Failed to load vehicle', life: 3000 })
      router.push('/inventory')
    }
  }
  loading.value = false
}

async function save() {
  if (!form.value.make || !form.value.model || !form.value.year) {
    toast.add({ severity: 'warn', summary: 'Please fill in Year, Make, and Model', life: 3000 })
    return
  }

  saving.value = true
  try {
    const payload = {
      vin: form.value.vin || null,
      year: form.value.year,
      make: form.value.make,
      model: form.value.model,
      trim: form.value.trim || null,
      type: form.value.type,
      condition: form.value.condition,
      color: form.value.color || null,
      mileage: form.value.mileage,
      cost: form.value.cost,
      msrp: form.value.msrp,
      salePrice: form.value.salePrice,
      status: form.value.status,
      description: form.value.description || null,
      specs: specsParsed.value.length ? JSON.stringify(specsParsed.value) : null,
      isFeatured: form.value.isFeatured,
    }

    if (isEdit.value && vehicleId.value) {
      await inventoryService.update(vehicleId.value, payload as any)
      toast.add({ severity: 'success', summary: 'Vehicle updated', life: 2500 })
    } else {
      const created = await inventoryService.create(payload as any)
      toast.add({ severity: 'success', summary: `Vehicle added — Stock #${created.stockNumber}`, life: 3000 })
      router.push(`/inventory/${created.id}/edit`)
    }
  } catch {
    toast.add({ severity: 'error', summary: 'Failed to save vehicle', life: 3000 })
  } finally {
    saving.value = false
  }
}

async function handleFileUpload(event: any) {
  if (!vehicleId.value) {
    toast.add({ severity: 'warn', summary: 'Save the vehicle first before uploading images', life: 3000 })
    return
  }
  const files: File[] = event.files
  if (!files.length) return

  uploadingImages.value = true
  try {
    const uploaded = await inventoryService.uploadImages(vehicleId.value, files)
    images.value.push(...uploaded)
    toast.add({ severity: 'success', summary: `${uploaded.length} image(s) uploaded`, life: 2500 })
  } catch {
    toast.add({ severity: 'error', summary: 'Image upload failed', life: 3000 })
  } finally {
    uploadingImages.value = false
  }
}

async function deleteImage(image: VehicleImage) {
  if (!vehicleId.value) return
  try {
    await inventoryService.deleteImage(vehicleId.value, image.id)
    images.value = images.value.filter(i => i.id !== image.id)
    toast.add({ severity: 'success', summary: 'Image removed', life: 2000 })
  } catch {
    toast.add({ severity: 'error', summary: 'Failed to remove image', life: 3000 })
  }
}

function setPrimary(image: VehicleImage) {
  images.value.forEach(i => i.isPrimary = i.id === image.id)
}

async function handleSpecSheet(event: Event) {
  const input = event.target as HTMLInputElement
  const file = input.files?.[0]
  if (!file) return
  input.value = ''

  // Step 1 — parse with Claude
  parsingSpec.value = true
  try {
    const formData = new FormData()
    formData.append('file', file)
    if (specModelHint.value.trim()) formData.append('modelHint', specModelHint.value.trim())
    const { data: extracted } = await import('../../services/api').then(m =>
      m.default.post('/api/inventory/parse-spec', formData, { headers: { 'Content-Type': 'multipart/form-data' } })
    )

    // Pre-fill core fields
    if (extracted.year) form.value.year = extracted.year
    if (extracted.make) form.value.make = extracted.make
    if (extracted.model) form.value.model = extracted.model
    if (extracted.trim) form.value.trim = extracted.trim
    if (extracted.type) form.value.type = extracted.type
    if (extracted.color) form.value.color = extracted.color
    if (extracted.msrp) form.value.msrp = extracted.msrp
    if (extracted.description) form.value.description = extracted.description

    // Store all extra specs
    if (Array.isArray(extracted.specs) && extracted.specs.length) {
      specsParsed.value = extracted.specs
    }

    const specCount = specsParsed.value.length
    toast.add({ severity: 'success', summary: `Spec sheet parsed — ${specCount} spec${specCount !== 1 ? 's' : ''} extracted. Review fields below.`, life: 4000 })

    // Step 2 — if editing an existing vehicle, also store the PDF
    if (vehicleId.value) {
      uploadingSpec.value = true
      try {
        const fd2 = new FormData()
        fd2.append('file', file)
        const { data: saved } = await import('../../services/api').then(m =>
          m.default.post(`/api/inventory/${vehicleId.value}/spec-sheet`, fd2, { headers: { 'Content-Type': 'multipart/form-data' } })
        )
        specSheetUrl.value = saved.specSheetUrl
      } finally {
        uploadingSpec.value = false
      }
    }
  } catch (e: any) {
    const msg = e?.response?.data?.message ?? 'Failed to parse spec sheet'
    toast.add({ severity: 'error', summary: msg, life: 4000 })
  } finally {
    parsingSpec.value = false
  }
}

onMounted(load)
</script>

<template>
  <div class="inventory-form">
    <!-- Header -->
    <div class="page-header">
      <div class="header-left">
        <Button icon="pi pi-arrow-left" text @click="router.push('/inventory')" />
        <div>
          <h1>{{ isEdit ? (isGear ? 'Edit Item' : 'Edit Vehicle') : (isGear ? 'Add Apparel & Gear' : 'Add Vehicle') }}</h1>
          <p class="page-sub">{{ isGear ? 'Brand, product name, pricing and images for this gear item.' : (isEdit ? 'Update vehicle details and images' : 'Enter vehicle details to add to inventory') }}</p>
        </div>
      </div>
      <div class="header-actions">
        <Button label="Cancel" severity="secondary" outlined @click="router.push('/inventory')" />
        <Button label="Save Vehicle" icon="pi pi-check" :loading="saving" @click="save" />
      </div>
    </div>

    <div v-if="loading" class="loading-center">
      <ProgressSpinner />
    </div>

    <div v-else class="form-layout">
      <!-- Left column -->
      <div class="form-main">

        <!-- General Info -->
        <Panel header="Vehicle Information" toggleable>
          <div class="form-grid">
            <div v-if="!isGear" class="field col-2">
              <label>Year *</label>
              <Select v-model="form.year" :options="years" placeholder="Year" fluid />
            </div>
            <div class="field col-2">
              <label>{{ isGear ? 'Brand *' : 'Make *' }}</label>
              <Select v-model="form.make" :options="makeOptions" optionLabel="label" optionValue="value" :placeholder="isGear ? 'e.g. Bell, HJC, Fox' : 'Select make'" editable fluid />
            </div>
            <div class="field col-2">
              <label>{{ isGear ? 'Product Name *' : 'Model *' }}</label>
              <InputText v-model="form.model" :placeholder="isGear ? 'e.g. Moto-9 Flex Helmet' : 'e.g. Outlander 570'" fluid />
            </div>
            <div v-if="!isGear" class="field col-2">
              <label>Trim</label>
              <InputText v-model="form.trim" placeholder="e.g. XT-P" fluid />
            </div>
            <div class="field col-2">
              <label>Type *</label>
              <Select v-model="form.type" :options="typeOptions" optionLabel="label" optionValue="value" fluid />
            </div>
            <div class="field col-2">
              <label>Condition *</label>
              <Select v-model="form.condition" :options="conditionOptions" optionLabel="label" optionValue="value" fluid />
            </div>
            <div class="field col-2">
              <label>Color</label>
              <InputText v-model="form.color" :placeholder="isGear ? 'e.g. Matte Black' : 'e.g. Camo Green'" fluid />
            </div>
            <div v-if="!isGear" class="field col-2">
              <label>Mileage</label>
              <InputNumber v-model="form.mileage" placeholder="0" :min="0" suffix=" mi" fluid />
            </div>
            <div v-if="!isGear" class="field col-full">
              <label>VIN</label>
              <div class="vin-row">
                <InputText v-model="form.vin" placeholder="17-character VIN" maxlength="17" fluid />
                <Button
                  label="Decode"
                  icon="pi pi-search"
                  size="small"
                  severity="secondary"
                  :loading="decodingVin"
                  @click="decodeVin"
                />
              </div>
            </div>
          </div>
        </Panel>

        <!-- Pricing -->
        <Panel header="Pricing" toggleable>
          <div class="form-grid">
            <div class="field col-3">
              <label>Cost (Invoice)</label>
              <InputNumber v-model="form.cost" mode="currency" currency="USD" locale="en-US" :min="0" fluid />
              <small class="field-hint">What you paid the manufacturer or supplier for this unit. Internal — not shown on the public site.</small>
            </div>
            <div class="field col-3">
              <label>MSRP</label>
              <InputNumber v-model="form.msrp" mode="currency" currency="USD" locale="en-US" :min="0" fluid />
              <small class="field-hint">Manufacturer's Suggested Retail Price. Shown on the public listing as the strike-through &ldquo;original&rdquo; price.</small>
            </div>
            <div class="field col-3">
              <label>Sale Price</label>
              <InputNumber v-model="form.salePrice" mode="currency" currency="USD" locale="en-US" :min="0" fluid />
              <small class="field-hint">The price customers will see and pay. Shown prominently on the public listing.</small>
            </div>
          </div>
          <div class="gross-preview">
            <span>Front-End Gross:</span>
            <span :class="frontGross >= 0 ? 'gross-positive' : 'gross-negative'">
              {{ frontGross.toLocaleString('en-US', { style: 'currency', currency: 'USD' }) }}
            </span>
          </div>
          <div class="pricing-help">
            <i class="pi pi-info-circle" />
            <div>
              <p><strong>Front-End Gross</strong> = Sale Price − Cost. This is your raw profit on the unit before any add-ons, financing reserve, or back-end income.</p>
              <p>Green when profitable, red when you're underwater. Customers never see Cost or Front-End Gross — those are staff-only.</p>
            </div>
          </div>
        </Panel>

        <!-- Description -->
        <Panel header="Description" toggleable>
          <Textarea v-model="form.description" rows="6" placeholder="Vehicle description, features, condition notes..." fluid autoResize />
        </Panel>

        <!-- Specs -->
        <Panel v-if="specsParsed.length" header="Extracted Specifications" toggleable>
          <div class="specs-table">
            <div v-for="spec in specsParsed" :key="spec.label" class="specs-row">
              <span class="specs-label">{{ spec.label }}</span>
              <span class="specs-value">{{ spec.value }}</span>
            </div>
          </div>
          <Button label="Clear Specs" icon="pi pi-trash" text size="small" severity="danger" class="mt-2" @click="specsParsed = []" />
        </Panel>
      </div>

      <!-- Right column -->
      <div class="form-sidebar">

        <!-- Status & Featured -->
        <Panel header="Status">
          <div class="field">
            <label>Status</label>
            <Select v-model="form.status" :options="statusOptions" optionLabel="label" optionValue="value" fluid />
          </div>
          <Divider />
          <div class="featured-toggle">
            <div>
              <p class="toggle-label">Featured on Homepage</p>
              <p class="toggle-sub">Show this unit in the featured section</p>
            </div>
            <ToggleSwitch v-model="form.isFeatured" />
          </div>
        </Panel>

        <!-- Spec Sheet -->
        <Panel header="Spec Sheet">
          <div class="spec-sheet-panel">
            <p class="spec-hint">Upload a manufacturer PDF spec sheet. Claude AI will auto-fill the vehicle fields from it.</p>

            <div class="spec-model-hint">
              <label class="spec-model-label">Model to extract (required for multi-model catalogs)</label>
              <InputText v-model="specModelHint" placeholder="e.g. SW680L PRO" fluid size="small" :disabled="parsingSpec || uploadingSpec" />
            </div>

            <label class="spec-upload-btn" :class="{ loading: parsingSpec || uploadingSpec }">
              <i class="pi" :class="parsingSpec ? 'pi-spin pi-spinner' : 'pi-file-pdf'" />
              {{ parsingSpec ? 'Parsing with AI…' : uploadingSpec ? 'Saving PDF…' : 'Upload & Auto-Fill' }}
              <input type="file" accept="application/pdf" @change="handleSpecSheet" :disabled="parsingSpec || uploadingSpec" />
            </label>

            <a v-if="specSheetUrl" :href="specSheetUrl" target="_blank" class="spec-view-link">
              <i class="pi pi-external-link" /> View Current Spec Sheet
            </a>
            <p v-if="!isEdit && !specSheetUrl" class="spec-note">
              <i class="pi pi-info-circle" /> Save the vehicle first to also store the PDF file.
            </p>
          </div>
        </Panel>

        <!-- Images -->
        <Panel header="Images">
          <template #header>
            <div class="panel-header-custom">
              <span>Images</span>
              <Tag v-if="uploadingImages" value="Uploading..." severity="info" />
            </div>
          </template>

          <div v-if="!isEdit" class="images-note">
            <i class="pi pi-info-circle" />
            Save the vehicle first to enable image upload.
          </div>

          <div v-else>
            <FileUpload
              mode="basic"
              accept="image/*"
              :multiple="true"
              :auto="true"
              :maxFileSize="10000000"
              chooseLabel="Upload Images"
              @select="handleFileUpload"
              class="upload-btn"
            />

            <div v-if="images.length" class="images-grid">
              <div v-for="image in images" :key="image.id" class="image-item" :class="{ primary: image.isPrimary }">
                <img :src="image.thumbnailUrl" :alt="`Image ${image.id}`" />
                <div class="image-actions">
                  <Button
                    v-if="!image.isPrimary"
                    icon="pi pi-star"
                    text
                    rounded
                    size="small"
                    v-tooltip.top="'Set as primary'"
                    @click="setPrimary(image)"
                  />
                  <Button
                    icon="pi pi-trash"
                    text
                    rounded
                    size="small"
                    severity="danger"
                    v-tooltip.top="'Remove'"
                    @click="deleteImage(image)"
                  />
                </div>
                <span v-if="image.isPrimary" class="primary-badge">Primary</span>
              </div>
            </div>

            <p v-else class="no-images">No images uploaded yet.</p>
          </div>
        </Panel>
      </div>
    </div>
  </div>
</template>

<style scoped>
.inventory-form { display: flex; flex-direction: column; gap: 1.5rem; }

.page-header { display: flex; align-items: flex-start; justify-content: space-between; flex-wrap: wrap; gap: 1rem; }
.header-left { display: flex; align-items: center; gap: 0.75rem; }
.page-header h1 { font-size: 1.75rem; font-weight: 800; color: white; margin: 0; }
.page-sub { color: #9e9e9e; font-size: 0.875rem; margin-top: 0.25rem; }
.header-actions { display: flex; gap: 0.75rem; }

.loading-center { display: flex; justify-content: center; padding: 4rem; }

.form-layout { display: grid; grid-template-columns: 1fr 360px; gap: 1.5rem; align-items: start; }
@media (max-width: 1100px) { .form-layout { grid-template-columns: 1fr; } }

.form-main { display: flex; flex-direction: column; gap: 1rem; }
.form-sidebar { display: flex; flex-direction: column; gap: 1rem; }

.form-grid { display: grid; grid-template-columns: repeat(6, 1fr); gap: 1rem; }
.field { display: flex; flex-direction: column; gap: 0.4rem; }
.field label { font-size: 0.8rem; font-weight: 600; color: #ccc; }
.field-hint { font-size: 0.7rem; color: #555; }
.col-2 { grid-column: span 2; }
.col-3 { grid-column: span 3; }
.col-full { grid-column: span 6; }
@media (max-width: 768px) { .col-2, .col-3 { grid-column: span 6; } }

.gross-preview { display: flex; align-items: center; justify-content: space-between; margin-top: 1rem; padding: 0.75rem 1rem; background: #1e1e1e; border-radius: 8px; font-size: 0.875rem; }
.gross-positive { color: #4caf50; font-weight: 700; font-size: 1rem; }
.gross-negative { color: #f44336; font-weight: 700; font-size: 1rem; }

.pricing-help {
  display: flex; gap: 0.65rem; margin-top: 0.75rem; padding: 0.85rem 1rem;
  background: rgba(33, 150, 243, 0.06); border: 1px solid rgba(33, 150, 243, 0.18);
  border-radius: 8px; font-size: 0.78rem; color: #b8c5d0; line-height: 1.5;
}
.pricing-help i { color: #4ea3e0; font-size: 1rem; flex-shrink: 0; margin-top: 1px; }
.pricing-help p { margin: 0; }
.pricing-help p + p { margin-top: 0.4rem; }
.pricing-help strong { color: #fff; font-weight: 600; }

.featured-toggle { display: flex; align-items: center; justify-content: space-between; gap: 1rem; }
.toggle-label { font-weight: 600; color: white; font-size: 0.875rem; }
.toggle-sub { font-size: 0.75rem; color: #9e9e9e; margin-top: 2px; }

.images-note { display: flex; align-items: center; gap: 0.5rem; color: #9e9e9e; font-size: 0.8rem; padding: 0.75rem; background: #1e1e1e; border-radius: 8px; }

/* Spec Sheet */
/* Specs table */
.specs-table { display: flex; flex-direction: column; }
.specs-row { display: flex; justify-content: space-between; gap: 1rem; padding: 0.35rem 0; border-bottom: 1px solid #1e1e1e; font-size: 0.85rem; }
.specs-row:last-child { border-bottom: none; }
.specs-label { color: #9e9e9e; min-width: 140px; }
.specs-value { color: white; font-weight: 500; text-align: right; }
.mt-2 { margin-top: 0.5rem; }

.vin-row { display: flex; gap: 0.5rem; align-items: center; }

.spec-sheet-panel { display: flex; flex-direction: column; gap: 0.75rem; }
.spec-hint { font-size: 0.78rem; color: #9e9e9e; margin: 0; line-height: 1.5; }
.spec-model-hint { display: flex; flex-direction: column; gap: 0.3rem; }
.spec-model-label { font-size: 0.75rem; color: #9e9e9e; font-weight: 500; }
.spec-upload-btn {
  display: flex; align-items: center; justify-content: center; gap: 0.5rem;
  padding: 0.65rem 1rem; border-radius: 8px; cursor: pointer;
  background: rgba(229,57,53,0.1); border: 1px dashed rgba(229,57,53,0.4);
  color: #e53935; font-size: 0.85rem; font-weight: 600;
  transition: all 0.15s;
}
.spec-upload-btn:hover { background: rgba(229,57,53,0.18); border-color: #e53935; }
.spec-upload-btn.loading { opacity: 0.6; cursor: not-allowed; }
.spec-upload-btn input[type="file"] { display: none; }
.spec-view-link { display: flex; align-items: center; gap: 0.4rem; font-size: 0.8rem; color: #4caf50; text-decoration: none; }
.spec-view-link:hover { text-decoration: underline; }
.spec-note { display: flex; align-items: center; gap: 0.4rem; font-size: 0.75rem; color: #666; margin: 0; }

.upload-btn { width: 100%; margin-bottom: 1rem; }

.images-grid { display: grid; grid-template-columns: repeat(3, 1fr); gap: 0.5rem; }

.image-item {
  position: relative;
  border-radius: 8px;
  overflow: hidden;
  border: 2px solid transparent;
  aspect-ratio: 4/3;

  &.primary { border-color: #e53935; }

  img { width: 100%; height: 100%; object-fit: cover; display: block; }
}

.image-actions {
  position: absolute;
  inset: 0;
  background: rgba(0,0,0,0.6);
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 0.25rem;
  opacity: 0;
  transition: opacity 0.15s;

  .image-item:hover & { opacity: 1; }
}

.primary-badge { position: absolute; bottom: 0; left: 0; right: 0; background: #e53935; color: white; font-size: 0.65rem; font-weight: 700; text-align: center; padding: 2px; text-transform: uppercase; letter-spacing: 0.5px; }

.no-images { color: #555; font-size: 0.8rem; text-align: center; padding: 1rem; }

.panel-header-custom { display: flex; align-items: center; justify-content: space-between; width: 100%; }
</style>
