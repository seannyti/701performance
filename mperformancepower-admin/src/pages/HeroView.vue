<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useToast } from 'primevue/usetoast'
import AdminShell from '@/components/layout/AdminShell.vue'
import Button from 'primevue/button'
import SelectButton from 'primevue/selectbutton'
import InputText from 'primevue/inputtext'
import InputNumber from 'primevue/inputnumber'
import Slider from 'primevue/slider'
import FileUpload from 'primevue/fileupload'

const toast = useToast()
const API_URL = import.meta.env.VITE_API_URL as string
const apiBase = API_URL.replace('/api', '')
const saving = ref(false)
const uploading = ref(false)

const hero = ref({
  heroType: 'none' as 'none' | 'youtube' | 'mp4' | 'image',
  youtubeUrl: null as string | null,
  youtubeStartTime: 0,
  overlayOpacity: 0.5,
  videoPath: null as string | null,
  imagePath: null as string | null,
})

const typeOptions = [
  { label: 'None', value: 'none' },
  { label: 'YouTube', value: 'youtube' },
  { label: 'MP4 Video', value: 'mp4' },
  { label: 'Image', value: 'image' },
]

function extractVideoId(url: string | null) {
  if (!url) return ''
  const match = url.match(/(?:v=|youtu\.be\/)([^&\s]+)/)
  return match?.[1] ?? ''
}

const embedUrl = computed(() => {
  const id = extractVideoId(hero.value.youtubeUrl)
  if (!id) return ''
  return `https://www.youtube.com/embed/${id}?autoplay=0&mute=1&loop=1&controls=1&start=${hero.value.youtubeStartTime}&playlist=${id}`
})

const hasPreview = computed(() =>
  (hero.value.heroType === 'youtube' && !!embedUrl.value) ||
  (hero.value.heroType === 'mp4' && !!hero.value.videoPath) ||
  (hero.value.heroType === 'image' && !!hero.value.imagePath)
)

async function load() {
  try {
    const token = localStorage.getItem('mpp_token')
    const res = await fetch(`${API_URL}/hero`, { headers: { Authorization: `Bearer ${token}` } })
    if (res.ok) {
      const data = await res.json()
      Object.assign(hero.value, data)
    }
  } catch {
    toast.add({ severity: 'error', summary: 'Failed to load hero settings', life: 3000 })
  }
}

async function handleUpload(event: { files: File[] }) {
  const file = event.files[0]
  if (!file) return
  uploading.value = true
  try {
    const token = localStorage.getItem('mpp_token')
    const form = new FormData()
    form.append('file', file)
    const res = await fetch(`${API_URL}/hero/upload`, {
      method: 'POST',
      headers: { Authorization: `Bearer ${token}` },
      body: form,
    })
    if (!res.ok) throw new Error()
    const data = await res.json()
    if (file.type.startsWith('video/')) {
      hero.value.videoPath = data.path
    } else {
      hero.value.imagePath = data.path
    }
    toast.add({ severity: 'success', summary: 'File uploaded', life: 3000 })
  } catch {
    toast.add({ severity: 'error', summary: 'Upload failed', life: 3000 })
  } finally {
    uploading.value = false
  }
}

async function save() {
  saving.value = true
  try {
    const token = localStorage.getItem('mpp_token')
    const res = await fetch(`${API_URL}/hero`, {
      method: 'PUT',
      headers: { 'Content-Type': 'application/json', Authorization: `Bearer ${token}` },
      body: JSON.stringify({
        heroType: hero.value.heroType,
        youtubeUrl: hero.value.youtubeUrl,
        youtubeStartTime: hero.value.youtubeStartTime,
        overlayOpacity: hero.value.overlayOpacity,
        videoPath: hero.value.videoPath,
        imagePath: hero.value.imagePath,
      }),
    })
    if (!res.ok) throw new Error()
    toast.add({ severity: 'success', summary: 'Hero settings saved', life: 3000 })
  } catch {
    toast.add({ severity: 'error', summary: 'Error saving', life: 3000 })
  } finally {
    saving.value = false
  }
}

async function clearYoutube() {
  hero.value.youtubeUrl = null
  hero.value.youtubeStartTime = 0
  await save()
}

async function clearVideo() {
  hero.value.videoPath = null
  await save()
}

async function clearImage() {
  hero.value.imagePath = null
  await save()
}

onMounted(load)
</script>

<template>
  <AdminShell>
    <div class="hero-page">
      <div class="page-header">
        <h1>Hero Settings</h1>
        <p>Control what plays in the full-screen hero section.</p>
      </div>

      <div class="hero-grid">
        <!-- Settings panel -->
        <div class="panel">
          <div class="form">
            <div class="field">
              <label>Hero Type</label>
              <SelectButton v-model="hero.heroType" :options="typeOptions" option-label="label" option-value="value" />
            </div>

            <!-- YouTube fields -->
            <div v-show="hero.heroType === 'youtube'" class="sub-fields">
              <div class="field">
                <label>YouTube URL</label>
                <InputText v-model="hero.youtubeUrl" placeholder="https://youtube.com/watch?v=..." style="width:100%" />
              </div>
              <div class="field">
                <label>Start Time (seconds)</label>
                <InputNumber v-model="hero.youtubeStartTime" :min="0" style="width:140px" />
              </div>
              <Button
                v-show="hero.youtubeUrl"
                label="Clear YouTube URL"
                severity="danger"
                size="small"
                outlined
                @click="clearYoutube"
              />
            </div>

            <!-- MP4 fields -->
            <div v-show="hero.heroType === 'mp4'" class="sub-fields">
              <div class="field">
                <label>Upload MP4 Video</label>
                <FileUpload
                  mode="basic"
                  accept="video/mp4,video/*"
                  choose-label="Choose Video"
                  :disabled="uploading"
                  @select="handleUpload"
                />
              </div>
              <div v-show="hero.videoPath" class="field">
                <label>Current File</label>
                <span class="file-path">{{ hero.videoPath }}</span>
                <Button
                  label="Remove Video"
                  severity="danger"
                  size="small"
                  outlined
                  style="width:fit-content; margin-top:4px;"
                  @click="clearVideo"
                />
              </div>
            </div>

            <!-- Image fields -->
            <div v-show="hero.heroType === 'image'" class="sub-fields">
              <div class="field">
                <label>Upload Image</label>
                <FileUpload
                  mode="basic"
                  accept="image/*"
                  choose-label="Choose Image"
                  :disabled="uploading"
                  @select="handleUpload"
                />
              </div>
              <div v-show="hero.imagePath" class="field">
                <label>Current File</label>
                <span class="file-path">{{ hero.imagePath }}</span>
                <Button
                  label="Remove Image"
                  severity="danger"
                  size="small"
                  outlined
                  style="width:fit-content; margin-top:4px;"
                  @click="clearImage"
                />
              </div>
            </div>

            <!-- Overlay opacity -->
            <div class="field">
              <label>Overlay Opacity: {{ Math.round(hero.overlayOpacity * 100) }}%</label>
              <Slider v-model="hero.overlayOpacity" :min="0" :max="1" :step="0.05" style="width:100%" />
            </div>

            <Button label="Save Settings" severity="warn" :loading="saving" @click="save" style="width:100%" />
          </div>
        </div>

        <!-- Preview panel -->
        <div v-show="hasPreview" class="panel preview-panel">
          <div class="field">
            <label>Preview</label>
          </div>

          <div v-show="hero.heroType === 'youtube' && embedUrl" class="preview-wrapper">
            <iframe
              :src="embedUrl"
              frameborder="0"
              allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture"
              allowfullscreen
              class="preview-iframe"
            />
            <div class="preview-overlay" :style="{ opacity: hero.overlayOpacity }" />
          </div>

          <div v-show="hero.heroType === 'mp4' && hero.videoPath">
            <video
              :src="hero.videoPath ? `${apiBase}/uploads/${hero.videoPath}` : ''"
              controls
              muted
              class="preview-media"
            />
          </div>

          <div v-show="hero.heroType === 'image' && hero.imagePath">
            <img
              :src="hero.imagePath ? `${apiBase}/uploads/${hero.imagePath}` : ''"
              class="preview-media"
              alt="Hero image"
            />
          </div>
        </div>
      </div>
    </div>
  </AdminShell>
</template>

<style scoped>
.page-header { margin-bottom: 1.5rem; }
.page-header h1 { font-size: 1.5rem; font-weight: 700; }
.page-header p { font-size: 0.875rem; color: #9a9a9a; margin-top: 4px; }

.hero-grid {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 1.5rem;
  align-items: start;
}

@media (max-width: 900px) {
  .hero-grid { grid-template-columns: 1fr; }
}

.panel { background: #111; border: 1px solid #222; border-radius: 10px; padding: 20px; }
.form { display: flex; flex-direction: column; gap: 1.5rem; }
.sub-fields { display: flex; flex-direction: column; gap: 1rem; }
.field { display: flex; flex-direction: column; gap: 0.5rem; }
.field label { font-size: 0.875rem; color: #94a3b8; }

.file-path { font-size: 0.75rem; color: #64748b; word-break: break-all; }

.preview-panel { display: flex; flex-direction: column; gap: 1rem; }

.preview-wrapper {
  position: relative;
  aspect-ratio: 16/9;
  border-radius: 6px;
  overflow: hidden;
  background: #000;
}

.preview-iframe { width: 100%; height: 100%; border: none; }

.preview-overlay {
  position: absolute;
  inset: 0;
  background: #000;
  pointer-events: none;
}

.preview-media {
  width: 100%;
  border-radius: 6px;
  border: 1px solid #2a2a2a;
}
</style>
