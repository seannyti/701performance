<script setup lang="ts">
import { reactive, ref } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth.store'

const auth = useAuthStore()
const router = useRouter()

const form = reactive({ email: '', password: '' })
const error = ref('')
const loading = ref(false)

async function handleLogin() {
  error.value = ''
  loading.value = true
  try {
    await auth.login(form)
    router.push('/dashboard')
  } catch (e: any) {
    error.value = e?.message ?? 'Invalid credentials.'
  } finally {
    loading.value = false
  }
}
</script>

<template>
  <div class="login-page">
    <div class="login-card">
      <h1 class="login-card__title">M Performance <strong>Power</strong></h1>
      <p class="login-card__sub">Admin Portal</p>

      <div v-if="error" class="login-error">{{ error }}</div>

      <form @submit.prevent="handleLogin" class="login-form">
        <div class="field">
          <label for="email">Email</label>
          <input id="email" v-model="form.email" type="email" required placeholder="admin@example.com" />
        </div>
        <div class="field">
          <label for="password">Password</label>
          <input id="password" v-model="form.password" type="password" required placeholder="••••••••" />
        </div>
        <button type="submit" class="login-btn" :disabled="loading">
          <i class="pi pi-spin pi-spinner" v-if="loading" />
          <i class="pi pi-sign-in" v-else />
          {{ loading ? 'Signing in...' : 'Sign In' }}
        </button>
      </form>
    </div>
  </div>
</template>

<style scoped>
.login-page {
  min-height: 100vh;
  display: flex;
  align-items: center;
  justify-content: center;
  background: #0f0f0f;
}

.login-card {
  background: #111;
  border: 1px solid #222;
  border-radius: 12px;
  padding: 40px;
  width: 100%;
  max-width: 400px;
}

.login-card__title {
  font-size: 1.4rem;
  font-weight: 700;
  text-align: center;
  margin-bottom: 4px;
  color: #f0f0f0;
}
.login-card__title strong { color: #e63946; }

.login-card__sub {
  text-align: center;
  color: #9a9a9a;
  font-size: 0.875rem;
  margin-bottom: 28px;
}

.login-error {
  background: color-mix(in srgb, #e63946 12%, transparent);
  border: 1px solid color-mix(in srgb, #e63946 40%, transparent);
  border-radius: 8px;
  padding: 10px 14px;
  color: #e63946;
  font-size: 0.85rem;
  margin-bottom: 16px;
}

.login-form {
  display: flex;
  flex-direction: column;
  gap: 16px;
}

.field {
  display: flex;
  flex-direction: column;
  gap: 6px;
}

.field label {
  font-size: 0.8rem;
  font-weight: 600;
  color: #9a9a9a;
}

.field input {
  background: #0d0d0d;
  border: 1px solid #2a2a2a;
  border-radius: 8px;
  padding: 10px 14px;
  color: #f0f0f0;
  font-size: 0.9rem;
  width: 100%;
  box-sizing: border-box;
  transition: border-color 0.15s;
}

.field input:focus {
  outline: none;
  border-color: #e63946;
}

.field input::placeholder { color: #444; }

/* Override browser autofill white background */
.field input:-webkit-autofill,
.field input:-webkit-autofill:hover,
.field input:-webkit-autofill:focus {
  -webkit-box-shadow: 0 0 0 1000px #0d0d0d inset;
  -webkit-text-fill-color: #f0f0f0;
  caret-color: #f0f0f0;
}

.login-btn {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 8px;
  width: 100%;
  padding: 11px;
  background: #e63946;
  color: #fff;
  border: none;
  border-radius: 8px;
  font-size: 0.95rem;
  font-weight: 700;
  cursor: pointer;
  transition: opacity 0.2s;
  margin-top: 4px;
}
.login-btn:hover:not(:disabled) { opacity: 0.85; }
.login-btn:disabled { opacity: 0.5; cursor: not-allowed; }
</style>
