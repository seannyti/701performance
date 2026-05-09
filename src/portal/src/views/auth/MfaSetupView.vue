<script setup lang="ts">
import { ref, onMounted, nextTick } from 'vue'
import { useToast } from 'primevue/usetoast'
import Button from 'primevue/button'
import InputText from 'primevue/inputtext'
import QRCode from 'qrcode'
import api from '../../services/api'

const toast = useToast()

const mfaEnabled = ref(false)
const loading = ref(true)

// Setup flow
const setupStep = ref<'idle' | 'qr'>('idle')
const qrDataUrl = ref('')
const secret = ref('')
const verifyCode = ref('')
const verifying = ref(false)

// Disable flow
const disableCode = ref('')
const disabling = ref(false)
const showDisable = ref(false)

async function loadStatus() {
  try {
    const { data } = await api.get('/api/auth/mfa/status')
    mfaEnabled.value = data.mfaEnabled
  } finally {
    loading.value = false
  }
}

async function startSetup() {
  try {
    const { data } = await api.get('/api/auth/mfa/setup')
    secret.value = data.secret
    qrDataUrl.value = await QRCode.toDataURL(data.otpUri, { width: 200, margin: 1 })
    setupStep.value = 'qr'
  } catch {
    toast.add({ severity: 'error', summary: 'Failed to start MFA setup', life: 3000 })
  }
}

async function verifySetup() {
  if (!verifyCode.value) return
  verifying.value = true
  try {
    await api.post('/api/auth/mfa/enable', { code: verifyCode.value })
    mfaEnabled.value = true
    setupStep.value = 'idle'
    verifyCode.value = ''
    qrDataUrl.value = ''
    toast.add({ severity: 'success', summary: 'MFA enabled — your account is now protected', life: 4000 })
  } catch (e: any) {
    toast.add({ severity: 'error', summary: e?.response?.data?.message ?? 'Invalid code', life: 3000 })
  } finally {
    verifying.value = false
  }
}

async function disableMfa() {
  if (!disableCode.value) return
  disabling.value = true
  try {
    await api.post('/api/auth/mfa/disable', { code: disableCode.value })
    mfaEnabled.value = false
    showDisable.value = false
    disableCode.value = ''
    toast.add({ severity: 'info', summary: 'MFA disabled', life: 3000 })
  } catch (e: any) {
    toast.add({ severity: 'error', summary: e?.response?.data?.message ?? 'Invalid code', life: 3000 })
  } finally {
    disabling.value = false
  }
}

onMounted(loadStatus)
</script>

<template>
  <div class="mfa-page">
    <div class="mfa-header">
      <h1>Two-Factor Authentication</h1>
      <p class="mfa-sub">Protect your account with an authenticator app (Google Authenticator, Authy, etc.)</p>
    </div>

    <div v-if="loading" class="mfa-loading">Loading…</div>

    <div v-else class="mfa-card">
      <!-- Status -->
      <div class="mfa-status" :class="mfaEnabled ? 'enabled' : 'disabled'">
        <i :class="mfaEnabled ? 'pi pi-shield' : 'pi pi-shield'" />
        <span>MFA is currently <strong>{{ mfaEnabled ? 'enabled' : 'disabled' }}</strong></span>
      </div>

      <!-- Enable flow -->
      <template v-if="!mfaEnabled">
        <template v-if="setupStep === 'idle'">
          <p class="mfa-desc">
            When enabled, you will be required to enter a 6-digit code from your authenticator app each time you sign in.
          </p>
          <Button label="Set Up Two-Factor Authentication" icon="pi pi-qrcode" @click="startSetup" />
        </template>

        <template v-else-if="setupStep === 'qr'">
          <p class="mfa-desc">
            <strong>Step 1:</strong> Scan the QR code below with your authenticator app, or enter the secret key manually.
          </p>

          <div class="qr-container">
            <img :src="qrDataUrl" alt="QR Code" class="qr-img" width="200" height="200" />
          </div>

          <div class="secret-block">
            <span class="secret-label">Manual entry key:</span>
            <code class="secret-key">{{ secret }}</code>
          </div>

          <p class="mfa-desc" style="margin-top:1.5rem">
            <strong>Step 2:</strong> Enter the 6-digit code from your app to confirm setup.
          </p>

          <div class="verify-row">
            <InputText v-model="verifyCode" placeholder="000000" maxlength="6" inputmode="numeric" autocomplete="one-time-code" />
            <Button label="Enable MFA" :loading="verifying" @click="verifySetup" />
          </div>

          <button class="mfa-cancel-link" @click="setupStep = 'idle'; verifyCode = ''">Cancel</button>
        </template>
      </template>

      <!-- Disable flow -->
      <template v-else>
        <p class="mfa-desc">
          Your account is protected with two-factor authentication. To disable it, enter your current authenticator code.
        </p>

        <template v-if="!showDisable">
          <Button label="Disable MFA" severity="danger" outlined icon="pi pi-times-circle" @click="showDisable = true" />
        </template>
        <template v-else>
          <div class="verify-row">
            <InputText v-model="disableCode" placeholder="000000" maxlength="6" inputmode="numeric" autocomplete="one-time-code" />
            <Button label="Confirm Disable" severity="danger" :loading="disabling" @click="disableMfa" />
          </div>
          <button class="mfa-cancel-link" @click="showDisable = false; disableCode = ''">Cancel</button>
        </template>
      </template>
    </div>
  </div>
</template>

<style scoped>
.mfa-page {
  padding: 2rem;
  max-width: 600px;
}

.mfa-header h1 {
  font-size: 1.5rem;
  font-weight: 700;
  color: white;
  margin-bottom: 0.25rem;
}

.mfa-sub {
  color: #9e9e9e;
  font-size: 0.875rem;
  margin-bottom: 1.5rem;
}

.mfa-loading { color: #9e9e9e; }

.mfa-card {
  background: #141414;
  border: 1px solid #2a2a2a;
  border-radius: 12px;
  padding: 1.5rem;
  display: flex;
  flex-direction: column;
  gap: 1.25rem;
}

.mfa-status {
  display: flex;
  align-items: center;
  gap: 0.625rem;
  padding: 0.75rem 1rem;
  border-radius: 8px;
  font-size: 0.875rem;
  &.enabled { background: rgba(76,175,80,0.1); color: #4caf50; border: 1px solid rgba(76,175,80,0.25); }
  &.disabled { background: rgba(158,158,158,0.08); color: #9e9e9e; border: 1px solid #2a2a2a; }
}

.mfa-desc {
  font-size: 0.875rem;
  color: #ccc;
  line-height: 1.6;
  margin: 0;
}

.qr-container {
  background: white;
  display: inline-block;
  padding: 12px;
  border-radius: 8px;
}

.qr-img { display: block; }

.secret-block {
  display: flex;
  align-items: center;
  gap: 0.75rem;
  background: #0a0a0a;
  border: 1px solid #333;
  border-radius: 8px;
  padding: 0.75rem 1rem;
  font-size: 0.875rem;
}

.secret-label { color: #9e9e9e; white-space: nowrap; }

.secret-key {
  font-family: monospace;
  color: #e0e0e0;
  word-break: break-all;
  letter-spacing: 0.05em;
}

.verify-row {
  display: flex;
  gap: 0.75rem;
  align-items: center;
}

.mfa-cancel-link {
  background: none;
  border: none;
  color: #9e9e9e;
  font-size: 0.8rem;
  cursor: pointer;
  padding: 0;
  &:hover { color: white; }
}
</style>
