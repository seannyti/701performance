<template>
  <div class="maintenance-page">
    <div class="maintenance-container">
      <div class="maintenance-icon">
        <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
          <path d="M14.7 6.3a1 1 0 0 0 0 1.4l1.6 1.6a1 1 0 0 0 1.4 0l3.77-3.77a6 6 0 0 1-7.94 7.94l-6.91 6.91a2.12 2.12 0 0 1-3-3l6.91-6.91a6 6 0 0 1 7.94-7.94l-3.76 3.76z"/>
        </svg>
      </div>
      
      <h1 class="maintenance-title">Site Under Maintenance</h1>
      
      <p class="maintenance-message">
        We're currently performing scheduled maintenance to improve your experience. 
        We'll be back online shortly!
      </p>
      
      <p class="maintenance-submessage">
        Thank you for your patience.
      </p>
      
      <div class="maintenance-actions">
        <router-link to="/login" class="btn-admin-login">
          <svg xmlns="http://www.w3.org/2000/svg" width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
            <path d="M15 3h4a2 2 0 0 1 2 2v14a2 2 0 0 1-2 2h-4"/>
            <polyline points="10 17 15 12 10 7"/>
            <line x1="15" y1="12" x2="3" y2="12"/>
          </svg>
          Admin Login
        </router-link>
      </div>
      
      <div class="maintenance-footer">
        <p class="contact-info">
          Need urgent assistance? Contact us at 
          <a :href="`mailto:${contactEmail}`">{{ contactEmail }}</a>
        </p>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import { useSettings } from '@/composables/useSettings'

const { getSetting } = useSettings()

const contactEmail = computed(() => getSetting('contact_email', 'support@example.com'))
</script>

<style scoped>
.maintenance-page {
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

.maintenance-container {
  max-width: 600px;
  width: 100%;
  background: white;
  border-radius: 20px;
  padding: 3rem;
  text-align: center;
  box-shadow: 0 20px 60px rgba(0, 0, 0, 0.3);
  position: relative;
  overflow: hidden;
}

.maintenance-container::before {
  content: '';
  position: absolute;
  top: 0;
  left: 0;
  right: 0;
  height: 4px;
  background: var(--gradient);
}

.maintenance-icon {
  width: 120px;
  height: 120px;
  margin: 0 auto 2rem;
  color: #667eea;
  animation: rotate 3s linear infinite;
}

.maintenance-icon svg {
  width: 100%;
  height: 100%;
}

@keyframes rotate {
  from {
    transform: rotate(0deg);
  }
  to {
    transform: rotate(360deg);
  }
}

.maintenance-title {
  font-size: 2.5rem;
  font-weight: 700;
  color: #2d3748;
  margin-bottom: 1rem;
}

.maintenance-message {
  font-size: 1.125rem;
  color: #4a5568;
  line-height: 1.6;
  margin-bottom: 0.5rem;
}

.maintenance-submessage {
  font-size: 1rem;
  color: #718096;
  margin-bottom: 2.5rem;
}

.maintenance-actions {
  margin: 2.5rem 0;
}

.btn-admin-login {
  display: inline-flex;
  align-items: center;
  gap: 0.5rem;
  padding: 0.875rem 2rem;
  background: linear-gradient(135deg, #ff6b35, #f7931e);
  color: white;
  text-decoration: none;
  border-radius: 12px;
  font-weight: 600;
  font-size: 1rem;
  transition: all 0.3s ease;
  box-shadow: 0 4px 14px rgba(255, 107, 53, 0.4);
  border: none;
  cursor: pointer;
}

.btn-admin-login:hover {
  transform: translateY(-2px);
  box-shadow: 0 8px 25px rgba(255, 107, 53, 0.5);
}

.btn-admin-login:active {
  transform: translateY(0);
}

.maintenance-footer {
  margin-top: 3rem;
  padding-top: 2rem;
  border-top: 1px solid #e2e8f0;
}

.contact-info {
  font-size: 0.875rem;
  color: #718096;
}

.contact-info a {
  color: #ff6b35;
  text-decoration: none;
  font-weight: 600;
  transition: color 0.2s;
}

.contact-info a:hover {
  color: #e55a2b;
  text-decoration: underline;
}

@media (max-width: 640px) {
  .maintenance-page {
    padding: 1rem;
  }

  .maintenance-container {
    padding: 2rem 1.5rem;
  }

  .maintenance-title {
    font-size: 2rem;
  }

  .maintenance-message {
    font-size: 1rem;
  }

  .maintenance-icon {
    width: 80px;
    height: 80px;
  }

  .btn-admin-login {
    padding: 0.75rem 1.5rem;
    font-size: 0.9rem;
  }
}
</style>
