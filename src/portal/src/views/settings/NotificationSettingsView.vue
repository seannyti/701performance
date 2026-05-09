<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useToast } from 'primevue/usetoast'
import Button from 'primevue/button'
import InputText from 'primevue/inputtext'
import Textarea from 'primevue/textarea'
import Panel from 'primevue/panel'
import ToggleSwitch from 'primevue/toggleswitch'
import InputChips from 'primevue/inputchips'
import settingsService from '../../services/settings.service'
import api from '../../services/api'

const router = useRouter()
const toast = useToast()
const saving = ref(false)
const loading = ref(true)
const testingEmail = ref(false)
const testEmailTo = ref('')
const showSmtpPassword = ref(false)

const form = ref({
  // SMTP
  smtp_host: '',
  smtp_port: '587',
  smtp_username: '',
  smtp_password: '',
  smtp_from: '',
  smtp_ssl: false,
  // Recipients
  notification_inquiry_emails: [] as string[],
  // Content
  email_footer: '',
  // Triggers
  notification_new_lead: false,
})

const triggers = [
  { key: 'notification_new_lead', label: 'New Contact Form Inquiry', desc: 'Triggered when a visitor submits the contact form on the public site.', icon: 'pi-user', recipients: 'Inquiry' },
]

function splitEmails(val: string): string[] {
  return val ? val.split(',').map(e => e.trim()).filter(Boolean) : []
}

async function load() {
  loading.value = true
  try {
    const s = await settingsService.getAll()
    form.value.smtp_host     = s['smtp_host']     ?? ''
    form.value.smtp_port     = s['smtp_port']     ?? '587'
    form.value.smtp_username = s['smtp_username'] ?? ''
    form.value.smtp_password = s['smtp_password'] ?? ''
    form.value.smtp_from     = s['smtp_from']     ?? ''
    form.value.smtp_ssl      = s['smtp_ssl'] === 'true'
    form.value.notification_inquiry_emails = splitEmails(s['notification_inquiry_emails'] ?? '')
    form.value.email_footer  = s['email_footer'] ?? ''
    form.value.notification_new_lead = s['notification_new_lead'] === 'true'
  } finally {
    loading.value = false
  }
}

async function save() {
  saving.value = true
  try {
    await settingsService.bulkUpdate({
      smtp_host:     form.value.smtp_host,
      smtp_port:     form.value.smtp_port,
      smtp_username: form.value.smtp_username,
      smtp_password: form.value.smtp_password,
      smtp_from:     form.value.smtp_from,
      smtp_ssl:      String(form.value.smtp_ssl),
      notification_inquiry_emails: form.value.notification_inquiry_emails.join(','),
      email_footer:  form.value.email_footer,
      notification_new_lead: String(form.value.notification_new_lead),
    })
    toast.add({ severity: 'success', summary: 'Email & notification settings saved', life: 2500 })
  } catch {
    toast.add({ severity: 'error', summary: 'Failed to save', life: 3000 })
  } finally {
    saving.value = false
  }
}

async function sendTestEmail() {
  if (!testEmailTo.value) return
  testingEmail.value = true
  try {
    const res = await api.post('/api/settings/test-email', { to: testEmailTo.value })
    toast.add({ severity: 'success', summary: res.data.message, life: 3000 })
  } catch (e: any) {
    toast.add({ severity: 'error', summary: e?.response?.data?.message ?? 'Test failed', life: 4000 })
  } finally {
    testingEmail.value = false
  }
}

onMounted(load)
</script>

<template>
  <div class="email-settings">
    <div class="page-header">
      <div class="header-left">
        <Button icon="pi pi-arrow-left" text @click="router.push('/settings')" />
        <div>
          <h1>Email & Notifications</h1>
          <p class="page-sub">SMTP configuration, notification recipients, and email triggers.</p>
        </div>
      </div>
      <Button label="Save Settings" icon="pi pi-check" :loading="saving" @click="save" />
    </div>

    <div class="settings-layout">
      <div class="settings-col">

      <!-- ── SMTP Configuration ───────────────────────────────── -->
      <Panel header="SMTP Configuration">
        <p class="panel-desc">
          Configure the outbound mail server used for all notification and customer emails.
          These settings are stored securely and take priority over any server environment variables.
        </p>

        <div class="two-col">
          <div class="field">
            <label>SMTP Host</label>
            <InputText v-model="form.smtp_host" placeholder="mail.privateemail.com" fluid />
          </div>
          <div class="field">
            <label>SMTP Port</label>
            <InputText v-model="form.smtp_port" placeholder="465" fluid />
          </div>
        </div>

        <div class="two-col">
          <div class="field">
            <label>Username</label>
            <InputText v-model="form.smtp_username" placeholder="noreply@yourdomain.com" fluid />
          </div>
          <div class="field">
            <label>Password</label>
            <div class="input-with-toggle">
              <InputText
                v-model="form.smtp_password"
                :type="showSmtpPassword ? 'text' : 'password'"
                placeholder="••••••••••••"
                fluid
              />
              <button class="toggle-vis" type="button" @click="showSmtpPassword = !showSmtpPassword">
                <i :class="showSmtpPassword ? 'pi pi-eye-slash' : 'pi pi-eye'" />
              </button>
            </div>
          </div>
        </div>

        <div class="field">
          <label>Reply-From Address <span class="field-hint">(shown as sender on outbound emails)</span></label>
          <InputText v-model="form.smtp_from" placeholder="noreply@yourdomain.com" fluid />
          <small>Leave blank to use the username above as the sender.</small>
        </div>

        <div class="toggle-row">
          <div>
            <div class="toggle-label">Use SSL/TLS</div>
            <div class="toggle-sub">Enable for port 465 (SSL). Leave off for port 587 (STARTTLS).</div>
          </div>
          <ToggleSwitch v-model="form.smtp_ssl" />
        </div>

        <div class="test-email-row">
          <div class="field" style="flex:1;max-width:340px;margin:0">
            <label>Send Test Email</label>
            <InputText v-model="testEmailTo" placeholder="you@example.com" fluid />
          </div>
          <Button
            label="Send Test"
            icon="pi pi-send"
            outlined
            :loading="testingEmail"
            :disabled="!testEmailTo"
            @click="sendTestEmail"
            style="align-self:flex-end"
          />
        </div>
      </Panel>
      </div><!-- /settings-col left -->

      <div class="settings-col">
      <!-- ── Notification Recipients ─────────────────────────── -->
      <Panel header="Notification Recipients">
        <p class="panel-desc">
          Set which inboxes receive internal staff alerts. Type an email and press Enter or comma to add it.
          You can add multiple recipients per category.
        </p>

        <div class="field">
          <label>
            <i class="pi pi-envelope label-icon" />
            Inquiry &amp; Contact Notifications
          </label>
          <InputChips
            v-model="form.notification_inquiry_emails"
            :placeholder="form.notification_inquiry_emails.length === 0 ? 'contact@yourdomain.com' : ''"
            separator=","
            :addOnBlur="true"
            fluid
          />
          <small>Receives alerts for contact form submissions from the public site.</small>
        </div>
      </Panel>

      <!-- ── Email Content ────────────────────────────────────── -->
      <Panel header="Email Content">
        <p class="panel-desc">
          Customize the footer text that appears at the bottom of all outbound emails.
        </p>
        <div class="field">
          <label>Email Footer Text</label>
          <Textarea
            v-model="form.email_footer"
            rows="2"
            placeholder="Thank you for choosing Performance Power Powersports!"
            fluid
          />
        </div>
      </Panel>

      <!-- ── Trigger Toggles ─────────────────────────────────── -->
      <Panel header="Email Triggers">
        <p class="panel-desc">
          Choose which events send a notification email. Each trigger routes to the appropriate
          recipient group configured above.
        </p>

        <div class="triggers-list">
          <div v-for="trigger in triggers" :key="trigger.key" class="trigger-row">
            <div class="trigger-icon">
              <i :class="`pi ${trigger.icon}`" />
            </div>
            <div class="trigger-body">
              <div class="trigger-label">{{ trigger.label }}</div>
              <div class="trigger-desc">{{ trigger.desc }}</div>
            </div>
            <ToggleSwitch v-model="(form as any)[trigger.key]" />
          </div>
        </div>
      </Panel>
      </div><!-- /settings-col right -->

    </div>
  </div>
</template>

<style scoped>
.email-settings { display: flex; flex-direction: column; gap: 1.5rem; }
.page-header { display: flex; align-items: flex-start; justify-content: space-between; flex-wrap: wrap; gap: 1rem; }
.header-left { display: flex; align-items: center; gap: 0.75rem; }
.page-header h1 { font-size: 1.75rem; font-weight: 800; color: white; margin: 0; }
.page-sub { color: #9e9e9e; font-size: 0.875rem; margin-top: 0.25rem; }

.settings-layout { display: grid; grid-template-columns: 1fr 1fr; gap: 1.5rem; align-items: stretch; }
.settings-col { display: flex; flex-direction: column; gap: 1rem; }
.settings-col:first-child :deep(.p-panel) { flex: 1; display: flex; flex-direction: column; }
.settings-col:first-child :deep(.p-panel-content) { flex: 1; }
@media (max-width: 960px) { .settings-layout { grid-template-columns: 1fr; } }
.panel-desc { font-size: 0.8rem; color: #9e9e9e; margin-bottom: 1.25rem; line-height: 1.5; }

.two-col { display: grid; grid-template-columns: 1fr 1fr; gap: 1rem; }
@media (max-width: 600px) { .two-col { grid-template-columns: 1fr; } }

.field { display: flex; flex-direction: column; gap: 0.6rem; margin-bottom: 1.25rem; }
.field:last-child { margin-bottom: 0; }
.field label { font-size: 0.8rem; font-weight: 600; color: #ccc; display: flex; align-items: center; gap: 0.35rem; }
.field small { font-size: 0.72rem; color: #555; }
.field-hint { font-weight: 400; color: #555; }
.label-icon { font-size: 0.7rem; color: #666; }

/* Password toggle */
.input-with-toggle { position: relative; }
.toggle-vis {
  position: absolute; right: 0.625rem; top: 50%; transform: translateY(-50%);
  background: none; border: none; color: #555; cursor: pointer; padding: 0.25rem;
  display: flex; align-items: center;
}
.toggle-vis:hover { color: #aaa; }

/* SSL toggle row */
.toggle-row {
  display: flex; align-items: center; justify-content: space-between;
  padding: 0.875rem 0; border-top: 1px solid #1e1e1e; margin-top: 0.5rem;
}
.toggle-label { font-size: 0.875rem; font-weight: 600; color: #ddd; }
.toggle-sub { font-size: 0.75rem; color: #555; margin-top: 0.15rem; }

/* Test email */
.test-email-row {
  display: flex; align-items: flex-end; gap: 0.75rem;
  border-top: 1px solid #1e1e1e; padding-top: 1rem; margin-top: 0.5rem;
}

/* Triggers */
.triggers-list { display: flex; flex-direction: column; }
.trigger-row {
  display: flex; align-items: center; gap: 1rem;
  padding: 1rem 0; border-bottom: 1px solid #1e1e1e;
}
.trigger-row:last-child { border-bottom: none; }
.trigger-icon {
  width: 36px; height: 36px; border-radius: 8px;
  background: rgba(229, 57, 53, 0.1); color: #e53935;
  display: flex; align-items: center; justify-content: center;
  flex-shrink: 0; font-size: 0.9rem;
}
.trigger-body { flex: 1; }
.trigger-label { font-size: 0.875rem; font-weight: 600; color: white; margin-bottom: 0.2rem; }
.trigger-desc { font-size: 0.75rem; color: #9e9e9e; }
.trigger-tag { font-size: 0.65rem; flex-shrink: 0; }
</style>
