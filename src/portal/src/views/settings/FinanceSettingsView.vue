<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useToast } from 'primevue/usetoast'
import Button from 'primevue/button'
import InputText from 'primevue/inputtext'
import Textarea from 'primevue/textarea'
import Panel from 'primevue/panel'
import Divider from 'primevue/divider'
import Message from 'primevue/message'
import settingsService from '../../services/settings.service'

const router = useRouter()
const toast = useToast()
const saving = ref(false)
const loading = ref(true)

const form = ref({
  synchrony_url: '',
  lender_synchrony_description: '',
  synchrony_features: 'Fast online application\nCompetitive interest rates\nFlexible repayment terms\nNew & used vehicles',
  octane_embed_url: '',
  lender_octane_description: '',
  octane_features: 'Soft pull pre-qualification\nDecisions in seconds\nPowersports specialists\nNew & used vehicles',
})

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
    toast.add({ severity: 'success', summary: 'Finance settings saved', life: 2500 })
  } catch {
    toast.add({ severity: 'error', summary: 'Failed to save', life: 3000 })
  } finally {
    saving.value = false
  }
}

onMounted(load)
</script>

<template>
  <div class="finance-settings">
    <!-- Header -->
    <div class="page-header">
      <div class="header-left">
        <Button icon="pi pi-arrow-left" text @click="router.push('/settings')" />
        <div>
          <h1>Financing Settings</h1>
          <p class="page-sub">Configure lender cards shown on the public Finance page.</p>
        </div>
      </div>
      <Button label="Save Settings" icon="pi pi-check" :loading="saving" @click="save" />
    </div>

    <Message severity="info" :closable="false" class="mb-3">
      These settings control the text and links on the public <strong>/finance</strong> page. Update the URLs with your actual lender application links.
    </Message>

    <div class="settings-grid">
      <!-- Synchrony -->
      <Panel header="Synchrony" class="lender-panel">
        <div class="lender-preview lender-preview--synchrony">
          <div class="lender-preview__logo">
            <svg viewBox="0 0 32 32" fill="none" width="32" height="32">
              <circle cx="16" cy="16" r="16" fill="#00a19a"/>
              <circle cx="16" cy="16" r="8" stroke="white" stroke-width="2.5" fill="none"/>
              <circle cx="16" cy="16" r="3" fill="white"/>
            </svg>
            <span>Synchrony</span>
          </div>
        </div>

        <div class="field mt-3">
          <label>Apply Now URL</label>
          <InputText v-model="form.synchrony_url" placeholder="https://www.synchrony.com/..." fluid />
          <small>Link customers click to apply with Synchrony</small>
        </div>

        <div class="field">
          <label>Description</label>
          <Textarea
            v-model="form.lender_synchrony_description"
            placeholder="Synchrony offers competitive rates and flexible terms..."
            rows="3"
            fluid
          />
        </div>

        <div class="field">
          <label>Feature Bullets (one per line, max 4)</label>
          <Textarea
            v-model="form.synchrony_features"
            placeholder="Fast online application&#10;Competitive interest rates&#10;Flexible repayment terms&#10;New & used vehicles"
            rows="4"
            fluid
          />
          <small>Each line renders as a checkmark bullet on the finance page</small>
        </div>
      </Panel>

      <!-- Octane -->
      <Panel header="Octane" class="lender-panel">
        <div class="lender-preview lender-preview--octane">
          <div class="lender-preview__logo">
            <svg viewBox="0 0 32 32" fill="none" width="32" height="32">
              <rect width="32" height="32" rx="6" fill="#ff5a1f"/>
              <path d="M10 10h6l6 6-6 6h-6l6-6-6-6z" fill="white"/>
            </svg>
            <span>Octane</span>
          </div>
        </div>

        <div class="field mt-3">
          <label>Apply Now URL / Embed URL</label>
          <InputText v-model="form.octane_embed_url" placeholder="https://www.octane.co/..." fluid />
          <small>Octane pre-qualification link or embed URL</small>
        </div>

        <div class="field">
          <label>Description</label>
          <Textarea
            v-model="form.lender_octane_description"
            placeholder="Octane specializes in powersports lending..."
            rows="3"
            fluid
          />
        </div>

        <div class="field">
          <label>Feature Bullets (one per line, max 4)</label>
          <Textarea
            v-model="form.octane_features"
            placeholder="Soft pull pre-qualification&#10;Decisions in seconds&#10;Powersports specialists&#10;New & used vehicles"
            rows="4"
            fluid
          />
          <small>Each line renders as a checkmark bullet on the finance page</small>
        </div>
      </Panel>
    </div>
  </div>
</template>

<style scoped>
.finance-settings { display: flex; flex-direction: column; gap: 1.5rem; }
.page-header { display: flex; align-items: flex-start; justify-content: space-between; flex-wrap: wrap; gap: 1rem; }
.header-left { display: flex; align-items: center; gap: 0.75rem; }
.page-header h1 { font-size: 1.75rem; font-weight: 800; color: white; margin: 0; }
.page-sub { color: #9e9e9e; font-size: 0.875rem; margin-top: 0.25rem; }
.mb-3 { margin-bottom: 0.75rem; }
.mt-3 { margin-top: 0.75rem; }

.settings-grid { display: grid; grid-template-columns: 1fr 1fr; gap: 1.5rem; }
@media (max-width: 900px) { .settings-grid { grid-template-columns: 1fr; } }

.field { display: flex; flex-direction: column; gap: 0.6rem; margin-bottom: 1.25rem; }
.field:last-child { margin-bottom: 0; }
.field label { font-size: 0.8rem; font-weight: 600; color: #ccc; }
.field small { font-size: 0.75rem; color: #666; }

.lender-preview { padding: 0.75rem 1rem; border-radius: 8px; background: #1e1e1e; border: 1px solid #333; }
.lender-preview__logo { display: flex; align-items: center; gap: 0.75rem; font-size: 1.1rem; font-weight: 700; color: white; }
</style>
