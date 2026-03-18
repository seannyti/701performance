<template>
  <AdminLayout>
    <div class="settings-page">
      <h1 class="page-title">🎵 Music Player</h1>

      <div v-if="isLoading && settings.length === 0" class="loading-state">
        <div class="spinner"></div>
        <p>Loading settings...</p>
      </div>

      <div v-else-if="error" class="error-state">
        <p>{{ error }}</p>
        <button @click="loadSettings" class="btn btn-secondary">Retry</button>
      </div>

      <div v-else class="settings-form">
        <div class="settings-card">
          <div class="section-header-collapsible" style="cursor: default;">
            <h3 class="section-header">🎵 Music Player Settings</h3>
          </div>

          <div class="collapsible-content">
            <div class="settings-grid">
              <!-- Enable toggle -->
              <div class="form-group">
                <label class="form-label">Enable Music Player</label>
                <div class="toggle-wrapper">
                  <label class="toggle">
                    <input
                      type="checkbox"
                      :checked="getSetting('music_enabled').value === 'true'"
                      @change="handleToggleChange('music_enabled', $event)"
                    />
                    <span class="toggle-slider"></span>
                  </label>
                  <span class="toggle-label">{{ getSetting('music_enabled').value === 'true' ? 'Enabled' : 'Disabled' }}</span>
                </div>
                <p class="form-hint">Shows a floating 🎵 button in the bottom-right corner of the site. Visitors click it to open the player.</p>
              </div>
            </div>

            <!-- Embed code -->
            <div class="form-group" style="margin-top: 1.25rem;">
              <label class="form-label">Embed Code</label>
              <textarea
                v-model="getSetting('music_embed_code').value"
                class="form-control"
                rows="7"
                placeholder="Paste your Spotify or SoundCloud <iframe> embed code here"
                style="font-family: monospace; font-size: 0.82rem;"
              ></textarea>
              <div class="form-hint">
                <strong>Spotify:</strong> Open a playlist → click <em>•••</em> → Share → Embed playlist → copy the &lt;iframe&gt; code.<br />
                <strong>SoundCloud:</strong> Open a track/playlist → Share → Embed tab → copy the &lt;iframe&gt; code.
              </div>
            </div>

            <!-- Live preview -->
            <div v-if="sanitizedEmbedCode" class="form-group" style="margin-top: 1.25rem;">
              <label class="form-label">Preview</label>
              <div class="music-preview" v-html="sanitizedEmbedCode"></div>
            </div>

            <div class="form-actions" style="margin-top: 1.5rem;">
              <button
                @click="saveSection(['music_enabled', 'music_embed_code'], 'music')"
                class="btn btn-primary"
                :disabled="isActionLoading('save-music')"
              >
                <span class="icon">💾</span>
                {{ isActionLoading('save-music') ? 'Saving...' : 'Save Music Settings' }}
              </button>
            </div>
          </div>
        </div>
      </div>
    </div>
  </AdminLayout>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import DOMPurify from 'dompurify'
import AdminLayout from '@/components/AdminLayout.vue'
import { logDebug, logError } from '@/services/logger'
import { useToast } from '@/composables/useToast'
import { apiGet, apiPut, apiPost } from '@/utils/apiClient'
import { useLoadingState } from '@/composables/useLoadingState'

interface SiteSetting {
  id: number
  key: string
  displayName: string
  value: string
  type: string
  category: string
  sortOrder: number
  isRequired: boolean
}

const toast = useToast()
const { isLoading, executeWithLoading, isActionLoading } = useLoadingState()

const settings = ref<SiteSetting[]>([])
const error = ref('')

const SETTING_METADATA: Record<string, { displayName: string; type: string; category: string; sortOrder: number; defaultValue?: string }> = {
  music_enabled: { displayName: 'Enable Music Player', type: 'Boolean', category: 'Music', sortOrder: 1, defaultValue: 'false' },
  music_embed_code: { displayName: 'Music Embed Code', type: 'TextArea', category: 'Music', sortOrder: 2, defaultValue: '' },
}

const getSetting = (key: string) => {
  return settings.value.find(s => s.key === key) || {
    id: 0, key, value: '', displayName: '', type: 'Text', category: 'Music', sortOrder: 0, isRequired: false
  } as SiteSetting
}

const ALLOWED_EMBED_ORIGINS = ['open.spotify.com', 'w.soundcloud.com']

const sanitizedEmbedCode = computed(() => {
  const raw = getSetting('music_embed_code').value.trim()
  if (!raw) return ''
  const clean = DOMPurify.sanitize(raw, {
    ALLOWED_TAGS: ['iframe'],
    ALLOWED_ATTR: ['src', 'width', 'height', 'frameborder', 'allow', 'allowfullscreen', 'title', 'loading', 'style'],
    ALLOW_DATA_ATTR: false,
  })
  // Strip iframes whose src doesn't match the domain allowlist
  const div = document.createElement('div')
  div.innerHTML = clean
  div.querySelectorAll('iframe').forEach(frame => {
    try {
      const origin = new URL(frame.src).hostname
      if (!ALLOWED_EMBED_ORIGINS.includes(origin)) frame.remove()
    } catch {
      frame.remove()
    }
  })
  return div.innerHTML
})

const handleToggleChange = (key: string, event: Event) => {
  const target = event.target as HTMLInputElement
  getSetting(key).value = target.checked ? 'true' : 'false'
}

const loadSettings = async () => {
  await executeWithLoading(async () => {
    try {
      const data = await apiGet<SiteSetting[]>('/admin/settings')
      settings.value = data
    } catch (err: any) {
      logError('Failed to load settings', err)
      error.value = err.message || 'Failed to load settings'
    }
  })
}

const saveSection = async (settingKeys: string[], sectionName: string) => {
  await executeWithLoading(async () => {
    try {
      const settingsToUpdate = settings.value.filter(s => settingKeys.includes(s.key) && s.id > 0)
      const missingKeys = settingKeys.filter(k => !settings.value.some(s => s.key === k && s.id > 0))

      const allPromises: Promise<any>[] = []

      for (const setting of settingsToUpdate) {
        allPromises.push(apiPut(`/admin/settings/${setting.id}`, { value: String(setting.value) }))
      }

      for (const key of missingKeys) {
        const meta = SETTING_METADATA[key]
        const currentValue = getSetting(key).value || meta?.defaultValue || ''
        allPromises.push(apiPost('/admin/settings', {
          key,
          displayName: meta?.displayName ?? key,
          value: String(currentValue),
          description: '',
          type: meta?.type ?? 'Text',
          category: meta?.category ?? 'Music',
          sortOrder: meta?.sortOrder ?? 0,
          isRequired: false
        }))
      }

      await Promise.all(allPromises)
      toast.saveSuccess('Music settings')
      await loadSettings()
    } catch (err: any) {
      logError('Failed to save music settings', err)
      toast.saveError(err.message || 'Failed to save music settings')
    }
  }, 'save-music')
}

onMounted(loadSettings)
</script>

<style scoped>
.settings-page {
  padding: 2rem;
  max-width: 1000px;
  margin: 0 auto;
}

.page-title {
  font-size: 2rem;
  font-weight: 700;
  color: #1a1a1a;
  margin: 0 0 2rem 0;
}

.loading-state, .error-state {
  text-align: center;
  padding: 4rem 2rem;
}

.settings-form {
  background: #f9fafb;
  border: 1px solid #e5e7eb;
  border-radius: 12px;
  padding: 2rem;
}

.settings-card {
  background: white;
  border: 1px solid #e5e7eb;
  border-radius: 12px;
  padding: 2rem;
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.1);
  transition: box-shadow 0.2s;
}

.settings-card:hover {
  box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1);
}

.section-header {
  font-size: 1.25rem;
  font-weight: 600;
  color: #1f2937;
  margin: 0 0 1.5rem 0;
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.section-header-collapsible {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 0.75rem;
  margin: -0.75rem -1rem 1rem -1rem;
  border-radius: 8px;
}

.collapsible-content {
  animation: slideDown 0.3s ease;
}

@keyframes slideDown {
  from { opacity: 0; transform: translateY(-10px); }
  to { opacity: 1; transform: translateY(0); }
}

.settings-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(280px, 1fr));
  gap: 1.5rem;
}

.form-group {
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
}

.form-label {
  font-weight: 600;
  font-size: 0.875rem;
  color: #374151;
}

.form-control {
  padding: 0.625rem 0.875rem;
  border: 1px solid #d1d5db;
  border-radius: 8px;
  font-size: 0.875rem;
  transition: border-color 0.2s, box-shadow 0.2s;
  background: white;
  color: #111827;
  width: 100%;
  box-sizing: border-box;
}

.form-control:focus {
  outline: none;
  border-color: #4f46e5;
  box-shadow: 0 0 0 3px rgba(79, 70, 229, 0.1);
}

.form-hint {
  font-size: 0.8rem;
  color: #6b7280;
  margin: 0;
  line-height: 1.5;
}

.toggle-wrapper {
  display: flex;
  align-items: center;
  gap: 0.75rem;
}

.toggle {
  position: relative;
  display: inline-block;
  width: 48px;
  height: 26px;
  flex-shrink: 0;
}

.toggle input {
  opacity: 0;
  width: 0;
  height: 0;
}

.toggle-slider {
  position: absolute;
  cursor: pointer;
  inset: 0;
  background: #d1d5db;
  border-radius: 26px;
  transition: 0.3s;
}

.toggle-slider::before {
  content: '';
  position: absolute;
  height: 20px;
  width: 20px;
  left: 3px;
  top: 3px;
  background: white;
  border-radius: 50%;
  transition: 0.3s;
  box-shadow: 0 1px 3px rgba(0,0,0,0.2);
}

.toggle input:checked + .toggle-slider {
  background: #4f46e5;
}

.toggle input:checked + .toggle-slider::before {
  transform: translateX(22px);
}

.toggle-label {
  font-size: 0.875rem;
  color: #374151;
}

.form-actions {
  display: flex;
  gap: 1rem;
}

.btn {
  padding: 0.625rem 1.25rem;
  border-radius: 8px;
  font-weight: 600;
  font-size: 0.875rem;
  border: none;
  cursor: pointer;
  display: inline-flex;
  align-items: center;
  gap: 0.5rem;
  transition: all 0.2s;
}

.btn-primary {
  background: #4f46e5;
  color: white;
}

.btn-primary:hover:not(:disabled) {
  background: #4338ca;
}

.btn-primary:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

.btn-secondary {
  background: #f3f4f6;
  color: #374151;
  border: 1px solid #d1d5db;
}

.icon {
  font-size: 1rem;
}

.music-preview {
  border: 1px solid #e2e8f0;
  border-radius: 12px;
  overflow: hidden;
  background: #000;
}

.music-preview :deep(iframe) {
  display: block;
  width: 100% !important;
}
</style>
