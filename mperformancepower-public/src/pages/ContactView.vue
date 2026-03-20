<script setup lang="ts">
import { computed } from 'vue'
import InquiryForm from '@/components/inquiry/InquiryForm.vue'
import { useSettings } from '@/composables/useSettings'

const { settings } = useSettings()

const g = computed(() => settings.value?.general)
const contact = computed(() => settings.value?.pages?.contact)

const phone = computed(() => g.value?.phone || '701-989-1494')
const email = computed(() => g.value?.email || 'contact@minotperformancepowersports.com')
const address = computed(() => g.value?.address || '6210 HWY 2 E, Minot, ND 58701')
const hours = computed(() => g.value?.hours || 'Mon–Fri: 9am – 6pm\nSat: 9am – 4pm\nSun: Closed')
const pageTitle = computed(() => contact.value?.title || 'Contact Us')
const pageIntro = computed(() => contact.value?.intro || 'Have a question about a vehicle or want to schedule a visit? We\'re here to help.')
</script>

<template>
  <div>
    <section class="contact-hero">
      <div class="container">
        <p class="contact-hero__label">Get In Touch</p>
        <h1 class="contact-hero__heading">{{ pageTitle }}</h1>
        <p class="contact-hero__sub">{{ pageIntro }}</p>
      </div>
    </section>

    <section class="contact-section">
      <div class="container contact-grid">
        <InquiryForm />

        <div class="contact-info">
          <h2 class="contact-info__heading">Contact Information</h2>

          <div class="contact-info__items">
            <div class="contact-info__item">
              <span class="contact-info__icon">📍</span>
              <div>
                <p class="contact-info__label">Location</p>
                <p class="contact-info__value">{{ address }}</p>
              </div>
            </div>

            <div class="contact-info__item">
              <span class="contact-info__icon">📞</span>
              <div>
                <p class="contact-info__label">Phone</p>
                <a :href="`tel:+1${phone.replace(/\D/g, '')}`" class="contact-info__value">{{ phone }}</a>
              </div>
            </div>

            <div class="contact-info__item">
              <span class="contact-info__icon">✉️</span>
              <div>
                <p class="contact-info__label">Email</p>
                <a :href="`mailto:${email}`" class="contact-info__value">{{ email }}</a>
              </div>
            </div>

            <div class="contact-info__item">
              <span class="contact-info__icon">🕐</span>
              <div>
                <p class="contact-info__label">Hours</p>
                <p class="contact-info__value" style="white-space: pre-line">{{ hours }}</p>
              </div>
            </div>
          </div>
        </div>
      </div>
    </section>
  </div>
</template>

<style lang="scss" scoped>
@use '@/styles/variables' as *;
@use '@/styles/mixins' as *;

.contact-hero {
  background: $color-surface;
  border-bottom: 1px solid $color-border;
  padding: 5rem 0 4rem;

  &__label {
    font-size: 0.75rem;
    font-weight: 700;
    text-transform: uppercase;
    letter-spacing: 0.1em;
    color: var(--color-primary);
    margin-bottom: 1rem;
  }

  &__heading {
    font-size: clamp(2rem, 4vw, 3rem);
    font-weight: 900;
    color: $color-text;
    margin-bottom: 1rem;
    line-height: 1.2;
  }

  &__sub {
    font-size: 1rem;
    color: $color-muted;
    max-width: 520px;
    line-height: 1.7;
  }
}

.contact-section {
  padding: 5rem 0;
}

.contact-grid {
  display: grid;
  grid-template-columns: 1.5fr 1fr;
  gap: 4rem;
  align-items: start;

  @media (max-width: 768px) {
    grid-template-columns: 1fr;
    gap: 2.5rem;
  }
}

.contact-info {
  &__heading {
    font-size: 1.25rem;
    font-weight: 700;
    color: $color-text;
    margin-bottom: 2rem;
  }

  &__items {
    display: flex;
    flex-direction: column;
    gap: 1.75rem;
  }

  &__item {
    display: flex;
    gap: 1rem;
    align-items: flex-start;
  }

  &__icon {
    font-size: 1.25rem;
    flex-shrink: 0;
    margin-top: 2px;
  }

  &__label {
    font-size: 0.75rem;
    font-weight: 700;
    text-transform: uppercase;
    letter-spacing: 0.08em;
    color: $color-muted;
    margin-bottom: 0.25rem;
  }

  &__value {
    font-size: 0.95rem;
    color: $color-text;
    line-height: 1.6;

    &[href] {
      transition: color 0.15s;
      &:hover { color: var(--color-primary); }
    }
  }
}
</style>
