<script setup lang="ts">
import { ref, computed } from 'vue'
import { useSettingsStore } from '../stores/settings'
import api from '../services/api'

const settings = useSettingsStore()

const phone = computed(() => settings.get('contact_phone', ''))
const email = computed(() => settings.get('contact_email', ''))
const address = computed(() => settings.get('contact_address', ''))
const mapsUrl = computed(() => settings.get('maps_embed_url', ''))
const facebook = computed(() => settings.get('social_facebook', ''))
const instagram = computed(() => settings.get('social_instagram', ''))
const youtube = computed(() => settings.get('social_youtube', ''))
const tiktok = computed(() => settings.get('social_tiktok', ''))

const businessHours = computed<Record<string, { open: string; close: string; closed: boolean }>>(() => {
  const raw = settings.get('business_hours', '{}')
  try { return JSON.parse(raw) } catch { return {} }
})
const daysOfWeek = ['Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday', 'Sunday']
const hasHours = computed(() =>
  Object.values(businessHours.value).some(h => h.closed || (h.open && h.close))
)

// Copy to clipboard
const copiedKey = ref<string | null>(null)
async function copyToClipboard(value: string, key: string) {
  try {
    await navigator.clipboard.writeText(value)
    copiedKey.value = key
    setTimeout(() => { copiedKey.value = null }, 2000)
  } catch {
    // Clipboard API unavailable (insecure context or blocked by browser) — show value inline so user can copy manually
    copiedKey.value = `${key}_error`
    setTimeout(() => { copiedKey.value = null }, 3000)
  }
}

// Form state
const form = ref({ name: '', email: '', phone: '', subject: '', message: '' })
const submitting = ref(false)
const submitted = ref(false)
const submitError = ref('')

async function submitForm() {
  if (!form.value.name || !form.value.email || !form.value.message) return
  submitting.value = true
  submitError.value = ''
  try {
    await api.post('/api/contact', form.value)
    submitted.value = true
    form.value = { name: '', email: '', phone: '', subject: '', message: '' }
  } catch {
    submitError.value = 'Something went wrong. Please try again or call us directly.'
  } finally {
    submitting.value = false
  }
}
</script>

<template>
  <div class="contact-page">
    <!-- Hero -->
    <div class="contact-hero">
      <div class="container">
        <h1>Contact <span class="accent">Us</span></h1>
        <p>We'd love to hear from you. Reach out with questions, ride talk, or to schedule service.</p>
      </div>
    </div>

    <div class="container contact-body">
      <div class="contact-grid">
        <!-- Left: Form -->
        <div class="form-col">
          <h2>Send a Message</h2>

          <div v-if="submitted" class="success-msg">
            <svg viewBox="0 0 24 24" fill="none" width="28" height="28">
              <circle cx="12" cy="12" r="11" stroke="currentColor" stroke-width="1.5"/>
              <path d="M7 12l3.5 3.5L17 9" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"/>
            </svg>
            <div>
              <strong>Message sent!</strong>
              <p>We'll get back to you as soon as possible.</p>
            </div>
          </div>

          <form v-else @submit.prevent="submitForm" class="contact-form" novalidate>
            <div class="form-row">
              <div class="form-field">
                <label for="name">Full Name <span class="required">*</span></label>
                <input
                  id="name"
                  v-model="form.name"
                  type="text"
                  placeholder="John Smith"
                  required
                  class="form-input"
                />
              </div>
              <div class="form-field">
                <label for="email">Email <span class="required">*</span></label>
                <input
                  id="email"
                  v-model="form.email"
                  type="email"
                  placeholder="john@example.com"
                  required
                  class="form-input"
                />
              </div>
            </div>

            <div class="form-row">
              <div class="form-field">
                <label for="phone">Phone (optional)</label>
                <input
                  id="phone"
                  v-model="form.phone"
                  type="tel"
                  placeholder="(555) 555-5555"
                  class="form-input"
                />
              </div>
              <div class="form-field">
                <label for="subject">Subject</label>
                <select id="subject" v-model="form.subject" class="form-input">
                  <option value="">Select a topic...</option>
                  <option value="Inventory Question">Inventory Question</option>
                  <option value="Financing">Financing</option>
                  <option value="Service / Repair">Service / Repair</option>
                  <option value="Parts">Parts</option>
                  <option value="Consignment">Consignment</option>
                  <option value="General Inquiry">General Inquiry</option>
                </select>
              </div>
            </div>

            <div class="form-field">
              <label for="message">Message <span class="required">*</span></label>
              <textarea
                id="message"
                v-model="form.message"
                rows="5"
                placeholder="Tell us what you're looking for, or ask any question..."
                required
                class="form-input form-textarea"
              />
            </div>

            <div v-if="submitError" class="error-msg">{{ submitError }}</div>

            <button
              type="submit"
              class="submit-btn"
              :disabled="submitting || !form.name || !form.email || !form.message"
            >
              <span v-if="submitting">Sending...</span>
              <span v-else>Send Message</span>
            </button>
          </form>
        </div>

        <!-- Right: Info + Hours -->
        <div class="info-col">

          <!-- Contact details -->
          <div class="info-card">
            <div class="info-card-header">
              <div class="info-card-header-icon">
                <svg viewBox="0 0 24 24" fill="none" width="20" height="20">
                  <path d="M21 15a2 2 0 01-2 2H7l-4 4V5a2 2 0 012-2h14a2 2 0 012 2z" stroke="currentColor" stroke-width="1.5" stroke-linejoin="round"/>
                </svg>
              </div>
              <div>
                <h3>Get In Touch</h3>
                <p class="info-card-sub">We're here to help — reach out anytime.</p>
              </div>
            </div>

            <div class="contact-methods">
              <button v-if="phone" class="contact-method" @click="copyToClipboard(phone, 'phone')">
                <div class="cm-icon">
                  <svg viewBox="0 0 24 24" fill="none" width="20" height="20">
                    <path d="M6.6 10.8c1.4 2.8 3.8 5.1 6.6 6.6l2.2-2.2c.3-.3.7-.4 1-.2 1.1.4 2.3.6 3.6.6.6 0 1 .4 1 1V20c0 .6-.4 1-1 1C9.6 21 3 14.4 3 6c0-.6.4-1 1-1h3.5c.6 0 1 .4 1 1 0 1.3.2 2.5.6 3.6.1.3 0 .7-.2 1L6.6 10.8z" stroke="currentColor" stroke-width="1.5"/>
                  </svg>
                </div>
                <div class="cm-body">
                  <span class="cm-label">Phone</span>
                  <span class="cm-value">{{ phone }}</span>
                </div>
                <span v-if="copiedKey === 'phone'" class="cm-copied">Copied!</span>
                <span v-else-if="copiedKey === 'phone_error'" class="cm-copied cm-copied--error">Copy failed</span>
                <svg v-else class="cm-arrow" viewBox="0 0 24 24" fill="none" width="16" height="16">
                  <rect x="8" y="8" width="12" height="12" rx="2" stroke="currentColor" stroke-width="1.5"/>
                  <path d="M16 8V6a2 2 0 00-2-2H6a2 2 0 00-2 2v8a2 2 0 002 2h2" stroke="currentColor" stroke-width="1.5" stroke-linecap="round"/>
                </svg>
              </button>

              <button v-if="email" class="contact-method" @click="copyToClipboard(email, 'email')">
                <div class="cm-icon">
                  <svg viewBox="0 0 24 24" fill="none" width="20" height="20">
                    <rect x="2" y="4" width="20" height="16" rx="2" stroke="currentColor" stroke-width="1.5"/>
                    <path d="M2 8l10 6 10-6" stroke="currentColor" stroke-width="1.5" stroke-linejoin="round"/>
                  </svg>
                </div>
                <div class="cm-body">
                  <span class="cm-label">Email</span>
                  <span class="cm-value">{{ email }}</span>
                </div>
                <span v-if="copiedKey === 'email'" class="cm-copied">Copied!</span>
                <span v-else-if="copiedKey === 'email_error'" class="cm-copied cm-copied--error">Copy failed</span>
                <svg v-else class="cm-arrow" viewBox="0 0 24 24" fill="none" width="16" height="16">
                  <rect x="8" y="8" width="12" height="12" rx="2" stroke="currentColor" stroke-width="1.5"/>
                  <path d="M16 8V6a2 2 0 00-2-2H6a2 2 0 00-2 2v8a2 2 0 002 2h2" stroke="currentColor" stroke-width="1.5" stroke-linecap="round"/>
                </svg>
              </button>

              <div v-if="address" class="contact-method no-hover">
                <div class="cm-icon">
                  <svg viewBox="0 0 24 24" fill="none" width="20" height="20">
                    <path d="M12 2C8.13 2 5 5.13 5 9c0 5.25 7 13 7 13s7-7.75 7-13c0-3.87-3.13-7-7-7z" stroke="currentColor" stroke-width="1.5"/>
                    <circle cx="12" cy="9" r="2.5" stroke="currentColor" stroke-width="1.5"/>
                  </svg>
                </div>
                <div class="cm-body">
                  <span class="cm-label">Address</span>
                  <address class="cm-value" style="font-style:normal">{{ address }}</address>
                </div>
              </div>
            </div>

            <!-- Social links -->
            <div v-if="facebook || instagram || youtube || tiktok" class="social-links">
              <a v-if="facebook" :href="facebook" target="_blank" rel="noopener" class="social-link" title="Facebook">
                <svg viewBox="0 0 24 24" fill="currentColor" width="18" height="18">
                  <path d="M18 2h-3a5 5 0 00-5 5v3H7v4h3v8h4v-8h3l1-4h-4V7a1 1 0 011-1h3z"/>
                </svg>
              </a>
              <a v-if="instagram" :href="instagram" target="_blank" rel="noopener" class="social-link" title="Instagram">
                <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" width="18" height="18">
                  <rect x="2" y="2" width="20" height="20" rx="5"/>
                  <circle cx="12" cy="12" r="4"/>
                  <circle cx="17.5" cy="6.5" r="1" fill="currentColor" stroke="none"/>
                </svg>
              </a>
              <a v-if="youtube" :href="youtube" target="_blank" rel="noopener" class="social-link" title="YouTube">
                <svg viewBox="0 0 24 24" fill="currentColor" width="18" height="18">
                  <path d="M22.54 6.42a2.78 2.78 0 00-1.95-1.96C18.88 4 12 4 12 4s-6.88 0-8.59.46a2.78 2.78 0 00-1.95 1.96A29 29 0 001 12a29 29 0 00.46 5.58a2.78 2.78 0 001.95 1.96C5.12 20 12 20 12 20s6.88 0 8.59-.46a2.78 2.78 0 001.95-1.96A29 29 0 0023 12a29 29 0 00-.46-5.58zM9.75 15.02V8.98L15.5 12l-5.75 3.02z"/>
                </svg>
              </a>
              <a v-if="tiktok" :href="tiktok" target="_blank" rel="noopener" class="social-link" title="TikTok">
                <svg viewBox="0 0 24 24" fill="currentColor" width="18" height="18">
                  <path d="M19.59 6.69a4.83 4.83 0 01-3.77-4.25V2h-3.45v13.67a2.89 2.89 0 01-5.2 1.74 2.89 2.89 0 012.31-4.64 2.93 2.93 0 01.88.13V9.4a6.84 6.84 0 00-1-.05A6.33 6.33 0 005.8 20.1a6.34 6.34 0 0010.86-4.43V8.84a8.16 8.16 0 004.77 1.52V6.91a4.85 4.85 0 01-1.84-.22z"/>
                </svg>
              </a>
            </div>
          </div>

          <!-- Business Hours -->
          <div v-if="hasHours" class="info-card">
            <div class="info-card-header">
              <div class="info-card-header-icon">
                <svg viewBox="0 0 24 24" fill="none" width="20" height="20">
                  <circle cx="12" cy="12" r="9" stroke="currentColor" stroke-width="1.5"/>
                  <path d="M12 7v5l3 3" stroke="currentColor" stroke-width="1.5" stroke-linecap="round"/>
                </svg>
              </div>
              <div>
                <h3>Business Hours</h3>
                <p class="info-card-sub">When you can find us on the lot.</p>
              </div>
            </div>
            <div class="hours-list">
              <div v-for="day in daysOfWeek" :key="day" class="hours-row">
                <span class="hours-day">{{ day }}</span>
                <span v-if="businessHours[day]?.closed" class="hours-closed">Closed</span>
                <span v-else-if="businessHours[day]" class="hours-time">
                  {{ businessHours[day].open }} – {{ businessHours[day].close }}
                </span>
                <span v-else class="hours-closed">—</span>
              </div>
            </div>
          </div>

        </div>
      </div>

      <!-- Map embed -->
      <div v-if="mapsUrl" class="map-section">
        <iframe
          :src="mapsUrl"
          width="100%"
          height="400"
          style="border:0;"
          allowfullscreen
          loading="lazy"
          class="map-embed"
        />
      </div>
    </div>
  </div>
</template>

<style lang="scss" scoped>
@use '../assets/styles/variables' as *;
@use '../assets/styles/mixins' as *;

.contact-page { padding-top: $navbar-height; min-height: 100vh; }

.contact-hero {
  background: $bg-card;
  border-bottom: 1px solid $border;
  padding: $spacing-3xl $spacing-lg;
  text-align: center;

  h1 { font-size: clamp($font-size-3xl, 5vw, $font-size-5xl); font-weight: 900; margin-bottom: $spacing-sm; }
  p { color: $text-muted; font-size: $font-size-lg; }
}

.accent { color: $primary; }

.contact-body {
  padding-top: $spacing-3xl;
  padding-bottom: $spacing-3xl;
}

.contact-grid {
  display: grid;
  grid-template-columns: 1fr 360px;
  gap: $spacing-3xl;
  align-items: start;

  @media (max-width: $bp-lg) { grid-template-columns: 1fr; }
}

// Form
.form-col {
  h2 { font-size: $font-size-2xl; font-weight: 800; margin-bottom: $spacing-xl; }
}

.contact-form { display: flex; flex-direction: column; gap: $spacing-lg; }

.form-row {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: $spacing-lg;
  @media (max-width: $bp-sm) { grid-template-columns: 1fr; }
}

.form-field {
  display: flex;
  flex-direction: column;
  gap: $spacing-xs;

  label { font-size: $font-size-sm; font-weight: 600; color: $text-secondary; }
}

.required { color: $primary; }

.form-input {
  background: $bg-elevated;
  border: 1px solid $border;
  border-radius: $radius-md;
  padding: 0.75rem 1rem;
  color: $text;
  font-size: $font-size-base;
  font-family: inherit;
  transition: border-color $transition-fast;

  &::placeholder { color: $text-muted; }
  &:focus { outline: none; border-color: $primary; }
}

select.form-input { appearance: none; cursor: pointer; }
option { background: $bg-card; }
.form-textarea { resize: vertical; min-height: 120px; }

.error-msg {
  background: rgba($error, 0.1);
  border: 1px solid rgba($error, 0.3);
  color: $error;
  padding: $spacing-sm $spacing-md;
  border-radius: $radius-md;
  font-size: $font-size-sm;
}

.submit-btn {
  @include btn-primary;
  padding: 1rem;
  font-size: $font-size-base;
  font-weight: 700;
  border-radius: $radius-md;
  cursor: pointer;
  border: none;

  &:disabled { opacity: 0.5; cursor: not-allowed; }
}

.success-msg {
  display: flex;
  align-items: flex-start;
  gap: $spacing-lg;
  padding: $spacing-xl;
  background: rgba($success, 0.08);
  border: 1px solid rgba($success, 0.25);
  border-radius: $radius-lg;
  color: $success;

  strong { display: block; font-size: $font-size-lg; margin-bottom: $spacing-xs; }
  p { font-size: $font-size-sm; color: $text-muted; margin: 0; }
}

// Info col
.info-col { display: flex; flex-direction: column; gap: $spacing-lg; }

.info-card {
  @include card;
}

.info-card-header {
  display: flex;
  align-items: center;
  gap: $spacing-md;
  margin-bottom: $spacing-lg;

  h3 { font-size: $font-size-lg; font-weight: 700; margin: 0 0 2px; }
}

.info-card-header-icon {
  width: 42px;
  height: 42px;
  min-width: 42px;
  background: rgba($primary, 0.12);
  color: $primary;
  border-radius: $radius-md;
  display: flex;
  align-items: center;
  justify-content: center;
}

.info-card-sub {
  font-size: $font-size-sm;
  color: $text-muted;
  margin: 0;
}

.contact-methods {
  display: flex;
  flex-direction: column;
  gap: $spacing-xs;
  margin-bottom: $spacing-md;
}

.contact-method {
  display: flex;
  align-items: center;
  gap: $spacing-md;
  padding: $spacing-sm $spacing-md;
  border-radius: $radius-md;
  border: 1px solid $border;
  background: $bg-elevated;
  color: $text;
  text-decoration: none;
  transition: all $transition-fast;
  min-width: 0;
  width: 100%;
  font-family: inherit;
  cursor: pointer;
  text-align: left;

  &:not(.no-hover):hover {
    border-color: $primary;
    background: rgba($primary, 0.06);
    .cm-arrow { color: $primary; opacity: 1; }
  }
}

.cm-icon {
  width: 36px;
  height: 36px;
  min-width: 36px;
  background: rgba($primary, 0.1);
  color: $primary;
  border-radius: $radius-sm;
  display: flex;
  align-items: center;
  justify-content: center;
}

.cm-body {
  display: flex;
  flex-direction: column;
  gap: 1px;
  flex: 1;
  min-width: 0;
}

.cm-label {
  font-size: 0.7rem;
  font-weight: 700;
  text-transform: uppercase;
  letter-spacing: 0.5px;
  color: $text-muted;
}

.cm-value {
  font-size: $font-size-sm;
  font-weight: 500;
  color: $text;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.cm-arrow {
  color: $text-muted;
  opacity: 0.5;
  flex-shrink: 0;
  transition: all $transition-fast;
}

.cm-copied {
  font-size: 0.7rem;
  font-weight: 700;
  color: $success;
  flex-shrink: 0;
  letter-spacing: 0.3px;
  &--error { color: $error; }
}

.social-links {
  display: flex;
  gap: $spacing-sm;
  margin-top: $spacing-lg;
}

.social-link {
  width: 36px;
  height: 36px;
  border-radius: $radius-sm;
  background: $bg-elevated;
  border: 1px solid $border;
  color: $text-muted;
  display: flex;
  align-items: center;
  justify-content: center;
  transition: all $transition-fast;

  &:hover { border-color: $primary; color: $primary; }
}

// Hours
.hours-list { display: flex; flex-direction: column; gap: 0; }
.hours-row {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 0.4rem 0;
  border-bottom: 1px solid $border;
  font-size: $font-size-sm;

  &:last-child { border-bottom: none; }
}
.hours-day { color: $text-secondary; font-weight: 500; }
.hours-time { color: $text; }
.hours-closed { color: $text-muted; font-style: italic; }

// Map
.map-section { margin-top: $spacing-3xl; }
.map-embed { border-radius: $radius-xl; display: block; }
</style>
