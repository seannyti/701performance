<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useToast } from 'primevue/usetoast'
import Button from 'primevue/button'
import Textarea from 'primevue/textarea'
import Panel from 'primevue/panel'
import settingsService from '../../services/settings.service'

const router = useRouter()
const toast = useToast()
const saving = ref(false)
const loading = ref(true)
const content = ref('')

const charCount = computed(() => content.value.length)

async function load() {
  loading.value = true
  try {
    const s = await settingsService.getAll()
    content.value = s['privacy_content'] ?? ''
  } finally {
    loading.value = false
  }
}

async function save() {
  saving.value = true
  try {
    await settingsService.bulkUpdate({ privacy_content: content.value })
    toast.add({ severity: 'success', summary: 'Privacy Policy saved', life: 2500 })
  } catch {
    toast.add({ severity: 'error', summary: 'Failed to save', life: 3000 })
  } finally {
    saving.value = false
  }
}

onMounted(load)
</script>

<template>
  <div class="privacy-settings">
    <div class="page-header">
      <div class="header-left">
        <Button icon="pi pi-arrow-left" text @click="router.push('/settings')" />
        <div>
          <h1>Privacy Policy</h1>
          <p class="page-sub">Edit the Privacy Policy displayed on your public website at <code>/privacy</code>.</p>
        </div>
      </div>
      <Button label="Save" icon="pi pi-check" :loading="saving" @click="save" />
    </div>

    <div class="settings-layout">
      <div class="settings-col">
        <Panel header="Privacy Policy Content">
          <p class="panel-desc">
            Paste or type your full Privacy Policy below. This is required by GLBA for financing dealerships.
            Line breaks and spacing are preserved exactly as entered.
          </p>
          <div class="field">
            <div class="label-row">
              <label>Content</label>
              <span class="char-count">{{ charCount.toLocaleString() }} characters</span>
            </div>
            <Textarea
              v-model="content"
              :rows="30"
              placeholder="Enter your Privacy Policy here..."
              fluid
              class="privacy-textarea"
            />
          </div>
        </Panel>
      </div>

      <div class="preview-col">
        <div class="preview-label"><i class="pi pi-eye" /> Live Preview</div>
        <div class="privacy-preview">
          <div v-if="content" class="privacy-preview__content">{{ content }}</div>
          <div v-else class="privacy-preview__empty">
            <i class="pi pi-shield" />
            <span>Start typing to see a preview</span>
          </div>
        </div>
        <div class="preview-hint">
          <i class="pi pi-info-circle" />
          Visible at <strong>/privacy</strong> on the public site
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
.privacy-settings { display: flex; flex-direction: column; gap: 1.5rem; }
.page-header { display: flex; align-items: flex-start; justify-content: space-between; flex-wrap: wrap; gap: 1rem; }
.header-left { display: flex; align-items: center; gap: 0.75rem; }
.page-header h1 { font-size: 1.75rem; font-weight: 800; color: white; margin: 0; }
.page-sub { color: #9e9e9e; font-size: 0.875rem; margin-top: 0.25rem; }
.page-sub code { background: #1e1e1e; padding: 0.1rem 0.4rem; border-radius: 4px; font-size: 0.8rem; color: #e53935; }

.settings-layout { display: grid; grid-template-columns: 1fr 380px; gap: 1.5rem; align-items: start; }
@media (max-width: 1100px) { .settings-layout { grid-template-columns: 1fr; } }

.settings-col { display: flex; flex-direction: column; gap: 1rem; }
.preview-col { position: sticky; top: 1.5rem; display: flex; flex-direction: column; gap: 0.75rem; }

.panel-desc { font-size: 0.8rem; color: #9e9e9e; margin-bottom: 1rem; line-height: 1.5; }

.field { display: flex; flex-direction: column; gap: 0.6rem; margin-bottom: 1.25rem; }
.field:last-child { margin-bottom: 0; }
.label-row { display: flex; justify-content: space-between; align-items: center; }
.field label { font-size: 0.8rem; font-weight: 600; color: #ccc; }
.char-count { font-size: 0.7rem; color: #666; }

.privacy-textarea { font-family: inherit; font-size: 0.8125rem; line-height: 1.7; resize: vertical; }

.preview-label { display: flex; align-items: center; gap: 0.5rem; font-size: 0.75rem; font-weight: 600; color: #9e9e9e; text-transform: uppercase; letter-spacing: 0.5px; }

.privacy-preview {
  background: #141414;
  border: 1px solid #2a2a2a;
  border-radius: 10px;
  padding: 1.25rem;
  max-height: 500px;
  overflow-y: auto;
}

.privacy-preview__content {
  color: #bbb;
  font-size: 0.8rem;
  line-height: 1.8;
  white-space: pre-wrap;
  font-family: inherit;
}

.privacy-preview__empty {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: 0.75rem;
  padding: 2rem;
  color: #444;
  text-align: center;
  i { font-size: 2rem; }
  span { font-size: 0.8rem; }
}

.preview-hint {
  display: flex;
  align-items: center;
  gap: 0.4rem;
  font-size: 0.75rem;
  color: #555;
  i { font-size: 0.7rem; }
  strong { color: #777; }
}
</style>
