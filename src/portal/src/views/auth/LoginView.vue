<script setup lang="ts">
import { ref } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import { useToast } from 'primevue/usetoast'
import { usePortalAuthStore } from '../../stores/auth'
import api from '../../services/api'
import Button from 'primevue/button'
import InputText from 'primevue/inputtext'
import Password from 'primevue/password'
import Card from 'primevue/card'
import Message from 'primevue/message'

const auth = usePortalAuthStore()
const router = useRouter()
const route = useRoute()
const toast = useToast()

const email = ref('')
const password = ref('')
const loading = ref(false)
const error = ref('')

// MFA verify step
const mfaRequired = ref(false)
const pendingMfaToken = ref('')
const mfaCode = ref('')

// MFA setup-required step (forced for sensitive roles with no MFA)
const mfaSetupRequired = ref(false)
const setupOtpUri = ref('')
const setupSecret = ref('')
const setupCode = ref('')
const setupLoading = ref(false)

async function handleLogin() {
  if (!email.value || !password.value) {
    error.value = 'Please enter your email and password.'
    return
  }

  loading.value = true
  error.value = ''

  try {
    const data = await auth.login(email.value, password.value)
    if (data.mfaRequired) {
      pendingMfaToken.value = data.mfaToken
      if (data.mfaSetupRequired) {
        // Role requires MFA — fetch QR code and show setup UI
        await loadMfaSetup(data.mfaToken)
        mfaSetupRequired.value = true
      } else {
        mfaRequired.value = true
      }
      return
    }
    const redirect = route.query.redirect as string || '/'
    router.push(redirect)
  } catch (e: any) {
    error.value = e.response?.data?.message || 'Invalid email or password.'
  } finally {
    loading.value = false
  }
}

async function loadMfaSetup(token: string) {
  const { data } = await api.get(`/api/auth/mfa/setup-required?mfaToken=${encodeURIComponent(token)}`)
  setupOtpUri.value = data.otpUri
  setupSecret.value = data.secret
}

async function handleMfaSetupEnable() {
  if (!setupCode.value || setupCode.value.length !== 6) {
    error.value = 'Enter the 6-digit code from your authenticator app.'
    return
  }
  setupLoading.value = true
  error.value = ''
  try {
    const { data } = await api.post('/api/auth/mfa/enable-required', {
      mfaToken: pendingMfaToken.value,
      code: setupCode.value,
    })
    // MFA now enabled — proceed to normal verify step with new token
    pendingMfaToken.value = data.mfaToken
    mfaSetupRequired.value = false
    mfaRequired.value = true
    mfaCode.value = ''
    error.value = ''
  } catch (e: any) {
    error.value = e.response?.data?.message || 'Invalid code. Please try again.'
  } finally {
    setupLoading.value = false
  }
}

async function handleMfaVerify() {
  if (!mfaCode.value) {
    error.value = 'Please enter your 6-digit code.'
    return
  }
  loading.value = true
  error.value = ''
  try {
    await auth.completeMfaLogin(pendingMfaToken.value, mfaCode.value)
    const redirect = route.query.redirect as string || '/'
    router.push(redirect)
  } catch (e: any) {
    error.value = e.response?.data?.message || 'Invalid code. Please try again.'
  } finally {
    loading.value = false
  }
}
</script>

<template>
  <div class="login-page">
    <div class="login-container">
      <div class="login-brand">
        <h1 class="brand-name">Performance<span class="accent">Power</span></h1>
        <p class="brand-sub">Dealer Management System</p>
      </div>

      <Card class="login-card">
        <template #content>

          <!-- MFA Setup Required Step -->
          <template v-if="mfaSetupRequired">
            <h2 class="login-title">Set Up Two-Factor Authentication</h2>
            <p class="login-sub">Your role requires MFA. Scan the QR code with your authenticator app, then enter the 6-digit code to continue.</p>

            <Message v-if="error" severity="error" :closable="false" class="mb-4">{{ error }}</Message>

            <div v-if="setupOtpUri" class="qr-wrap">
              <img :src="`https://api.qrserver.com/v1/create-qr-code/?size=180x180&data=${encodeURIComponent(setupOtpUri)}`" alt="MFA QR Code" class="qr-img" />
              <p class="qr-manual">Can't scan? Enter this code manually:</p>
              <code class="qr-secret">{{ setupSecret }}</code>
            </div>

            <form @submit.prevent="handleMfaSetupEnable" class="login-form" style="margin-top:1.25rem">
              <div class="field">
                <label for="setup-code">Verification Code</label>
                <InputText id="setup-code" v-model="setupCode" placeholder="000000" maxlength="6" autocomplete="one-time-code" inputmode="numeric" fluid />
              </div>
              <Button type="submit" label="Enable & Continue" :loading="setupLoading" class="login-btn" fluid />
            </form>
          </template>

          <!-- MFA Verify Step -->
          <template v-else-if="mfaRequired">
            <h2 class="login-title">Two-Factor Authentication</h2>
            <p class="login-sub">Enter the 6-digit code from your authenticator app</p>

            <Message v-if="error" severity="error" :closable="false" class="mb-4">
              {{ error }}
            </Message>

            <form @submit.prevent="handleMfaVerify" class="login-form">
              <div class="field">
                <label for="mfa-code">Verification Code</label>
                <InputText
                  id="mfa-code"
                  v-model="mfaCode"
                  placeholder="000000"
                  maxlength="6"
                  autocomplete="one-time-code"
                  inputmode="numeric"
                  fluid
                />
              </div>
              <Button type="submit" label="Verify" :loading="loading" class="login-btn" fluid />
              <button type="button" class="mfa-back-link" @click="mfaRequired = false; mfaCode = ''; error = ''">
                ← Back to login
              </button>
            </form>
          </template>

          <!-- Login Step -->
          <template v-else>
            <h2 class="login-title">Staff Login</h2>
            <p class="login-sub">Sign in to access the portal</p>

            <Message v-if="error" severity="error" :closable="false" class="mb-4">
              {{ error }}
            </Message>

            <form @submit.prevent="handleLogin" class="login-form">
              <div class="field">
                <label for="email">Email</label>
                <InputText
                  id="email"
                  v-model="email"
                  type="email"
                  placeholder="you@example.com"
                  autocomplete="email"
                  fluid
                />
              </div>

              <div class="field">
                <label for="password">Password</label>
                <Password
                  id="password"
                  v-model="password"
                  placeholder="••••••••"
                  :feedback="false"
                  toggleMask
                  fluid
                />
              </div>

              <Button
                type="submit"
                label="Sign In"
                :loading="loading"
                class="login-btn"
                fluid
              />
            </form>
          </template>
        </template>
      </Card>

      <p class="login-footer">
        PerformancePower &copy; {{ new Date().getFullYear() }}
      </p>
    </div>
  </div>
</template>

<style scoped>
.login-page {
  min-height: 100vh;
  background: #0a0a0a;
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 1.5rem;
}

.login-container {
  width: 100%;
  max-width: 420px;
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 2rem;
}

.login-brand {
  text-align: center;
}

.brand-name {
  font-size: 2rem;
  font-weight: 900;
  color: white;
  letter-spacing: -1px;
}

.accent { color: #e53935; }

.brand-sub {
  color: #9e9e9e;
  font-size: 0.875rem;
  margin-top: 0.25rem;
}

.login-card {
  width: 100%;
  background: #141414 !important;
  border: 1px solid #2a2a2a !important;
  border-radius: 12px !important;
}

.login-title {
  font-size: 1.5rem;
  font-weight: 700;
  color: white;
  margin-bottom: 0.25rem;
}

.login-sub {
  color: #9e9e9e;
  font-size: 0.875rem;
  margin-bottom: 1.5rem;
}

.login-form {
  display: flex;
  flex-direction: column;
  gap: 1.25rem;
}

.field {
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
}

.field label {
  font-size: 0.875rem;
  font-weight: 500;
  color: #cccccc;
}

.login-btn {
  margin-top: 0.5rem;
  background: #e53935 !important;
  border-color: #e53935 !important;
}

.login-footer {
  color: #9e9e9e;
  font-size: 0.75rem;
}

.mfa-back-link {
  background: none;
  border: none;
  color: #9e9e9e;
  font-size: 0.8rem;
  cursor: pointer;
  text-align: center;
  padding: 0;
  &:hover { color: white; }
}

.mb-4 { margin-bottom: 1rem; }

.qr-wrap { display: flex; flex-direction: column; align-items: center; gap: 0.5rem; margin: 0.75rem 0; }
.qr-img { border-radius: 8px; border: 3px solid #2a2a2a; background: white; padding: 4px; }
.qr-manual { font-size: 0.75rem; color: #9e9e9e; margin: 0; }
.qr-secret { font-size: 0.72rem; background: #1a1a1a; border: 1px solid #333; border-radius: 4px; padding: 0.3rem 0.6rem; color: #e53935; letter-spacing: 0.05em; word-break: break-all; text-align: center; }
</style>
