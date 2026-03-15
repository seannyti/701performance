<template>
  <div class="contact">
    <div class="container">
      <!-- Page Header -->
      <div class="page-header">
        <h1 class="page-title">{{ getSetting('contact_title', 'Contact Us') }}</h1>
        <p class="page-subtitle">
          {{ getSetting('contact_subtitle', 'Get in touch with our team for questions, support, or to schedule a visit') }}
        </p>
      </div>

      <div class="contact-content">
        <!-- Contact Form Section -->
        <div class="contact-form-section">
          <h2 class="section-title">Send us a Message</h2>
          
          <!-- Success Message -->
          <div v-if="showSuccess" class="success">
            <p>Thank you for your message! We'll get back to you within 24 hours.</p>
          </div>

          <!-- Error Message -->
          <div v-if="submitError" class="error">
            <p>{{ submitError }}</p>
          </div>

          <!-- Contact Form -->
          <form @submit.prevent="handleSubmit" class="contact-form">
            <div class="form-group">
              <label for="name" class="form-label">Full Name *</label>
              <input
                id="name"
                v-model="form.name"
                type="text"
                class="form-control"
                :class="{ error: errors.name }"
                placeholder="Enter your full name"
                required
              />
              <span v-if="errors.name" class="error-text">{{ errors.name }}</span>
            </div>

            <div class="form-group">
              <label for="email" class="form-label">Email Address *</label>
              <input
                id="email"
                v-model="form.email"
                type="email"
                class="form-control"
                :class="{ error: errors.email }"
                placeholder="Enter your email address"
                required
              />
              <span v-if="errors.email" class="error-text">{{ errors.email }}</span>
            </div>

            <div class="form-group">
              <label for="subject" class="form-label">Subject</label>
              <select
                id="subject"
                v-model="form.subject"
                class="form-control"
              >
                <option value="">Select a subject</option>
                <option value="general">General Inquiry</option>
                <option value="product">Product Question</option>
                <option value="support">Technical Support</option>
                <option value="sales">Sales Information</option>
                <option value="other">Other</option>
              </select>
            </div>

            <div class="form-group">
              <label for="message" class="form-label">Message *</label>
              <textarea
                id="message"
                v-model="form.message"
                class="form-control"
                :class="{ error: errors.message }"
                rows="5"
                placeholder="Tell us how we can help you..."
                required
              ></textarea>
              <span v-if="errors.message" class="error-text">{{ errors.message }}</span>
            </div>

            <button
              type="submit"
              class="btn btn-primary btn-lg"
              :disabled="isSubmitting"
            >
              {{ isSubmitting ? 'Sending...' : 'Send Message' }}
            </button>
          </form>
        </div>

        <!-- Contact Information Section -->
        <div class="contact-info-section">
          <h2 class="section-title">{{ getSetting('contact_section_title', 'Get in Touch') }}</h2>
          
          <div class="contact-cards">
            <div class="contact-card" v-if="getSetting('contact_address')">
              <div class="contact-icon">📍</div>
              <div class="contact-details">
                <h3>{{ getSetting('contact_address_title', 'Visit Our Showroom') }}</h3>
                <p>{{ getSetting('contact_address', '123 Adventure Way, Motorville, MV 12345') }}</p>
                <p class="contact-note">{{ getSetting('contact_address_note', 'Open for personal consultations') }}</p>
              </div>
            </div>

            <div class="contact-card" v-if="getSetting('contact_phone')">
              <div class="contact-icon">📞</div>
              <div class="contact-details">
                <h3>{{ getSetting('contact_phone_title', 'Call Us') }}</h3>
                <p><a :href="`tel:${getSetting('contact_phone', '(555) 123-4567').replace(/[^0-9]/g, '')}`">{{ getSetting('contact_phone', '(555) 123-4567') }}</a></p>
                <p class="contact-note" v-html="getSetting('contact_hours', 'Mon-Fri: 9AM-6PM<br>Sat: 9AM-4PM').replace(/\n/g, '<br>')"></p>
              </div>
            </div>

            <div class="contact-card" v-if="getSetting('contact_email')">
              <div class="contact-icon">✉️</div>
              <div class="contact-details">
                <h3>{{ getSetting('contact_email_title', 'Email Us') }}</h3>
                <p><a :href="`mailto:${getSetting('contact_email', 'info@powersportsshowcase.com')}`">{{ getSetting('contact_email', 'info@powersportsshowcase.com') }}</a></p>
                <p class="contact-note">{{ getSetting('contact_email_note', 'We respond within 24 hours') }}</p>
              </div>
            </div>

            <div class="contact-card">
              <div class="contact-icon">💬</div>
              <div class="contact-details">
                <h3>{{ getSetting('contact_livechat_title', 'Live Chat') }}</h3>
                <p>{{ getSetting('contact_livechat_text', 'Available during business hours') }}</p>
                <p class="contact-note">{{ getSetting('contact_livechat_note', 'Quick answers to your questions') }}</p>
              </div>
            </div>
          </div>

          <!-- Map or Additional Info -->
          <div class="additional-info">
            <h3>Why Contact Us?</h3>
            <ul class="info-list">
              <li v-for="(reason, index) in contactReasons" :key="index">{{ reason.title }}</li>
            </ul>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, computed } from 'vue';
import type { ContactForm } from '@/types';
import { useSettings } from '@/composables/useSettings';
import { SUCCESS_MESSAGE_DURATION_MS } from '@/constants';

const { getSetting } = useSettings();

// Parse contact reasons from settings
const contactReasons = computed(() => {
  try {
    const reasonsJson = getSetting('contact_reasons', '[]');
    return JSON.parse(reasonsJson);
  } catch {
    return [
      { title: 'Expert advice on product selection' },
      { title: 'Personalized recommendations' },
      { title: 'Technical support and guidance' },
      { title: 'Financing and warranty information' },
      { title: 'Schedule test rides and demos' }
    ];
  }
});

// Form data
const form = reactive<ContactForm & { subject: string }>({
  name: '',
  email: '',
  message: '',
  subject: ''
});

// Form state
const isSubmitting = ref(false);
const showSuccess = ref(false);
const submitError = ref<string | null>(null);

// Form validation errors
const errors = reactive({
  name: '',
  email: '',
  message: ''
});

// Validation functions
const validateName = (name: string): string => {
  if (!name.trim()) return 'Name is required';
  if (name.trim().length < 2) return 'Name must be at least 2 characters';
  return '';
};

const validateEmail = (email: string): string => {
  if (!email.trim()) return 'Email is required';
  const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
  if (!emailRegex.test(email)) return 'Please enter a valid email address';
  return '';
};

const validateMessage = (message: string): string => {
  if (!message.trim()) return 'Message is required';
  if (message.trim().length < 10) return 'Message must be at least 10 characters';
  return '';
};

// Form validation
const validateForm = (): boolean => {
  errors.name = validateName(form.name);
  errors.email = validateEmail(form.email);
  errors.message = validateMessage(form.message);

  return !errors.name && !errors.email && !errors.message;
};

// Reset form
const resetForm = () => {
  form.name = '';
  form.email = '';
  form.message = '';
  form.subject = '';
  errors.name = '';
  errors.email = '';
  errors.message = '';
};

// Handle form submission
const handleSubmit = async () => {
  // Reset previous states
  showSuccess.value = false;
  submitError.value = null;

  // Validate form
  if (!validateForm()) {
    return;
  }

  isSubmitting.value = true;

  try {
    const API_URL = import.meta.env.VITE_API_URL || 'http://localhost:5226';
    const response = await fetch(`${API_URL}/api/v1/contact`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json'
      },
      body: JSON.stringify({
        name: form.name,
        email: form.email,
        subject: form.subject || '',
        message: form.message
      })
    });

    const data = await response.json();

    if (response.ok) {
      showSuccess.value = true;
      resetForm();
      
      setTimeout(() => {
        showSuccess.value = false;
      }, SUCCESS_MESSAGE_DURATION_MS);
    } else {
      submitError.value = data.message || 'Failed to send message. Please try again later.';
    }

  } catch (error) {
    submitError.value = 'Failed to send message. Please check your connection and try again.';
  } finally {
    isSubmitting.value = false;
  }
};
</script>

<style scoped>
.contact {
  padding: 1rem 0 2rem;
}

.page-header {
  text-align: center;
  margin-bottom: 4rem;
}

.page-title {
  font-size: 2.5rem;
  font-weight: bold;
  color: var(--text-dark);
  margin-bottom: 1rem;
}

.page-subtitle {
  font-size: 1.25rem;
  color: var(--text-light);
  max-width: 600px;
  margin: 0 auto;
}

.contact-content {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 4rem;
  max-width: 1200px;
  margin: 0 auto;
}

.contact-form-section,
.contact-info-section {
  background: white;
  padding: 2.5rem;
  border-radius: 12px;
  box-shadow: var(--shadow-md);
  height: fit-content;
}

.section-title {
  font-size: 1.5rem;
  font-weight: bold;
  color: var(--text-dark);
  margin-bottom: 2rem;
}

.contact-form {
  margin-top: 1.5rem;
}

.error-text {
  color: #dc3545;
  font-size: 0.875rem;
  margin-top: 0.25rem;
  display: block;
}

.contact-cards {
  display: grid;
  gap: 1.5rem;
  margin-bottom: 2rem;
}

.contact-card {
  display: flex;
  align-items: flex-start;
  gap: 1rem;
  padding: 1.5rem;
  background: var(--background-light);
  border-radius: 8px;
  border-left: 4px solid var(--primary-color);
}

.contact-icon {
  font-size: 2rem;
  flex-shrink: 0;
}

.contact-details h3 {
  font-size: 1.125rem;
  font-weight: 600;
  color: var(--text-dark);
  margin-bottom: 0.5rem;
}

.contact-details p {
  color: var(--text-light);
  margin-bottom: 0.25rem;
  line-height: 1.5;
}

.contact-details a {
  color: var(--primary-color);
  text-decoration: none;
  font-weight: 500;
}

.contact-details a:hover {
  text-decoration: underline;
}

.contact-note {
  font-size: 0.9rem;
  font-style: italic;
}

.additional-info {
  padding-top: 1.5rem;
  border-top: 1px solid var(--border-light);
}

.additional-info h3 {
  font-size: 1.125rem;
  font-weight: 600;
  color: var(--text-dark);
  margin-bottom: 1rem;
}

.info-list {
  list-style: none;
  padding: 0;
}

.info-list li {
  padding: 0.5rem 0;
  color: var(--text-light);
  position: relative;
  padding-left: 1.5rem;
}

.info-list li::before {
  content: '✓';
  position: absolute;
  left: 0;
  color: var(--primary-color);
  font-weight: bold;
}

/* Mobile responsive */
@media (max-width: 768px) {
  .page-title {
    font-size: 2rem;
  }

  .contact-content {
    grid-template-columns: 1fr;
    gap: 2rem;
  }

  .contact-form-section,
  .contact-info-section {
    padding: 2rem 1.5rem;
  }

  .contact-card {
    padding: 1.25rem;
    flex-direction: column;
    text-align: center;
    align-items: center;
  }

  .contact-icon {
    margin-bottom: 0.5rem;
  }
}

@media (max-width: 480px) {
  .page-title {
    font-size: 1.75rem;
  }

  .contact-form-section,
  .contact-info-section {
    padding: 1.5rem 1rem;
  }

  .contact-card {
    padding: 1rem;
  }
}
</style>