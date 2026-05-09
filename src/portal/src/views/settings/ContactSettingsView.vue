<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useToast } from 'primevue/usetoast'
import Button from 'primevue/button'
import InputText from 'primevue/inputtext'
import Textarea from 'primevue/textarea'
import Panel from 'primevue/panel'
import Divider from 'primevue/divider'
import DataTable from 'primevue/datatable'
import Column from 'primevue/column'
import settingsService from '../../services/settings.service'

const router = useRouter()
const toast = useToast()
const saving = ref(false)
const loading = ref(true)

const form = ref({
  site_name: '',
  site_name_accent: '',
  contact_phone: '',
  contact_email: '',
  contact_address: '',
  social_facebook: '',
  social_instagram: '',
  social_youtube: '',
  maps_embed_url: '',
})

// Business hours as structured data
const daysOfWeek = ['Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday', 'Sunday']
const hours = ref<Record<string, { open: string; close: string; closed: boolean }>>(
  Object.fromEntries(daysOfWeek.map(d => [d, { open: '9:00 AM', close: '6:00 PM', closed: false }]))
)

async function load() {
  loading.value = true
  try {
    const s = await settingsService.getAll()
    Object.keys(form.value).forEach(key => {
      if (s[key] !== undefined) (form.value as any)[key] = s[key]
    })
    if (s['business_hours'] && s['business_hours'] !== '{}') {
      try {
        const parsed = JSON.parse(s['business_hours'])
        Object.keys(parsed).forEach(day => {
          if (hours.value[day]) hours.value[day] = parsed[day]
        })
      } catch { /* keep defaults */ }
    }
  } finally {
    loading.value = false
  }
}

async function save() {
  saving.value = true
  try {
    const updates: Record<string, string> = {}
    Object.entries(form.value).forEach(([k, v]) => { updates[k] = String(v) })
    updates['business_hours'] = JSON.stringify(hours.value)
    await settingsService.bulkUpdate(updates)
    toast.add({ severity: 'success', summary: 'Contact settings saved', life: 2500 })
  } catch {
    toast.add({ severity: 'error', summary: 'Failed to save', life: 3000 })
  } finally {
    saving.value = false
  }
}

onMounted(load)
</script>

<template>
  <div class="contact-settings">
    <!-- Header -->
    <div class="page-header">
      <div class="header-left">
        <Button icon="pi pi-arrow-left" text @click="router.push('/settings')" />
        <div>
          <h1>Contact & Hours</h1>
          <p class="page-sub">Phone, email, address, business hours, social links, and map embed.</p>
        </div>
      </div>
      <Button label="Save Settings" icon="pi pi-check" :loading="saving" @click="save" />
    </div>

    <div class="settings-layout">
      <div class="settings-col">
        <!-- Contact Info -->
        <Panel header="Business Identity">
          <div class="field">
            <label>Business Name</label>
            <InputText v-model="form.site_name" placeholder="PerformancePower" fluid />
            <small>Shown in the navbar, footer, copyright, and About page</small>
          </div>
          <div class="field">
            <label>Accent Word</label>
            <InputText v-model="form.site_name_accent" placeholder="Power" fluid />
            <small>The word within the business name that gets the primary theme color (e.g. "Power" → Performance<strong style="color:#e53935">Power</strong>)</small>
          </div>
        </Panel>

        <Panel header="Contact Information">
          <div class="field">
            <label>Phone Number</label>
            <InputText v-model="form.contact_phone" placeholder="(555) 555-5555" fluid />
          </div>
          <div class="field">
            <label>Email Address</label>
            <InputText v-model="form.contact_email" placeholder="info@example.com" fluid />
          </div>
          <div class="field">
            <label>Street Address</label>
            <Textarea v-model="form.contact_address" placeholder="123 Main St&#10;City, State 12345" rows="2" fluid />
            <small>Displayed in the footer and contact page</small>
          </div>
        </Panel>

        <!-- Social Media -->
        <Panel header="Social Media Links">
          <div class="field">
            <label><i class="pi pi-facebook mr-1" />Facebook URL</label>
            <InputText v-model="form.social_facebook" placeholder="https://facebook.com/yourpage" fluid />
          </div>
          <div class="field">
            <label><i class="pi pi-instagram mr-1" />Instagram URL</label>
            <InputText v-model="form.social_instagram" placeholder="https://instagram.com/yourhandle" fluid />
          </div>
          <div class="field">
            <label><i class="pi pi-youtube mr-1" />YouTube URL</label>
            <InputText v-model="form.social_youtube" placeholder="https://youtube.com/@yourchannel" fluid />
          </div>
        </Panel>

        <!-- Maps Embed -->
        <Panel header="Google Maps Embed">
          <div class="field">
            <label>Embed URL</label>
            <Textarea
              v-model="form.maps_embed_url"
              placeholder="Paste the src URL from Google Maps embed code..."
              rows="3"
              fluid
            />
            <small>In Google Maps: Share → Embed a map → Copy the <em>src</em> value from the iframe tag</small>
          </div>
          <div v-if="form.maps_embed_url" class="map-preview">
            <iframe
              :src="form.maps_embed_url"
              width="100%"
              height="200"
              style="border:0; border-radius: 8px;"
              allowfullscreen
              loading="lazy"
            />
          </div>
        </Panel>
      </div>

      <!-- Business Hours -->
      <div class="settings-col">
        <Panel header="Business Hours">
          <div class="hours-table">
            <div v-for="day in daysOfWeek" :key="day" class="hours-row">
              <div class="hours-day">{{ day }}</div>
              <div v-if="hours[day].closed" class="hours-closed">
                <span class="closed-label">Closed</span>
              </div>
              <div v-else class="hours-times">
                <input
                  v-model="hours[day].open"
                  type="text"
                  class="time-input"
                  placeholder="9:00 AM"
                />
                <span class="hours-sep">–</span>
                <input
                  v-model="hours[day].close"
                  type="text"
                  class="time-input"
                  placeholder="6:00 PM"
                />
              </div>
              <button
                class="closed-toggle"
                :class="{ 'is-closed': hours[day].closed }"
                @click="hours[day].closed = !hours[day].closed"
              >
                {{ hours[day].closed ? 'Open' : 'Close' }}
              </button>
            </div>
          </div>

          <Divider />
          <p class="hours-tip">
            <i class="pi pi-info-circle" />
            Use any time format you prefer (e.g. "9:00 AM", "9am", "9–5"). These are displayed exactly as typed.
          </p>
        </Panel>
      </div>
    </div>
  </div>
</template>

<style scoped>
.contact-settings { display: flex; flex-direction: column; gap: 1.5rem; }
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
.mr-1 { margin-right: 0.25rem; }

.map-preview { margin-top: 0.75rem; }

/* Hours table */
.hours-table { display: flex; flex-direction: column; gap: 0.5rem; }
.hours-row {
  display: flex; align-items: center; gap: 0.75rem;
  padding: 0.5rem 0.75rem;
  border-radius: 8px;
  background: #1a1a1a;
}
.hours-day { width: 90px; font-size: 0.8rem; font-weight: 600; color: #ccc; flex-shrink: 0; }
.hours-times { display: flex; align-items: center; gap: 0.5rem; flex: 1; }
.hours-closed { flex: 1; }
.closed-label { font-size: 0.75rem; color: #666; font-style: italic; }
.time-input {
  width: 80px;
  background: #2a2a2a;
  border: 1px solid #3a3a3a;
  border-radius: 6px;
  padding: 0.3rem 0.5rem;
  color: white;
  font-size: 0.8rem;
  text-align: center;
}
.time-input:focus { outline: none; border-color: #e53935; }
.hours-sep { color: #666; font-size: 0.8rem; }
.closed-toggle {
  font-size: 0.7rem;
  font-weight: 600;
  padding: 0.2rem 0.6rem;
  border-radius: 4px;
  border: 1px solid #3a3a3a;
  background: transparent;
  color: #9e9e9e;
  cursor: pointer;
  transition: all 0.15s;
  flex-shrink: 0;
}
.closed-toggle:hover { border-color: #e53935; color: #e53935; }
.closed-toggle.is-closed { border-color: #4caf50; color: #4caf50; }

.hours-tip { font-size: 0.75rem; color: #666; display: flex; gap: 0.4rem; align-items: flex-start; margin: 0; }
.hours-tip i { margin-top: 0.1rem; flex-shrink: 0; }
</style>
