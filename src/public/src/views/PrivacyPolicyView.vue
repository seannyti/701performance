<script setup lang="ts">
import { computed } from 'vue'
import { useSettingsStore } from '../stores/settings'

const settings = useSettingsStore()
const privacyContent = computed(() => settings.get('privacy_content', ''))
</script>

<template>
  <div class="privacy-page">
    <div class="privacy-hero">
      <div class="container">
        <p class="privacy-hero__label">Legal</p>
        <h1>Privacy Policy</h1>
        <p class="privacy-hero__sub">How we collect, use, and protect your personal information.</p>
      </div>
    </div>

    <div class="container privacy-body">
      <div class="privacy-card">
        <div v-if="privacyContent" class="privacy-content">{{ privacyContent }}</div>
        <div v-else class="privacy-empty">
          <i class="pi pi-shield" />
          <p>Privacy Policy has not been configured yet.</p>
        </div>
      </div>
    </div>
  </div>
</template>

<style lang="scss" scoped>
@use '../assets/styles/variables' as *;

.privacy-page {
  min-height: 60vh;
}

.privacy-hero {
  background: linear-gradient(180deg, #111 0%, $bg 100%);
  border-bottom: 1px solid $border;
  padding: $spacing-3xl $spacing-lg $spacing-2xl;

  h1 {
    font-size: clamp(2rem, 5vw, 3rem);
    font-weight: 900;
    color: $text;
    margin-bottom: $spacing-sm;
    line-height: 1.1;
  }
}

.privacy-hero__label {
  font-size: 0.7rem;
  font-weight: 700;
  text-transform: uppercase;
  letter-spacing: 2.5px;
  color: $primary;
  margin-bottom: $spacing-sm;
}

.privacy-hero__sub {
  color: $text-muted;
  font-size: $font-size-base;
}

.privacy-body {
  padding-top: $spacing-3xl;
  padding-bottom: $spacing-3xl;
}

.privacy-card {
  max-width: 800px;
  background: $bg-card;
  border: 1px solid $border;
  border-radius: $radius-xl;
  padding: $spacing-3xl $spacing-2xl;
}

.privacy-content {
  color: $text-secondary;
  font-size: $font-size-base;
  line-height: 1.85;
  white-space: pre-wrap;
  font-family: inherit;
}

.privacy-empty {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: $spacing-md;
  padding: $spacing-3xl;
  color: $text-muted;

  i { font-size: 2.5rem; }
  p { font-size: $font-size-sm; }
}
</style>
