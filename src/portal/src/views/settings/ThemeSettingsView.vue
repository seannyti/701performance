<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useToast } from 'primevue/usetoast'
import Button from 'primevue/button'
import InputText from 'primevue/inputtext'
import Textarea from 'primevue/textarea'
import Panel from 'primevue/panel'
import ToggleSwitch from 'primevue/toggleswitch'
import FileUpload from 'primevue/fileupload'
import Divider from 'primevue/divider'
import settingsService from '../../services/settings.service'
import api from '../../services/api'

const router = useRouter()
const toast = useToast()
const saving = ref(false)
const loading = ref(true)

const form = ref({
  theme_logo_url: '',
  theme_favicon_url: '',
  theme_primary_color: '#e53935',
  announcement_enabled: false,
  announcement_text: '',
})

// Preset color swatches
const colorPresets = [
  '#e53935', // red (default)
  '#1976d2', // blue
  '#388e3c', // green
  '#f57c00', // orange
  '#7b1fa2', // purple
  '#00838f', // teal
  '#c62828', // dark red
  '#283593', // dark blue
]

async function load() {
  loading.value = true
  try {
    const s = await settingsService.getAll()
    if (s['theme_logo_url'] !== undefined) form.value.theme_logo_url = s['theme_logo_url']
    if (s['theme_favicon_url'] !== undefined) form.value.theme_favicon_url = s['theme_favicon_url']
    if (s['theme_primary_color'] !== undefined) form.value.theme_primary_color = s['theme_primary_color']
    if (s['announcement_enabled'] !== undefined) form.value.announcement_enabled = s['announcement_enabled'] === 'true'
    if (s['announcement_text'] !== undefined) form.value.announcement_text = s['announcement_text']
  } finally {
    loading.value = false
  }
}

async function save() {
  saving.value = true
  try {
    await settingsService.bulkUpdate({
      theme_logo_url: form.value.theme_logo_url,
      theme_favicon_url: form.value.theme_favicon_url,
      theme_primary_color: form.value.theme_primary_color,
      announcement_enabled: String(form.value.announcement_enabled),
      announcement_text: form.value.announcement_text,
    })
    toast.add({ severity: 'success', summary: 'Theme settings saved', life: 2500 })
  } catch {
    toast.add({ severity: 'error', summary: 'Failed to save', life: 3000 })
  } finally {
    saving.value = false
  }
}

async function handleLogoUpload(event: any) {
  const file: File = event.files[0]
  if (!file) return
  const formData = new FormData()
  formData.append('file', file)
  try {
    const { data } = await api.post('/api/settings/upload/logo', formData, {
      headers: { 'Content-Type': 'multipart/form-data' }
    })
    form.value.theme_logo_url = data.url
    toast.add({ severity: 'success', summary: 'Logo uploaded', life: 2500 })
  } catch {
    toast.add({ severity: 'error', summary: 'Upload failed', life: 3000 })
  }
}

async function handleFaviconUpload(event: any) {
  const file: File = event.files[0]
  if (!file) return
  const formData = new FormData()
  formData.append('file', file)
  try {
    const { data } = await api.post('/api/settings/upload/logo', formData, {
      headers: { 'Content-Type': 'multipart/form-data' }
    })
    form.value.theme_favicon_url = data.url
    toast.add({ severity: 'success', summary: 'Favicon uploaded', life: 2500 })
  } catch {
    toast.add({ severity: 'error', summary: 'Upload failed', life: 3000 })
  }
}

onMounted(load)
</script>

<template>
  <div class="theme-settings">
    <div class="page-header">
      <div class="header-left">
        <Button icon="pi pi-arrow-left" text @click="router.push('/settings')" />
        <div>
          <h1>Theme & Branding</h1>
          <p class="page-sub">Logo, favicon, accent color, and announcement banner.</p>
        </div>
      </div>
      <Button label="Save Settings" icon="pi pi-check" :loading="saving" @click="save" />
    </div>

    <div class="settings-layout">
      <div class="settings-col">
        <!-- Logo -->
        <Panel header="Logo">
          <div class="field">
            <label>Logo URL</label>
            <InputText v-model="form.theme_logo_url" placeholder="https://..." fluid />
          </div>
          <div class="field">
            <label>Or Upload Logo</label>
            <FileUpload mode="basic" accept="image/*" chooseLabel="Upload Logo" :auto="true" @select="handleLogoUpload" />
            <small>Recommended: PNG with transparent background, at least 200px wide</small>
          </div>
          <div v-if="form.theme_logo_url" class="logo-preview">
            <img :src="form.theme_logo_url" alt="Logo Preview" />
          </div>
        </Panel>

        <!-- Favicon -->
        <Panel header="Favicon">
          <div class="field">
            <label>Favicon URL</label>
            <InputText v-model="form.theme_favicon_url" placeholder="https://..." fluid />
          </div>
          <div class="field">
            <label>Or Upload Favicon</label>
            <FileUpload mode="basic" accept="image/*" chooseLabel="Upload Favicon" :auto="true" @select="handleFaviconUpload" />
            <small>Recommended: 32×32 or 64×64 PNG/ICO file</small>
          </div>
          <div v-if="form.theme_favicon_url" class="favicon-preview">
            <img :src="form.theme_favicon_url" alt="Favicon Preview" />
            <span>Appears in browser tabs</span>
          </div>
        </Panel>
      </div>

      <div class="settings-col">
        <!-- Primary Color -->
        <Panel header="Accent Color">
          <p class="panel-desc">
            This color is used for buttons, links, badges, and accents across the public website.
          </p>

          <div class="color-section">
            <div class="color-swatches">
              <button
                v-for="color in colorPresets"
                :key="color"
                class="swatch"
                :style="{ background: color }"
                :class="{ active: form.theme_primary_color === color }"
                @click="form.theme_primary_color = color"
                :title="color"
              />
            </div>

            <div class="custom-color">
              <label>Custom Color</label>
              <div class="color-input-row">
                <input type="color" v-model="form.theme_primary_color" class="color-picker" />
                <InputText v-model="form.theme_primary_color" placeholder="#e53935" style="width: 130px;" />
              </div>
            </div>

            <div class="color-live-preview" :style="{ '--accent': form.theme_primary_color }">
              <div class="preview-btn">Browse Inventory</div>
              <div class="preview-badge">ATV</div>
              <div class="preview-link">View details →</div>
            </div>
          </div>
        </Panel>

        <!-- Announcement Banner -->
        <Panel header="Announcement Banner">
          <p class="panel-desc">
            Shows a dismissable banner at the top of the public website.
          </p>

          <div class="field">
            <div class="toggle-row">
              <label>Enable Banner</label>
              <ToggleSwitch v-model="form.announcement_enabled" />
            </div>
          </div>

          <div class="field">
            <label>Banner Text</label>
            <Textarea
              v-model="form.announcement_text"
              placeholder="🎉 Summer Sale — Up to 20% off select ATVs and UTVs!"
              rows="2"
              fluid
              :disabled="!form.announcement_enabled"
            />
          </div>

          <div v-if="form.announcement_enabled && form.announcement_text" class="banner-preview">
            <div class="banner-preview__bar">
              <span>{{ form.announcement_text }}</span>
              <span class="banner-preview__close">✕</span>
            </div>
          </div>
        </Panel>
      </div>
    </div>
  </div>
</template>

<style scoped>
.theme-settings { display: flex; flex-direction: column; gap: 1.5rem; }
.page-header { display: flex; align-items: flex-start; justify-content: space-between; flex-wrap: wrap; gap: 1rem; }
.header-left { display: flex; align-items: center; gap: 0.75rem; }
.page-header h1 { font-size: 1.75rem; font-weight: 800; color: white; margin: 0; }
.page-sub { color: #9e9e9e; font-size: 0.875rem; margin-top: 0.25rem; }

.settings-layout { display: grid; grid-template-columns: 1fr 1fr; gap: 1.5rem; align-items: start; }
@media (max-width: 960px) { .settings-layout { grid-template-columns: 1fr; } }

.settings-col { display: flex; flex-direction: column; gap: 1rem; }

.field { display: flex; flex-direction: column; gap: 0.6rem; margin-bottom: 1.25rem; }
.field:last-child { margin-bottom: 0; }
.field label { font-size: 0.8rem; font-weight: 600; color: #ccc; }
.field small { font-size: 0.75rem; color: #666; }

.panel-desc { font-size: 0.8rem; color: #9e9e9e; margin-bottom: 1rem; line-height: 1.5; }

/* Logo / Favicon */
.logo-preview { margin-top: 0.75rem; padding: 1rem; background: #1a1a1a; border-radius: 8px; border: 1px solid #2a2a2a; }
.logo-preview img { max-height: 60px; max-width: 100%; display: block; }
.favicon-preview { display: flex; align-items: center; gap: 0.75rem; margin-top: 0.75rem; }
.favicon-preview img { width: 32px; height: 32px; border-radius: 4px; }
.favicon-preview span { font-size: 0.75rem; color: #666; }

/* Color */
.color-section { display: flex; flex-direction: column; gap: 1rem; }
.color-swatches { display: flex; flex-wrap: wrap; gap: 0.5rem; }
.swatch {
  width: 32px; height: 32px; border-radius: 50%; border: 3px solid transparent;
  cursor: pointer; transition: transform 0.1s, border-color 0.1s;
}
.swatch:hover { transform: scale(1.15); }
.swatch.active { border-color: white; }

.custom-color { display: flex; flex-direction: column; gap: 0.35rem; }
.custom-color label { font-size: 0.8rem; font-weight: 600; color: #ccc; }
.color-input-row { display: flex; align-items: center; gap: 0.5rem; }
.color-picker { width: 44px; height: 36px; border: 1px solid #3a3a3a; border-radius: 6px; background: #1a1a1a; cursor: pointer; padding: 2px; }

.color-live-preview {
  display: flex; align-items: center; gap: 0.75rem;
  padding: 0.875rem 1rem;
  background: #0f0f0f;
  border-radius: 8px;
  border: 1px solid #2a2a2a;
}
.preview-btn {
  background: var(--accent);
  color: white;
  font-size: 0.75rem;
  font-weight: 700;
  padding: 0.4rem 0.875rem;
  border-radius: 6px;
}
.preview-badge {
  background: color-mix(in srgb, var(--accent) 15%, transparent);
  color: var(--accent);
  font-size: 0.65rem;
  font-weight: 700;
  padding: 0.2rem 0.6rem;
  border-radius: 20px;
  text-transform: uppercase;
}
.preview-link {
  color: var(--accent);
  font-size: 0.75rem;
  font-weight: 600;
}

/* Toggle row */
.toggle-row { display: flex; align-items: center; justify-content: space-between; }

/* Banner preview */
.banner-preview { margin-top: 0.5rem; }
.banner-preview__bar {
  display: flex; align-items: center; justify-content: space-between;
  background: #e53935;
  color: white;
  font-size: 0.8rem;
  font-weight: 600;
  padding: 0.5rem 1rem;
  border-radius: 6px;
}
.banner-preview__close { opacity: 0.7; cursor: pointer; }
</style>
