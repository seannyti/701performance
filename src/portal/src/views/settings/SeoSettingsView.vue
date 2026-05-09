<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useToast } from 'primevue/usetoast'
import Button from 'primevue/button'
import InputText from 'primevue/inputtext'
import Textarea from 'primevue/textarea'
import Panel from 'primevue/panel'
import FileUpload from 'primevue/fileupload'
import Divider from 'primevue/divider'
import settingsService from '../../services/settings.service'
import api from '../../services/api'

const router = useRouter()
const toast = useToast()
const saving = ref(false)
const loading = ref(true)

const form = ref({
  seo_title: '',
  seo_description: '',
  seo_og_image: '',
})

const titleLength = computed(() => form.value.seo_title.length)
const descLength = computed(() => form.value.seo_description.length)

async function load() {
  loading.value = true
  try {
    const s = await settingsService.getAll()
    Object.keys(form.value).forEach(key => {
      if (s[key] !== undefined) (form.value as any)[key] = s[key]
    })
  } finally {
    loading.value = false
  }
}

async function save() {
  saving.value = true
  try {
    const updates: Record<string, string> = {}
    Object.entries(form.value).forEach(([k, v]) => { updates[k] = String(v) })
    await settingsService.bulkUpdate(updates)
    toast.add({ severity: 'success', summary: 'SEO settings saved', life: 2500 })
  } catch {
    toast.add({ severity: 'error', summary: 'Failed to save', life: 3000 })
  } finally {
    saving.value = false
  }
}

async function handleOgUpload(event: any) {
  const file: File = event.files[0]
  if (!file) return
  const formData = new FormData()
  formData.append('file', file)
  try {
    const { data } = await api.post('/api/settings/upload/image', formData, {
      headers: { 'Content-Type': 'multipart/form-data' }
    })
    form.value.seo_og_image = data.url
    toast.add({ severity: 'success', summary: 'Image uploaded', life: 2500 })
  } catch {
    toast.add({ severity: 'error', summary: 'Upload failed', life: 3000 })
  }
}

onMounted(load)
</script>

<template>
  <div class="seo-settings">
    <div class="page-header">
      <div class="header-left">
        <Button icon="pi pi-arrow-left" text @click="router.push('/settings')" />
        <div>
          <h1>SEO Settings</h1>
          <p class="page-sub">Meta title, description, and Open Graph image for search engines and social sharing.</p>
        </div>
      </div>
      <Button label="Save Settings" icon="pi pi-check" :loading="saving" @click="save" />
    </div>

    <div class="settings-layout">
      <!-- Left: Inputs -->
      <div class="settings-col">
        <Panel header="Page Meta">
          <div class="field">
            <div class="label-row">
              <label>Meta Title</label>
              <span class="char-count" :class="{ warn: titleLength > 60, good: titleLength > 0 && titleLength <= 60 }">
                {{ titleLength }} / 60
              </span>
            </div>
            <InputText v-model="form.seo_title" placeholder="PerformancePower Powersports" fluid />
            <small>Appears in browser tabs and search results. Ideal: 50–60 characters.</small>
          </div>

          <div class="field">
            <div class="label-row">
              <label>Meta Description</label>
              <span class="char-count" :class="{ warn: descLength > 160, good: descLength > 0 && descLength <= 160 }">
                {{ descLength }} / 160
              </span>
            </div>
            <Textarea
              v-model="form.seo_description"
              placeholder="ATVs, UTVs, Dirt Bikes, Snowmobiles & More. Your local powersports destination."
              rows="3"
              fluid
            />
            <small>Shown in Google search results. Ideal: 120–160 characters.</small>
          </div>
        </Panel>

        <Panel header="Open Graph Image">
          <p class="panel-desc">
            This image appears when your site is shared on Facebook, Twitter/X, LinkedIn, and iMessage.
            Recommended size: <strong>1200 × 630 px</strong>.
          </p>
          <div class="field">
            <label>Image URL</label>
            <InputText v-model="form.seo_og_image" placeholder="https://..." fluid />
          </div>
          <div class="field mt-2">
            <label>Or Upload Image</label>
            <FileUpload
              mode="basic"
              accept="image/*"
              chooseLabel="Upload OG Image"
              :auto="true"
              @select="handleOgUpload"
            />
          </div>
          <div v-if="form.seo_og_image" class="og-preview">
            <img :src="form.seo_og_image" alt="OG Preview" />
          </div>
        </Panel>
      </div>

      <!-- Right: Live SERP Preview -->
      <div class="preview-col">
        <div class="preview-label"><i class="pi pi-search" /> Google Preview</div>
        <div class="serp-preview">
          <div class="serp-url">yourdomain.com</div>
          <div class="serp-title">{{ form.seo_title || 'PerformancePower Powersports' }}</div>
          <div class="serp-desc">{{ form.seo_description || 'ATVs, UTVs, Dirt Bikes, Snowmobiles & More. Your local powersports destination.' }}</div>
        </div>

        <div class="preview-label mt-3"><i class="pi pi-share-alt" /> Social Share Preview</div>
        <div class="og-card">
          <div v-if="form.seo_og_image" class="og-card__image">
            <img :src="form.seo_og_image" alt="OG Image" />
          </div>
          <div v-else class="og-card__placeholder">
            <i class="pi pi-image" />
            <span>No OG image set</span>
          </div>
          <div class="og-card__body">
            <div class="og-card__domain">yourdomain.com</div>
            <div class="og-card__title">{{ form.seo_title || 'PerformancePower Powersports' }}</div>
            <div class="og-card__desc">{{ form.seo_description || 'ATVs, UTVs, Dirt Bikes, Snowmobiles & More.' }}</div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
.seo-settings { display: flex; flex-direction: column; gap: 1.5rem; }
.page-header { display: flex; align-items: flex-start; justify-content: space-between; flex-wrap: wrap; gap: 1rem; }
.header-left { display: flex; align-items: center; gap: 0.75rem; }
.page-header h1 { font-size: 1.75rem; font-weight: 800; color: white; margin: 0; }
.page-sub { color: #9e9e9e; font-size: 0.875rem; margin-top: 0.25rem; }

.settings-layout { display: grid; grid-template-columns: 1fr 400px; gap: 1.5rem; align-items: start; }
@media (max-width: 1100px) { .settings-layout { grid-template-columns: 1fr; } }

.settings-col { display: flex; flex-direction: column; gap: 1rem; }
.preview-col { position: sticky; top: 1.5rem; display: flex; flex-direction: column; gap: 1rem; }

.field { display: flex; flex-direction: column; gap: 0.6rem; margin-bottom: 1.25rem; }
.field:last-child { margin-bottom: 0; }
.label-row { display: flex; justify-content: space-between; align-items: center; }
.field label { font-size: 0.8rem; font-weight: 600; color: #ccc; }
.field small { font-size: 0.75rem; color: #666; }
.char-count { font-size: 0.7rem; color: #666; }
.char-count.warn { color: #f44336; }
.char-count.good { color: #4caf50; }
.mt-2 { margin-top: 0.5rem; }
.mt-3 { margin-top: 0.75rem; }

.panel-desc { font-size: 0.8rem; color: #9e9e9e; margin-bottom: 1rem; line-height: 1.5; }

.og-preview { margin-top: 0.75rem; border-radius: 8px; overflow: hidden; border: 1px solid #2a2a2a; }
.og-preview img { width: 100%; display: block; }

/* SERP preview */
.preview-label { display: flex; align-items: center; gap: 0.5rem; font-size: 0.75rem; font-weight: 600; color: #9e9e9e; text-transform: uppercase; letter-spacing: 0.5px; }
.serp-preview {
  background: #fff;
  border-radius: 10px;
  padding: 1rem 1.25rem;
  font-family: Arial, sans-serif;
}
.serp-url { font-size: 0.75rem; color: #006621; margin-bottom: 0.2rem; }
.serp-title { font-size: 1.1rem; color: #1a0dab; font-weight: 400; margin-bottom: 0.3rem; }
.serp-desc { font-size: 0.8rem; color: #545454; line-height: 1.5; }

/* OG card */
.og-card { border-radius: 10px; overflow: hidden; border: 1px solid #2a2a2a; }
.og-card__image img { width: 100%; display: block; aspect-ratio: 1.91/1; object-fit: cover; }
.og-card__placeholder {
  aspect-ratio: 1.91/1;
  background: #1a1a1a;
  display: flex; flex-direction: column; align-items: center; justify-content: center;
  gap: 0.5rem; color: #444;
  i { font-size: 2rem; }
  span { font-size: 0.75rem; }
}
.og-card__body { background: #1e1e1e; padding: 0.75rem 1rem; }
.og-card__domain { font-size: 0.7rem; color: #666; text-transform: uppercase; margin-bottom: 0.25rem; }
.og-card__title { font-size: 0.9rem; font-weight: 700; color: white; margin-bottom: 0.2rem; }
.og-card__desc { font-size: 0.75rem; color: #9e9e9e; line-height: 1.4; }
</style>
