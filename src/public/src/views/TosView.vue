<script setup lang="ts">
import { computed } from 'vue'
import { useSettingsStore } from '../stores/settings'

const settings = useSettingsStore()
const tosContent = computed(() => settings.get('tos_content', ''))
</script>

<template>
  <div class="tos-page">
    <div class="tos-hero">
      <div class="container">
        <p class="tos-hero__label">Legal</p>
        <h1>Terms of Service</h1>
        <p class="tos-hero__sub">Please read these terms carefully before using our services.</p>
      </div>
    </div>

    <div class="container tos-body">
      <div class="tos-card">
        <div v-if="tosContent" class="tos-content">{{ tosContent }}</div>
        <div v-else class="tos-empty">
          <i class="pi pi-file-edit" />
          <p>Terms of Service have not been configured yet.</p>
        </div>
      </div>
    </div>
  </div>
</template>

<style lang="scss" scoped>
@use '../assets/styles/variables' as *;

.tos-page {
  min-height: 60vh;
}

.tos-hero {
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

.tos-hero__label {
  font-size: 0.7rem;
  font-weight: 700;
  text-transform: uppercase;
  letter-spacing: 2.5px;
  color: $primary;
  margin-bottom: $spacing-sm;
}

.tos-hero__sub {
  color: $text-muted;
  font-size: $font-size-base;
}

.tos-body {
  padding-top: $spacing-3xl;
  padding-bottom: $spacing-3xl;
}

.tos-card {
  max-width: 800px;
  background: $bg-card;
  border: 1px solid $border;
  border-radius: $radius-xl;
  padding: $spacing-3xl $spacing-2xl;
}

.tos-content {
  color: $text-secondary;
  font-size: $font-size-base;
  line-height: 1.85;
  white-space: pre-wrap;
  font-family: inherit;
}

.tos-empty {
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
