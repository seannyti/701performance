<script setup lang="ts">
import { ref, computed, onMounted, watch } from 'vue'
import { useRouter } from 'vue-router'
import { useToast } from 'primevue/usetoast'
import Button from 'primevue/button'
import InputText from 'primevue/inputtext'
import InputNumber from 'primevue/inputnumber'
import Panel from 'primevue/panel'
import Slider from 'primevue/slider'
import SelectButton from 'primevue/selectbutton'
import FileUpload from 'primevue/fileupload'
import Divider from 'primevue/divider'
import settingsService from '../../services/settings.service'
import api from '../../services/api'

const router = useRouter()
const toast = useToast()

const saving = ref(false)
const loading = ref(true)

// Hero type
const heroTypeOptions = [
  { label: 'None', value: 'none' },
  { label: 'YouTube', value: 'youtube' },
  { label: 'MP4 Video', value: 'mp4' },
  { label: 'Image', value: 'image' },
]

const form = ref({
  hero_type: 'none',
  hero_youtube_url: '',
  hero_start_time: 0,
  hero_overlay_opacity: 50,
  hero_title: 'Your Powersports Destination',
  hero_title_accent: 'Destination',
  hero_subtitle: 'ATVs, UTVs, Dirt Bikes, Snowmobiles & More',
  hero_btn1_label: 'Browse Inventory',
  hero_btn1_link: '/inventory',
  hero_btn2_label: 'Contact Us',
  hero_btn2_link: '/contact',
  hero_video_url: '',
  hero_image_url: '',
})

// Extract YouTube video ID for preview
const youtubeVideoId = computed(() => {
  const url = form.value.hero_youtube_url
  const match = url.match(/(?:v=|youtu\.be\/)([^&\s]+)/)
  return match ? match[1] : ''
})

const youtubeEmbedUrl = computed(() =>
  youtubeVideoId.value
    ? `https://www.youtube.com/embed/${youtubeVideoId.value}?autoplay=1&mute=1&loop=1&controls=0&playlist=${youtubeVideoId.value}&start=${form.value.hero_start_time}&disablekb=1&modestbranding=1&rel=0`
    : ''
)

const overlayStyle = computed(() => ({
  opacity: form.value.hero_overlay_opacity / 100,
}))

// Title accent preview
const titleParts = computed(() => {
  const t = form.value.hero_title
  const a = form.value.hero_title_accent
  if (!a || !t.includes(a)) return { before: t, accent: '', after: '' }
  const idx = t.indexOf(a)
  return { before: t.substring(0, idx), accent: a, after: t.substring(idx + a.length) }
})

async function load() {
  loading.value = true
  try {
    const settings = await settingsService.getAll()
    Object.keys(form.value).forEach(key => {
      if (settings[key] !== undefined) {
        const val = settings[key]
        if (key === 'hero_start_time' || key === 'hero_overlay_opacity') {
          (form.value as any)[key] = Number(val)
        } else {
          (form.value as any)[key] = val
        }
      }
    })
  } finally {
    loading.value = false
  }
}

async function save() {
  saving.value = true
  try {
    const updates: Record<string, string> = {}
    Object.entries(form.value).forEach(([k, v]) => {
      updates[k] = String(v)
    })
    await settingsService.bulkUpdate(updates)
    toast.add({ severity: 'success', summary: 'Hero settings saved', life: 2500 })
  } catch {
    toast.add({ severity: 'error', summary: 'Failed to save settings', life: 3000 })
  } finally {
    saving.value = false
  }
}

function clearYoutubeUrl() {
  form.value.hero_youtube_url = ''
}

async function handleVideoUpload(event: any) {
  const file: File = event.files[0]
  if (!file) return
  const formData = new FormData()
  formData.append('file', file)
  try {
    const { data } = await api.post('/api/settings/upload/video', formData, {
      headers: { 'Content-Type': 'multipart/form-data' }
    })
    form.value.hero_video_url = data.url
    toast.add({ severity: 'success', summary: 'Video uploaded', life: 2500 })
  } catch {
    toast.add({ severity: 'error', summary: 'Upload failed', life: 3000 })
  }
}

async function handleImageUpload(event: any) {
  const file: File = event.files[0]
  if (!file) return
  const formData = new FormData()
  formData.append('file', file)
  try {
    const { data } = await api.post('/api/settings/upload/image', formData, {
      headers: { 'Content-Type': 'multipart/form-data' }
    })
    form.value.hero_image_url = data.url
    toast.add({ severity: 'success', summary: 'Image uploaded', life: 2500 })
  } catch {
    toast.add({ severity: 'error', summary: 'Upload failed', life: 3000 })
  }
}

onMounted(load)
</script>

<template>
  <div class="hero-settings">
    <!-- Header -->
    <div class="page-header">
      <div class="header-left">
        <Button icon="pi pi-arrow-left" text @click="router.push('/settings')" />
        <div>
          <h1>Hero Settings</h1>
          <p class="page-sub">Control what plays in the full-screen hero section.</p>
        </div>
      </div>
      <Button label="Save Settings" icon="pi pi-check" :loading="saving" @click="save" />
    </div>

    <div class="hero-layout">
      <!-- Left: Controls -->
      <div class="controls">

        <!-- Hero Type -->
        <Panel header="Hero Type">
          <SelectButton
            v-model="form.hero_type"
            :options="heroTypeOptions"
            optionLabel="label"
            optionValue="value"
            class="type-selector"
          />
        </Panel>

        <!-- YouTube Controls -->
        <Panel v-if="form.hero_type === 'youtube'" header="YouTube">
          <div class="field">
            <label>YouTube URL</label>
            <InputText v-model="form.hero_youtube_url" placeholder="https://www.youtube.com/watch?v=..." fluid />
          </div>
          <div class="field">
            <label>Start Time (seconds)</label>
            <InputNumber v-model="form.hero_start_time" :min="0" placeholder="0" fluid />
          </div>
          <Button
            label="Clear YouTube URL"
            severity="danger"
            outlined
            fluid
            class="mt-3"
            @click="clearYoutubeUrl"
          />
          <Divider />
          <div class="field">
            <label>Overlay Opacity: {{ form.hero_overlay_opacity }}%</label>
            <Slider v-model="form.hero_overlay_opacity" :min="0" :max="100" class="mt-2" />
          </div>
        </Panel>

        <!-- MP4 Controls -->
        <Panel v-if="form.hero_type === 'mp4'" header="MP4 Video">
          <div class="field">
            <label>Video URL</label>
            <InputText v-model="form.hero_video_url" placeholder="https://..." fluid />
          </div>
          <div class="field mt-3">
            <label>Or Upload Video</label>
            <FileUpload mode="basic" accept="video/*" chooseLabel="Upload MP4" :auto="true" @select="handleVideoUpload" />
          </div>
          <Divider />
          <div class="field">
            <label>Overlay Opacity: {{ form.hero_overlay_opacity }}%</label>
            <Slider v-model="form.hero_overlay_opacity" :min="0" :max="100" class="mt-2" />
          </div>
        </Panel>

        <!-- Image Controls -->
        <Panel v-if="form.hero_type === 'image'" header="Background Image">
          <div class="field">
            <label>Image URL</label>
            <InputText v-model="form.hero_image_url" placeholder="https://..." fluid />
          </div>
          <div class="field mt-3">
            <label>Or Upload Image</label>
            <FileUpload mode="basic" accept="image/*" chooseLabel="Upload Image" :auto="true" @select="handleImageUpload" />
          </div>
          <Divider />
          <div class="field">
            <label>Overlay Opacity: {{ form.hero_overlay_opacity }}%</label>
            <Slider v-model="form.hero_overlay_opacity" :min="0" :max="100" class="mt-2" />
          </div>
        </Panel>

        <!-- Text Content -->
        <Panel header="Hero Text" toggleable>
          <div class="field">
            <label>Headline</label>
            <InputText v-model="form.hero_title" placeholder="Your Powersports Destination" fluid />
          </div>
          <div class="field">
            <label>Accent Word (renders in red)</label>
            <InputText v-model="form.hero_title_accent" placeholder="Destination" fluid />
          </div>
          <div class="field">
            <label>Subtitle</label>
            <InputText v-model="form.hero_subtitle" placeholder="ATVs, UTVs, Dirt Bikes..." fluid />
          </div>
          <Divider />
          <div class="field-row">
            <div class="field">
              <label>Button 1 Label</label>
              <InputText v-model="form.hero_btn1_label" fluid />
            </div>
            <div class="field">
              <label>Button 1 Link</label>
              <InputText v-model="form.hero_btn1_link" fluid />
            </div>
          </div>
          <div class="field-row">
            <div class="field">
              <label>Button 2 Label</label>
              <InputText v-model="form.hero_btn2_label" fluid />
            </div>
            <div class="field">
              <label>Button 2 Link</label>
              <InputText v-model="form.hero_btn2_link" fluid />
            </div>
          </div>
        </Panel>
      </div>

      <!-- Right: Live Preview -->
      <div class="preview-panel">
        <div class="preview-label">
          <i class="pi pi-eye" /> Preview
        </div>
        <div class="preview-hero">
          <!-- Background -->
          <div class="preview-bg">
            <!-- YouTube -->
            <iframe
              v-if="form.hero_type === 'youtube' && youtubeEmbedUrl"
              :src="youtubeEmbedUrl"
              frameborder="0"
              allow="autoplay; encrypted-media"
              class="preview-iframe"
            />
            <!-- Video -->
            <video
              v-else-if="form.hero_type === 'mp4' && form.hero_video_url"
              :src="form.hero_video_url"
              autoplay muted loop playsinline
              class="preview-video"
            />
            <!-- Image -->
            <div
              v-else-if="form.hero_type === 'image' && form.hero_image_url"
              class="preview-image"
              :style="{ backgroundImage: `url(${form.hero_image_url})` }"
            />
            <!-- None -->
            <div v-else class="preview-none">
              <i class="pi pi-image" />
              <span>No background selected</span>
            </div>
          </div>

          <!-- Overlay -->
          <div class="preview-overlay" :style="overlayStyle" />

          <!-- Content -->
          <div class="preview-content">
            <h2 class="preview-title">
              {{ titleParts.before }}<span class="preview-accent">{{ titleParts.accent }}</span>{{ titleParts.after }}
            </h2>
            <p class="preview-subtitle">{{ form.hero_subtitle }}</p>
            <div class="preview-btns">
              <span class="preview-btn preview-btn--primary">{{ form.hero_btn1_label }}</span>
              <span class="preview-btn preview-btn--ghost">{{ form.hero_btn2_label }}</span>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
.hero-settings { display: flex; flex-direction: column; gap: 1.5rem; }

.page-header { display: flex; align-items: flex-start; justify-content: space-between; flex-wrap: wrap; gap: 1rem; }
.header-left { display: flex; align-items: center; gap: 0.75rem; }
.page-header h1 { font-size: 1.75rem; font-weight: 800; color: white; margin: 0; }
.page-sub { color: #9e9e9e; font-size: 0.875rem; margin-top: 0.25rem; }

.hero-layout { display: grid; grid-template-columns: 1fr 480px; gap: 1.5rem; align-items: start; }
@media (max-width: 1200px) { .hero-layout { grid-template-columns: 1fr; } }

.controls { display: flex; flex-direction: column; gap: 1rem; }

.type-selector { width: 100%; }

.field { display: flex; flex-direction: column; gap: 0.6rem; margin-bottom: 1.25rem; }
.field:last-child { margin-bottom: 0; }
.field label { font-size: 0.8rem; font-weight: 600; color: #ccc; }
.field-row { display: grid; grid-template-columns: 1fr 1fr; gap: 1rem; margin-bottom: 1.25rem; }
.field-row:last-child { margin-bottom: 0; }
.field-row .field { margin-bottom: 0; }
.mt-2 { margin-top: 0.5rem; }
.mt-3 { margin-top: 0.75rem; }

/* Preview */
.preview-panel { position: sticky; top: 1.5rem; }
.preview-label { display: flex; align-items: center; gap: 0.5rem; font-size: 0.8rem; font-weight: 600; color: #9e9e9e; margin-bottom: 0.75rem; text-transform: uppercase; letter-spacing: 0.5px; }

.preview-hero {
  position: relative;
  aspect-ratio: 16/9;
  border-radius: 12px;
  overflow: hidden;
  background: #0a0a0a;
  border: 1px solid #2a2a2a;
}

.preview-bg { position: absolute; inset: 0; }

.preview-iframe {
  width: 200%;
  height: 200%;
  top: -50%;
  left: -50%;
  position: absolute;
  pointer-events: none;
}

.preview-video {
  position: absolute; inset: 0; width: 100%; height: 100%; object-fit: cover;
}

.preview-image {
  position: absolute; inset: 0; background-size: cover; background-position: center;
}

.preview-none {
  position: absolute; inset: 0; display: flex; flex-direction: column; align-items: center; justify-content: center; gap: 0.5rem; color: #333;
  i { font-size: 2rem; }
  span { font-size: 0.75rem; }
}

.preview-overlay {
  position: absolute; inset: 0; background: #000; pointer-events: none;
}

.preview-content {
  position: absolute; inset: 0; display: flex; flex-direction: column; align-items: center; justify-content: center; text-align: center; padding: 1.5rem; gap: 0.5rem;
}

.preview-title { font-size: clamp(0.875rem, 2.5vw, 1.25rem); font-weight: 900; color: white; line-height: 1.2; }
.preview-accent { color: #e53935; }
.preview-subtitle { font-size: clamp(0.6rem, 1.2vw, 0.75rem); color: rgba(255,255,255,0.6); }
.preview-btns { display: flex; gap: 0.5rem; margin-top: 0.25rem; }
.preview-btn { padding: 0.3rem 0.875rem; border-radius: 6px; font-size: clamp(0.5rem, 1vw, 0.7rem); font-weight: 700; }
.preview-btn--primary { background: #e53935; color: white; }
.preview-btn--ghost { border: 1px solid rgba(255,255,255,0.5); color: white; }
</style>
