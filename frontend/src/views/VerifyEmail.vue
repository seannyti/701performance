<template>
  <div class="verify-container">
    <div class="verify-card">
      <div v-if="status === 'loading'" class="state-loading">
        <div class="spinner-large"></div>
        <h2>Verifying your email...</h2>
        <p>Please wait while we verify your email address.</p>
      </div>

      <div v-else-if="status === 'success'" class="state-success">
        <div class="status-icon success-icon">✅</div>
        <h2>Email Verified!</h2>
        <p>Your email address has been verified successfully. You can now log in.</p>
        <router-link to="/login" class="btn btn-primary">Go to Login</router-link>
      </div>

      <div v-else-if="status === 'expired'" class="state-error">
        <div class="status-icon error-icon">⏰</div>
        <h2>Link Expired</h2>
        <p>Your verification link has expired. Request a new one below.</p>
        <div class="resend-form">
          <input
            v-model="resendEmail"
            type="email"
            placeholder="Enter your email address"
            class="form-control"
          />
          <button @click="resendVerificationEmail" :disabled="resending" class="btn btn-primary" style="margin-top: 0.75rem;">
            {{ resending ? 'Sending...' : 'Resend Verification Email' }}
          </button>
          <p v-if="resendMessage" class="resend-message">{{ resendMessage }}</p>
        </div>
        <router-link to="/login" class="back-link">Back to Login</router-link>
      </div>

      <div v-else class="state-error">
        <div class="status-icon error-icon">❌</div>
        <h2>Verification Failed</h2>
        <p>{{ errorMessage || 'This verification link is invalid or has already been used.' }}</p>
        <router-link to="/login" class="btn btn-primary">Back to Login</router-link>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { useRoute } from 'vue-router';

const route = useRoute();
const API_URL = `${import.meta.env.VITE_API_URL || 'http://localhost:5226'}/api/v1`;

const status = ref<'loading' | 'success' | 'expired' | 'error'>('loading');
const errorMessage = ref('');
const resendEmail = ref('');
const resending = ref(false);
const resendMessage = ref('');

onMounted(async () => {
  const token = route.query.token as string;
  if (!token) {
    status.value = 'error';
    errorMessage.value = 'No verification token provided.';
    return;
  }

  try {
    const response = await fetch(`${API_URL}/auth/verify-email?token=${encodeURIComponent(token)}`);
    const data = await response.json();

    if (response.ok) {
      status.value = 'success';
    } else if (data.message?.toLowerCase().includes('expired')) {
      status.value = 'expired';
    } else {
      status.value = 'error';
      errorMessage.value = data.message || 'Verification failed.';
    }
  } catch {
    status.value = 'error';
    errorMessage.value = 'An error occurred. Please try again.';
  }
});

const resendVerificationEmail = async () => {
  if (!resendEmail.value) return;
  resending.value = true;
  resendMessage.value = '';
  try {
    const response = await fetch(`${API_URL}/auth/resend-verification`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ email: resendEmail.value })
    });
    const data = await response.json();
    resendMessage.value = data.message || 'Verification email sent!';
  } catch {
    resendMessage.value = 'Failed to resend. Please try again.';
  } finally {
    resending.value = false;
  }
};
</script>

<style scoped>
.verify-container {
  min-height: 100vh;
  display: flex;
  align-items: center;
  justify-content: center;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  padding: 2rem;
}

.verify-card {
  background: white;
  border-radius: 16px;
  padding: 3rem 2.5rem;
  max-width: 480px;
  width: 100%;
  text-align: center;
  box-shadow: 0 20px 60px rgba(0, 0, 0, 0.2);
}

.status-icon {
  font-size: 4rem;
  margin-bottom: 1rem;
}

h2 {
  font-size: 1.75rem;
  font-weight: 700;
  color: #1a1a2e;
  margin-bottom: 0.75rem;
}

p {
  color: #666;
  font-size: 1rem;
  line-height: 1.6;
  margin-bottom: 1.5rem;
}

.btn {
  display: inline-block;
  padding: 0.75rem 2rem;
  border-radius: 8px;
  font-weight: 600;
  text-decoration: none;
  cursor: pointer;
  border: none;
  font-size: 1rem;
}

.btn-primary {
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  color: white;
  width: 100%;
}

.btn-primary:disabled {
  opacity: 0.7;
  cursor: not-allowed;
}

.form-control {
  width: 100%;
  padding: 0.75rem 1rem;
  border: 2px solid #e0e0e0;
  border-radius: 8px;
  font-size: 1rem;
  box-sizing: border-box;
}

.form-control:focus {
  outline: none;
  border-color: #667eea;
}

.resend-form {
  margin-bottom: 1.5rem;
}

.resend-message {
  color: #2ecc71;
  font-size: 0.9rem;
  margin-top: 0.5rem;
  margin-bottom: 0;
}

.back-link {
  display: block;
  margin-top: 1rem;
  color: #667eea;
  text-decoration: none;
  font-size: 0.9rem;
}

.back-link:hover {
  text-decoration: underline;
}

.spinner-large {
  width: 48px;
  height: 48px;
  border: 4px solid #e0e0e0;
  border-top-color: #667eea;
  border-radius: 50%;
  animation: spin 0.8s linear infinite;
  margin: 0 auto 1.5rem;
}

@keyframes spin {
  to { transform: rotate(360deg); }
}
</style>
