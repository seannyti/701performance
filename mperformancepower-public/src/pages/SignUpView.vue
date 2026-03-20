<script setup lang="ts">
import { reactive, ref } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth.store'

const router = useRouter()
const auth = useAuthStore()

const form = reactive({ firstName: '', lastName: '', email: '', password: '', confirm: '' })
const loading = ref(false)
const error = ref('')
const verificationSent = ref(false)
const verificationEmail = ref('')

async function handleSubmit() {
  error.value = ''
  if (form.password !== form.confirm) {
    error.value = 'Passwords do not match.'
    return
  }
  if (form.password.length < 8) {
    error.value = 'Password must be at least 8 characters.'
    return
  }
  loading.value = true
  try {
    const needsVerification = await auth.register({
      firstName: form.firstName, lastName: form.lastName,
      email: form.email, password: form.password,
    })
    if (needsVerification) {
      verificationEmail.value = form.email
      verificationSent.value = true
    } else {
      router.push('/')
    }
  } catch (e: any) {
    error.value = e?.response?.data?.message ?? 'Registration failed. Please try again.'
  } finally {
    loading.value = false
  }
}
</script>

<template>
  <div class="auth-page">
    <div class="auth-card">
      <div class="auth-card__logo">
        M Performance <strong>Power</strong>
      </div>

      <!-- Email verification sent state -->
      <template v-if="verificationSent">
        <div class="verify-sent">
          <i class="verify-icon pi pi-envelope" />
          <h1 class="auth-card__heading">Check Your Email</h1>
          <p class="auth-card__sub">
            We sent a verification link to <strong>{{ verificationEmail }}</strong>.
            Click the link to activate your account.
          </p>
          <p class="verify-note">Didn't get it? Check your spam folder. The link expires in 24 hours.</p>
          <RouterLink to="/login" class="verify-login-link">Go to Login</RouterLink>
        </div>
      </template>

      <template v-else>
        <h1 class="auth-card__heading">Create Account</h1>
        <p class="auth-card__sub">Sign up to save favorites and track your inquiries.</p>

        <div v-if="error" class="auth-error">{{ error }}</div>

      <form class="auth-form" @submit.prevent="handleSubmit">
        <div class="form-row">
          <div class="form-group">
            <label for="firstName">First Name</label>
            <input id="firstName" v-model="form.firstName" type="text" placeholder="John" required />
          </div>
          <div class="form-group">
            <label for="lastName">Last Name</label>
            <input id="lastName" v-model="form.lastName" type="text" placeholder="Doe" required />
          </div>
        </div>

        <div class="form-group">
          <label for="email">Email</label>
          <input id="email" v-model="form.email" type="email" placeholder="you@example.com" required />
        </div>

        <div class="form-group">
          <label for="password">Password</label>
          <input id="password" v-model="form.password" type="password" placeholder="••••••••" required />
        </div>

        <div class="form-group">
          <label for="confirm">Confirm Password</label>
          <input id="confirm" v-model="form.confirm" type="password" placeholder="••••••••" required />
        </div>

        <button type="submit" class="btn-primary" :disabled="loading">
          {{ loading ? 'Creating account...' : 'Create Account' }}
        </button>
      </form>

        <p class="auth-card__footer">
          Already have an account?
          <RouterLink to="/login">Sign In</RouterLink>
        </p>
      </template>
    </div>
  </div>
</template>

<style lang="scss" scoped>
@use '@/styles/variables' as *;
@use '@/styles/mixins' as *;

.auth-page {
  min-height: calc(100vh - #{$header-height});
  display: flex;
  align-items: center;
  justify-content: center;
  padding: $space-2xl $space-md;
  background: $color-dark;
}

.auth-card {
  width: 100%;
  max-width: 520px;
  background: $color-surface;
  border: 1px solid $color-border;
  border-radius: 12px;
  padding: $space-xl;

  &__logo {
    font-size: 1.1rem;
    font-weight: 800;
    color: $color-text;
    margin-bottom: $space-xl;

    strong { color: var(--color-primary); }
  }

  &__heading {
    font-size: 1.75rem;
    font-weight: 800;
    color: $color-text;
    margin-bottom: $space-xs;
  }

  &__sub {
    font-size: 0.9rem;
    color: $color-muted;
    margin-bottom: $space-xl;
  }

  &__footer {
    margin-top: $space-lg;
    text-align: center;
    font-size: 0.875rem;
    color: $color-muted;

    a {
      color: var(--color-primary);
      font-weight: 600;
      margin-left: 4px;
      transition: opacity 0.15s;
      &:hover { opacity: 0.8; }
    }
  }
}

.auth-form {
  display: flex;
  flex-direction: column;
  gap: $space-md;
}

.form-row {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: $space-md;
}

.form-group {
  display: flex;
  flex-direction: column;
  gap: 6px;

  label {
    font-size: 0.8rem;
    font-weight: 600;
    color: $color-muted;
    text-transform: uppercase;
    letter-spacing: 0.05em;
  }

  input {
    background: $color-dark;
    border: 1px solid $color-border;
    border-radius: $border-radius;
    padding: $space-sm $space-md;
    color: $color-text;
    font-size: 0.9rem;
    transition: border-color 0.2s;

    &:focus {
      outline: none;
      border-color: var(--color-primary);
    }

    &::placeholder { color: $color-muted; }
  }
}

.auth-error {
  background: rgba(var(--color-primary-rgb), 0.1);
  border: 1px solid var(--color-primary);
  border-radius: $border-radius;
  padding: $space-sm $space-md;
  color: var(--color-primary);
  font-size: 0.875rem;
  margin-bottom: $space-md;
}

.verify-sent {
  display: flex;
  flex-direction: column;
  align-items: center;
  text-align: center;
  gap: $space-md;
  padding: $space-lg 0;
}
.verify-icon {
  font-size: 2.5rem;
  color: var(--color-primary);
}
.verify-note {
  font-size: 0.8rem;
  color: $color-muted;
}
.verify-login-link {
  margin-top: $space-sm;
  color: var(--color-primary);
  font-weight: 600;
  font-size: 0.9rem;
  transition: opacity 0.15s;
  &:hover { opacity: 0.8; }
}

.btn-primary {
  width: 100%;
  padding: $space-md;
  background: var(--color-primary);
  color: #fff;
  font-size: 0.95rem;
  font-weight: 600;
  border-radius: $border-radius;
  border: none;
  cursor: pointer;
  transition: background 0.2s;
  margin-top: $space-xs;

  &:hover:not(:disabled) { background: var(--color-primary-dark); }
  &:disabled { opacity: 0.5; cursor: not-allowed; }
}
</style>
