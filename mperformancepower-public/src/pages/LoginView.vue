<script setup lang="ts">
import { reactive, ref } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth.store'

const router = useRouter()
const auth = useAuthStore()

const form = reactive({ email: '', password: '' })
const loading = ref(false)
const error = ref('')

async function handleSubmit() {
  loading.value = true
  error.value = ''
  try {
    await auth.login(form)
    router.push('/')
  } catch {
    error.value = 'Invalid email or password.'
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

      <h1 class="auth-card__heading">Sign In</h1>
      <p class="auth-card__sub">Welcome back. Sign in to your account.</p>

      <div v-if="error" class="auth-error">{{ error }}</div>

      <form class="auth-form" @submit.prevent="handleSubmit">
        <div class="form-group">
          <label for="email">Email</label>
          <input id="email" v-model="form.email" type="email" placeholder="you@example.com" required />
        </div>

        <div class="form-group">
          <label for="password">Password</label>
          <input id="password" v-model="form.password" type="password" placeholder="••••••••" required />
        </div>

        <button type="submit" class="btn-primary" :disabled="loading">
          {{ loading ? 'Signing in...' : 'Sign In' }}
        </button>
      </form>

      <p class="auth-card__footer">
        Don't have an account?
        <RouterLink to="/signup">Sign Up</RouterLink>
      </p>
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
  max-width: 420px;
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
