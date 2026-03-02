<template>
  <div class="login-container">
    <div class="login-card">
      <div class="login-header">
        <h1>Welcome Back</h1>
        <p>Sign in to your Powersports account</p>
      </div>

      <form @submit.prevent="handleLogin" class="login-form">
        <div class="form-group">
          <label for="email">Email Address</label>
          <input
            id="email"
            v-model="loginForm.email"
            type="email"
            placeholder="Enter your email"
            required
            :class="{ 'error': errors.email }"
          />
          <span v-if="errors.email" class="error-message">{{ errors.email }}</span>
        </div>

        <div class="form-group">
          <label for="password">Password</label>
          <input
            id="password"
            v-model="loginForm.password"
            type="password"
            placeholder="Enter your password"
            required
            :class="{ 'error': errors.password }"
          />
          <span v-if="errors.password" class="error-message">{{ errors.password }}</span>
        </div>

        <div class="form-options">
          <label class="checkbox-label">
            <input v-model="loginForm.rememberMe" type="checkbox" />
            <span class="checkmark"></span>
            Remember me
          </label>
          <router-link to="/forgot-password" class="forgot-link">Forgot Password?</router-link>
        </div>

        <button 
          type="submit" 
          class="login-btn"
          :disabled="isLoading"
        >
          <span v-if="isLoading" class="spinner"></span>
          {{ isLoading ? 'Signing In...' : 'Sign In' }}
        </button>

        <div v-if="errorMessage && !emailNotVerified" class="error-alert">
          {{ errorMessage }}
        </div>

        <div v-if="emailNotVerified" class="error-alert">
          <strong>Email not verified.</strong> Please check your inbox for a verification link.
          <br /><br />
          <button @click="resendVerification" :disabled="resendingVerification" class="btn-resend">
            {{ resendingVerification ? 'Sending...' : 'Resend verification email' }}
          </button>
          <span v-if="resendMessage" style="display:block; margin-top:0.5rem; color: #2ecc71;">{{ resendMessage }}</span>
        </div>
      </form>

      <div class="login-footer">
        <p v-if="registrationEnabled">Don't have an account? 
          <router-link to="/signup" class="signup-link">Sign up here</router-link>
        </p>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, computed } from 'vue';
import { useRouter } from 'vue-router';
import { useAuthStore } from '@/stores/auth';
import { useSettings } from '@/composables/useSettings';

const router = useRouter();
const authStore = useAuthStore();
const { getSetting } = useSettings();

const registrationEnabled = computed(() => getSetting('allow_user_registration', 'true') !== 'false');

const isLoading = ref(false);
const errorMessage = ref('');
const emailNotVerified = ref(false);
const resendingVerification = ref(false);
const resendMessage = ref('');

const loginForm = reactive({
  email: '',
  password: '',
  rememberMe: false
});

const errors = reactive({
  email: '',
  password: ''
});

const validateForm = () => {
  errors.email = '';
  errors.password = '';

  if (!loginForm.email) {
    errors.email = 'Email is required';
    return false;
  }

  if (!loginForm.email.includes('@')) {
    errors.email = 'Please enter a valid email address';
    return false;
  }

  if (!loginForm.password) {
    errors.password = 'Password is required';
    return false;
  }

  if (loginForm.password.length < 6) {
    errors.password = 'Password must be at least 6 characters';
    return false;
  }

  return true;
};

const handleLogin = async () => {
  if (!validateForm()) return;

  isLoading.value = true;
  errorMessage.value = '';
  emailNotVerified.value = false;
  resendMessage.value = '';

  try {
    await authStore.login(loginForm.email, loginForm.password, loginForm.rememberMe);
    router.push('/');
  } catch (error: any) {
    if (error.message === 'EMAIL_NOT_VERIFIED') {
      emailNotVerified.value = true;
    } else {
      errorMessage.value = error.message || 'Login failed. Please try again.';
    }
  } finally {
    isLoading.value = false;
  }
};

const resendVerification = async () => {
  resendingVerification.value = true;
  resendMessage.value = '';
  try {
    const API_URL = `${import.meta.env.VITE_API_URL || 'http://localhost:5226'}/api/v1`;
    const response = await fetch(`${API_URL}/auth/resend-verification`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ email: loginForm.email })
    });
    const data = await response.json();
    resendMessage.value = data.message || 'Verification email sent!';
  } catch {
    resendMessage.value = 'Failed to resend. Please try again.';
  } finally {
    resendingVerification.value = false;
  }
};
</script>

<style scoped>
.login-container {
  min-height: 100vh;
  display: flex;
  align-items: center;
  justify-content: center;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  padding: 2rem;
}

.login-card {
  background: white;
  border-radius: 20px;
  box-shadow: 0 20px 60px rgba(0, 0, 0, 0.3);
  padding: 3rem;
  width: 100%;
  max-width: 400px;
  position: relative;
  overflow: hidden;
}

.login-card::before {
  content: '';
  position: absolute;
  top: 0;
  left: 0;
  right: 0;
  height: 4px;
  background: linear-gradient(90deg, #ff6b35, #f7931e, #ffd700);
}

.login-header {
  text-align: center;
  margin-bottom: 2rem;
}

.login-header h1 {
  color: #333;
  font-size: 2rem;
  margin-bottom: 0.5rem;
  font-weight: 700;
}

.login-header p {
  color: #666;
  margin: 0;
}

.login-form {
  display: flex;
  flex-direction: column;
  gap: 1.5rem;
}

.form-group {
  display: flex;
  flex-direction: column;
}

.form-group label {
  margin-bottom: 0.5rem;
  font-weight: 600;
  color: #333;
}

.form-group input {
  padding: 0.75rem 1rem;
  border: 2px solid #e1e5e9;
  border-radius: 8px;
  font-size: 1rem;
  transition: border-color 0.3s ease;
}

.form-group input:focus {
  outline: none;
  border-color: #ff6b35;
}

.form-group input.error {
  border-color: #e74c3c;
}

.error-message {
  color: #e74c3c;
  font-size: 0.875rem;
  margin-top: 0.5rem;
}

.form-options {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin: 0.5rem 0;
}

.checkbox-label {
  display: flex;
  align-items: center;
  cursor: pointer;
  font-size: 0.875rem;
  color: #666;
}

.checkbox-label input {
  margin-right: 0.5rem;
}

.forgot-link {
  color: #ff6b35;
  text-decoration: none;
  font-size: 0.875rem;
  transition: color 0.3s ease;
}

.forgot-link:hover {
  color: #e55a2b;
}

.login-btn {
  background: linear-gradient(135deg, #ff6b35, #f7931e);
  color: white;
  border: none;
  padding: 0.875rem 1.5rem;
  border-radius: 8px;
  font-size: 1rem;
  font-weight: 600;
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 0.5rem;
  transition: transform 0.2s ease, box-shadow 0.2s ease;
}

.login-btn:hover:not(:disabled) {
  transform: translateY(-2px);
  box-shadow: 0 8px 25px rgba(255, 107, 53, 0.3);
}

.login-btn:disabled {
  opacity: 0.7;
  cursor: not-allowed;
}

.spinner {
  width: 18px;
  height: 18px;
  border: 2px solid transparent;
  border-top: 2px solid white;
  border-radius: 50%;
  animation: spin 1s linear infinite;
}

@keyframes spin {
  to { transform: rotate(360deg); }
}

.error-alert {
  background: #fdf2f2;
  color: #e74c3c;
  padding: 1rem;
  border-radius: 8px;
  border-left: 4px solid #e74c3c;
  font-size: 0.875rem;
}

.login-footer {
  text-align: center;
  margin-top: 2rem;
  padding-top: 2rem;
  border-top: 1px solid #e1e5e9;
}

.login-footer p {
  margin: 0;
  color: #666;
}

.signup-link {
  color: #ff6b35;
  text-decoration: none;
  font-weight: 600;
  transition: color 0.3s ease;
}

.signup-link:hover {
  color: #e55a2b;
}

@media (max-width: 768px) {
  .login-card {
    padding: 2rem;
    margin: 1rem;
  }

  .login-header h1 {
    font-size: 1.75rem;
  }
}

.btn-resend {
  background: none;
  border: 2px solid #e74c3c;
  color: #e74c3c;
  padding: 0.5rem 1.25rem;
  border-radius: 6px;
  font-weight: 600;
  font-size: 0.875rem;
  cursor: pointer;
  transition: background 0.2s, color 0.2s;
}

.btn-resend:hover:not(:disabled) {
  background: #e74c3c;
  color: white;
}

.btn-resend:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}
</style>