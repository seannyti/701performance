<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRoute } from 'vue-router'
import { verifyEmail } from '@/services/auth.service'

const route = useRoute()

const status = ref<'loading' | 'success' | 'error'>('loading')
const message = ref('')

onMounted(async () => {
  const token = route.query.token as string
  if (!token) {
    status.value = 'error'
    message.value = 'No verification token found in the URL.'
    return
  }
  try {
    await verifyEmail(token)
    status.value = 'success'
  } catch (e: any) {
    status.value = 'error'
    message.value = e?.response?.data?.message ?? 'Verification failed. The link may be invalid or expired.'
  }
})
</script>

<template>
  <div class="verify-page">
    <div class="verify-card">
      <div class="verify-logo">M Performance <strong>Power</strong></div>

      <div v-if="status === 'loading'" class="verify-state">
        <i class="pi pi-spin pi-spinner verify-icon" />
        <p>Verifying your email…</p>
      </div>

      <div v-else-if="status === 'success'" class="verify-state">
        <i class="pi pi-check-circle verify-icon verify-icon--success" />
        <h1>Email Verified!</h1>
        <p>Your email has been verified. You can now log in to your account.</p>
        <RouterLink to="/login" class="verify-btn">Sign In</RouterLink>
      </div>

      <div v-else class="verify-state">
        <i class="pi pi-times-circle verify-icon verify-icon--error" />
        <h1>Verification Failed</h1>
        <p>{{ message }}</p>
        <RouterLink to="/signup" class="verify-btn verify-btn--outline">Back to Sign Up</RouterLink>
      </div>
    </div>
  </div>
</template>

<style lang="scss" scoped>
@use '@/styles/variables' as *;

.verify-page {
  min-height: calc(100vh - #{$header-height});
  display: flex;
  align-items: center;
  justify-content: center;
  padding: $space-2xl $space-md;
  background: $color-dark;
}

.verify-card {
  width: 100%;
  max-width: 440px;
  background: $color-surface;
  border: 1px solid $color-border;
  border-radius: 12px;
  padding: $space-xl;
  text-align: center;
}

.verify-logo {
  font-size: 1rem;
  font-weight: 800;
  color: $color-text;
  margin-bottom: $space-xl;

  strong { color: var(--color-primary); }
}

.verify-state {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: $space-md;
}

.verify-icon {
  font-size: 3rem;
  color: $color-muted;
}
.verify-icon--success { color: #4ade80; }
.verify-icon--error   { color: var(--color-primary); }

.verify-state h1 {
  font-size: 1.4rem;
  font-weight: 800;
  color: $color-text;
}
.verify-state p {
  font-size: 0.9rem;
  color: $color-muted;
  line-height: 1.6;
}

.verify-btn {
  margin-top: $space-sm;
  display: inline-block;
  padding: $space-sm $space-xl;
  background: var(--color-primary);
  color: #fff;
  font-weight: 600;
  font-size: 0.95rem;
  border-radius: $border-radius;
  transition: opacity 0.15s;

  &:hover { opacity: 0.85; }
}

.verify-btn--outline {
  background: transparent;
  border: 1px solid $color-border;
  color: $color-muted;

  &:hover { border-color: var(--color-primary); color: var(--color-primary); }
}
</style>
