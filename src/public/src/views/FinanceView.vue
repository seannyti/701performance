<script setup lang="ts">
import { computed } from 'vue'
import { useSettingsStore } from '../stores/settings'

const settings = useSettingsStore()

const synchronyUrl = computed(() => settings.get('synchrony_url', '#'))
const octaneUrl = computed(() => settings.get('octane_embed_url', '#'))
const synchronyDesc = computed(() => settings.get('lender_synchrony_description', 'Synchrony offers competitive rates and flexible terms for powersports financing. Whether you\'re buying new or used, Synchrony makes it simple to get approved.'))
const octaneDesc = computed(() => settings.get('lender_octane_description', 'Octane specializes in powersports lending. With a quick pre-qualification that won\'t affect your credit score, you\'ll know your options before you even step in the door.'))

const synchronyFeatures = [
  'Fast online application',
  'Competitive interest rates',
  'Flexible repayment terms',
  'New & used vehicles',
]

const octaneFeatures = [
  'Soft pull pre-qualification',
  'Decisions in seconds',
  'Powersports specialists',
  'New & used vehicles',
]
</script>

<template>
  <div class="finance-page">
    <div class="finance-hero">
      <h1 class="finance-hero__title">Financing Made Easy</h1>
      <p class="finance-hero__subtitle">
        We work with two trusted lenders to get you on the road — or trail — fast.<br>
        Apply online in minutes and get a decision the same day.
      </p>
    </div>

    <div class="container">
      <div class="lender-grid">
        <!-- Synchrony -->
        <div class="lender-card">
          <div class="lender-card__header">
            <div class="lender-logo lender-logo--synchrony">
              <svg viewBox="0 0 32 32" fill="none"><circle cx="16" cy="16" r="16" fill="#00a19a"/><circle cx="16" cy="16" r="8" stroke="white" stroke-width="2.5" fill="none"/><circle cx="16" cy="16" r="3" fill="white"/></svg>
            </div>
            <span class="lender-name">Synchrony</span>
          </div>
          <p class="lender-desc">{{ synchronyDesc }}</p>
          <ul class="lender-features">
            <li v-for="f in synchronyFeatures" :key="f">
              <span class="check">✓</span> {{ f }}
            </li>
          </ul>
          <a :href="synchronyUrl" target="_blank" rel="noopener" class="lender-btn">
            Apply with Synchrony
          </a>
        </div>

        <!-- Octane -->
        <div class="lender-card">
          <div class="lender-card__header">
            <div class="lender-logo lender-logo--octane">
              <svg viewBox="0 0 32 32" fill="none"><rect width="32" height="32" rx="6" fill="#ff5a1f"/><path d="M10 10h6l6 6-6 6h-6l6-6-6-6z" fill="white"/></svg>
            </div>
            <span class="lender-name">Octane</span>
          </div>
          <p class="lender-desc">{{ octaneDesc }}</p>
          <ul class="lender-features">
            <li v-for="f in octaneFeatures" :key="f">
              <span class="check">✓</span> {{ f }}
            </li>
          </ul>
          <a :href="octaneUrl" target="_blank" rel="noopener" class="lender-btn">
            Apply with Octane
          </a>
        </div>
      </div>

      <p class="finance-footer">
        Not sure which lender is right for you?
        <RouterLink to="/contact" class="contact-link">Contact us</RouterLink>
        and we'll help you find the best option for your situation.
      </p>
    </div>
  </div>
</template>

<style lang="scss" scoped>
@use '../assets/styles/variables' as *;
@use '../assets/styles/mixins' as *;

.finance-page {
  padding-top: $navbar-height;
  min-height: 100vh;
  background: $bg;
}

.finance-hero {
  text-align: center;
  padding: $spacing-3xl $spacing-lg;

  &__title {
    font-size: clamp($font-size-3xl, 5vw, $font-size-5xl);
    font-weight: 900;
    margin-bottom: $spacing-lg;
  }

  &__subtitle {
    color: $text-muted;
    font-size: $font-size-lg;
    line-height: 1.8;
  }
}

.lender-grid {
  display: grid;
  grid-template-columns: 1fr;
  gap: $spacing-xl;
  max-width: 880px;
  margin: 0 auto $spacing-2xl;

  @include respond-to(md) {
    grid-template-columns: 1fr 1fr;
  }
}

.lender-card {
  @include card;
  display: flex;
  flex-direction: column;
  gap: $spacing-lg;

  &__header {
    display: flex;
    align-items: center;
    gap: $spacing-md;
    padding-bottom: $spacing-lg;
    border-bottom: 1px solid $border;
  }
}

.lender-logo {
  width: 40px;
  height: 40px;
  flex-shrink: 0;
  svg { width: 100%; height: 100%; }
}

.lender-name {
  font-size: $font-size-xl;
  font-weight: 700;
}

.lender-desc {
  color: $text-muted;
  font-size: $font-size-sm;
  line-height: 1.7;
}

.lender-features {
  list-style: none;
  display: flex;
  flex-direction: column;
  gap: $spacing-sm;
  flex: 1;

  li {
    font-size: $font-size-sm;
    color: $text-secondary;
    display: flex;
    align-items: center;
    gap: $spacing-sm;
  }
}

.check {
  color: $primary;
  font-weight: 700;
  flex-shrink: 0;
}

.lender-btn {
  display: block;
  text-align: center;
  background: $primary;
  color: white;
  font-weight: 700;
  padding: 1rem;
  border-radius: $radius-md;
  transition: background $transition-fast;
  margin-top: auto;

  &:hover { background: $primary-dark; }
}

.finance-footer {
  text-align: center;
  color: $text-muted;
  font-size: $font-size-sm;
  padding-bottom: $spacing-3xl;
}

.contact-link {
  color: $primary;
  font-weight: 600;
  &:hover { text-decoration: underline; }
}
</style>
