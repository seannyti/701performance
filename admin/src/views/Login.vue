<template>
  <div class="login-container" style="min-height: 100vh; display: flex; align-items: center; justify-content: center; background: #f8fafc;">
    <div class="card" style="width: 100%; max-width: 400px;">
      <div class="card-header text-center">
        <h1 class="card-title">🏍️ Admin Login</h1>
        <p class="text-muted">Powersports Admin Dashboard</p>
      </div>
      <div class="card-body">
        <form @submit.prevent="handleLogin">
          <div class="form-group">
            <label class="form-label" for="email">Email</label>
            <input
              id="email"
              type="email"
              v-model="email"
              class="form-control"
              :class="{ error: emailError }"
              required
            />
            <div v-if="emailError" class="text-danger" style="font-size: 0.875rem; margin-top: 0.25rem;">
              {{ emailError }}
            </div>
          </div>

          <div class="form-group">
            <label class="form-label" for="password">Password</label>
            <input
              id="password"
              type="password"
              v-model="password"
              class="form-control"
              :class="{ error: passwordError }"
              required
            />
            <div v-if="passwordError" class="text-danger" style="font-size: 0.875rem; margin-top: 0.25rem;">
              {{ passwordError }}
            </div>
          </div>

          <button type="submit" class="btn" style="width: 100%;" :disabled="loading">
            {{ loading ? 'Signing In...' : 'Sign In' }}
          </button>

          <div v-if="error" class="text-danger text-center mt-4">
            {{ error }}
          </div>
        </form>

        <div class="text-center mt-6">
          <p class="text-muted" style="font-size: 0.875rem;">
            <a href="http://localhost:5174" style="color: #4f46e5;">← Back to Main Site</a>
          </p>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import { useToast } from '@/composables/useToast'

const router = useRouter()
const authStore = useAuthStore()
const toast = useToast()

const email = ref('')
const password = ref('')
const error = ref('')
const emailError = ref('')
const passwordError = ref('')
const loading = ref(false)

// Check for logout notification
onMounted(() => {
  const logoutReason = localStorage.getItem('auth_logout_reason')
  if (logoutReason) {
    toast.warning(logoutReason)
    localStorage.removeItem('auth_logout_reason')
  }
})

const validateForm = () => {
  emailError.value = ''
  passwordError.value = ''
  
  if (!email.value) {
    emailError.value = 'Email is required'
    return false
  }
  
  if (!password.value) {
    passwordError.value = 'Password is required'
    return false
  }
  
  return true
}

const handleLogin = async () => {
  if (!validateForm()) return
  
  loading.value = true
  error.value = ''
  
  try {
    await authStore.login(email.value, password.value)
    router.push('/')
  } catch (err: any) {
    error.value = err.message || 'Login failed'
  } finally {
    loading.value = false
  }
}
</script>