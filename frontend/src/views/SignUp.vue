<template>
  <div class="signup-container">
    <div class="signup-card">
      <div class="signup-header">
        <h1>Join Powersports</h1>
        <p>Create your account to start shopping</p>
      </div>

      <form @submit.prevent="handleSignup" class="signup-form">
        <div class="form-group">
          <label for="firstName">First Name</label>
          <input
            id="firstName"
            v-model="signupForm.firstName"
            type="text"
            placeholder="Enter your first name"
            required
            :class="{ 'error': errors.firstName }"
          />
          <span v-if="errors.firstName" class="error-message">{{ errors.firstName }}</span>
        </div>

        <div class="form-group">
          <label for="lastName">Last Name</label>
          <input
            id="lastName"
            v-model="signupForm.lastName"
            type="text"
            placeholder="Enter your last name"
            required
            :class="{ 'error': errors.lastName }"
          />
          <span v-if="errors.lastName" class="error-message">{{ errors.lastName }}</span>
        </div>

        <div class="form-group">
          <label for="email">Email Address</label>
          <input
            id="email"
            v-model="signupForm.email"
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
            v-model="signupForm.password"
            type="password"
            placeholder="Create a password"
            required
            :class="{ 'error': errors.password }"
          />
          
          <!-- Password strength indicator -->
          <div v-if="signupForm.password" class="password-strength">
            <div class="strength-bars">
              <div
                v-for="i in 4"
                :key="i"
                class="strength-bar"
                :class="{ 'active': i <= passwordStrength.score }"
                :style="{ backgroundColor: i <= passwordStrength.score ? passwordStrength.color : 'rgba(0,0,0,0.1)' }"
              ></div>
            </div>
            <small class="strength-label" :style="{ color: passwordStrength.color }">{{ passwordStrength.label }}</small>
          </div>
          
          <span v-if="errors.password" class="error-message">{{ errors.password }}</span>
        </div>

        <div class="form-group">
          <label for="confirmPassword">Confirm Password</label>
          <input
            id="confirmPassword"
            v-model="signupForm.confirmPassword"
            type="password"
            placeholder="Confirm your password"
            required
            :class="{ 'error': errors.confirmPassword }"
          />
          <span v-if="errors.confirmPassword" class="error-message">{{ errors.confirmPassword }}</span>
        </div>

        <div class="form-group">
          <label for="phone">Phone Number</label>
          <input
            id="phone"
            v-model="signupForm.phone"
            type="tel"
            placeholder="Enter your phone number"
            :class="{ 'error': errors.phone }"
          />
          <span v-if="errors.phone" class="error-message">{{ errors.phone }}</span>
        </div>

        <div class="form-options">
          <label class="checkbox-label">
            <input v-model="signupForm.agreeToTerms" type="checkbox" required />
            <span class="checkmark"></span>
            I agree to the <a href="/terms" class="link">Terms of Service</a> and <a href="/privacy" class="link">Privacy Policy</a>
          </label>

          <label class="checkbox-label">
            <input v-model="signupForm.subscribeNewsletter" type="checkbox" />
            <span class="checkmark"></span>
            Subscribe to newsletter for deals and updates
          </label>
        </div>

        <button 
          type="submit" 
          class="signup-btn"
          :disabled="isLoading"
        >
          <span v-if="isLoading" class="spinner"></span>
          {{ isLoading ? 'Creating Account...' : 'Create Account' }}
        </button>

        <div v-if="errorMessage" class="error-alert">
          {{ errorMessage }}
        </div>

        <div v-if="successMessage" class="success-alert">
          {{ successMessage }}
        </div>
      </form>

      <div class="signup-footer">
        <p>Already have an account? 
          <router-link to="/login" class="login-link">Sign in here</router-link>
        </p>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, computed, onMounted } from 'vue';
import { useRouter } from 'vue-router';
import { useAuthStore } from '@/stores/auth';
import { useSettings } from '@/composables/useSettings';

const router = useRouter();
const authStore = useAuthStore();
const { getSetting } = useSettings();

// Redirect away if registration is disabled
onMounted(() => {
  if (getSetting('allow_user_registration', 'true') === 'false') {
    router.replace('/login');
  }
});

const isLoading = ref(false);
const errorMessage = ref('');
const successMessage = ref('');

const signupForm = reactive({
  firstName: '',
  lastName: '',
  email: '',
  password: '',
  confirmPassword: '',
  phone: '',
  agreeToTerms: false,
  subscribeNewsletter: false
});

const errors = reactive({
  firstName: '',
  lastName: '',
  email: '',
  password: '',
  confirmPassword: '',
  phone: ''
});

// Password strength computed property
const passwordStrength = computed(() => {
  const pwd = signupForm.password
  if (!pwd) return { score: 0, label: '', color: 'transparent' }

  let score = 0
  if (pwd.length >= 6) score++
  if (pwd.length >= 10) score++
  if (/[A-Z]/.test(pwd) && /[a-z]/.test(pwd)) score++
  if (/\d/.test(pwd) && /[^a-zA-Z0-9]/.test(pwd)) score++

  const levels = [
    { label: 'Weak',   color: '#e74c3c' },
    { label: 'Fair',   color: '#f39c12' },
    { label: 'Good',   color: '#3498db' },
    { label: 'Strong', color: '#27ae60' }
  ]

  return { score, ...levels[score - 1] || levels[0] }
});

const validateForm = () => {
  // Reset errors
  Object.keys(errors).forEach(key => {
    (errors as any)[key] = '';
  });

  let isValid = true;

  if (!signupForm.firstName.trim()) {
    errors.firstName = 'First name is required';
    isValid = false;
  }

  if (!signupForm.lastName.trim()) {
    errors.lastName = 'Last name is required';
    isValid = false;
  }

  if (!signupForm.email) {
    errors.email = 'Email is required';
    isValid = false;
  } else if (!signupForm.email.includes('@')) {
    errors.email = 'Please enter a valid email address';
    isValid = false;
  }

  if (!signupForm.password) {
    errors.password = 'Password is required';
    isValid = false;
  } else if (signupForm.password.length < 8) {
    errors.password = 'Password must be at least 8 characters';
    isValid = false;
  } else if (!/(?=.*[a-z])(?=.*[A-Z])(?=.*\d)/.test(signupForm.password)) {
    errors.password = 'Password must contain uppercase, lowercase, and number';
    isValid = false;
  }

  if (!signupForm.confirmPassword) {
    errors.confirmPassword = 'Please confirm your password';
    isValid = false;
  } else if (signupForm.password !== signupForm.confirmPassword) {
    errors.confirmPassword = 'Passwords do not match';
    isValid = false;
  }

  if (signupForm.phone && !/^\+?[\d\s-()]+$/.test(signupForm.phone)) {
    errors.phone = 'Please enter a valid phone number';
    isValid = false;
  }

  return isValid;
};

const handleSignup = async () => {
  if (!validateForm()) return;

  if (!signupForm.agreeToTerms) {
    errorMessage.value = 'You must agree to the Terms of Service and Privacy Policy';
    return;
  }

  isLoading.value = true;
  errorMessage.value = '';
  successMessage.value = '';

  try {
    const result = await authStore.signup({
      firstName: signupForm.firstName.trim(),
      lastName: signupForm.lastName.trim(),
      email: signupForm.email,
      password: signupForm.password,
      phone: signupForm.phone
    });
    
    if (result.requiresEmailVerification) {
      successMessage.value = 'Account created! Please check your email and click the verification link before logging in.';
      // Don't redirect — let user see the message
    } else {
      successMessage.value = 'Account created successfully! Welcome to Powersports!';
      // Redirect after a short delay
      setTimeout(() => {
        router.push('/');
      }, 2000);
    }
  } catch (error: any) {
    errorMessage.value = error.message || 'Registration failed. Please try again.';
  } finally {
    isLoading.value = false;
  }
};
</script>

<style scoped>
.signup-container {
  min-height: 100vh;
  display: flex;
  align-items: center;
  justify-content: center;
  background: linear-gradient(
    var(--gradient-direction, 135deg),
    var(--gradient-start, #667eea),
    var(--gradient-end, #764ba2)
  );
  padding: 2rem;
}

.signup-card {
  background: white;
  border-radius: 20px;
  box-shadow: 0 20px 60px rgba(0, 0, 0, 0.3);
  padding: 3rem;
  width: 100%;
  max-width: 500px;
  position: relative;
  overflow: hidden;
}

.signup-card::before {
  content: '';
  position: absolute;
  top: 0;
  left: 0;
  right: 0;
  height: 4px;
  background: var(--gradient);
}

.signup-header {
  text-align: center;
  margin-bottom: 2rem;
}

.signup-header h1 {
  color: #333;
  font-size: 2rem;
  margin-bottom: 0.5rem;
  font-weight: 700;
}

.signup-header p {
  color: #666;
  margin: 0;
}

.signup-form {
  display: flex;
  flex-direction: column;
  gap: 0.75rem;
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
  flex-direction: column;
  gap: 1rem;
}

.checkbox-label {
  display: flex;
  align-items: flex-start;
  cursor: pointer;
  font-size: 0.875rem;
  color: #666;
  gap: 0.5rem;
}

.checkbox-label input {
  margin: 0;
  flex-shrink: 0;
}

.link {
  color: #ff6b35;
  text-decoration: none;
  transition: color 0.3s ease;
}

.link:hover {
  color: #e55a2b;
}

.signup-btn {
  background: var(--gradient);
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

.signup-btn:hover:not(:disabled) {
  transform: translateY(-2px);
  box-shadow: 0 8px 25px rgba(255, 107, 53, 0.3);
}

.signup-btn:disabled {
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

.success-alert {
  background: #f0fff4;
  color: #22c55e;
  padding: 1rem;
  border-radius: 8px;
  border-left: 4px solid #22c55e;
  font-size: 0.875rem;
}

.signup-footer {
  text-align: center;
  margin-top: 2rem;
  padding-top: 2rem;
  border-top: 1px solid #e1e5e9;
}

.signup-footer p {
  margin: 0;
  color: #666;
}

.login-link {
  color: #ff6b35;
  text-decoration: none;
  font-weight: 600;
  transition: color 0.3s ease;
}

.login-link:hover {
  color: #e55a2b;
}

/* Password Strength Indicator Styles */
.password-strength {
  margin-top: 0.5rem;
}

.strength-bars {
  display: flex;
  gap: 4px;
  margin-bottom: 0.25rem;
}

.strength-bar {
  height: 3px;
  flex: 1;
  border-radius: 2px;
  background-color: rgba(0,0,0,0.1);
  transition: background-color 0.3s ease;
}

.strength-bar.active {
  opacity: 1;
}

.strength-label {
  font-size: 0.875rem;
  font-weight: 500;
  transition: color 0.3s ease;
}

@media (max-width: 768px) {
  .signup-card {
    padding: 2rem;
    margin: 1rem;
  }

  .signup-header h1 {
    font-size: 1.75rem;
  }

  .checkbox-label {
    align-items: flex-start;
  }
}
</style>