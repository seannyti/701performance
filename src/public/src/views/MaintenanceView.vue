<script setup lang="ts">
import { computed } from 'vue'
import { useSettingsStore } from '../stores/settings'

const settings = useSettingsStore()
const siteName = computed(() => settings.get('site_name', 'PerformancePower'))
</script>

<template>
  <div class="maintenance">
    <div class="maintenance__card">
      <div class="maintenance__icon">
        <svg viewBox="0 0 64 64" fill="none" xmlns="http://www.w3.org/2000/svg" width="56" height="56">
          <path d="M32 4L4 58h56L32 4z" stroke="currentColor" stroke-width="3" stroke-linejoin="round"/>
          <path d="M32 24v16M32 46v4" stroke="currentColor" stroke-width="3" stroke-linecap="round"/>
        </svg>
      </div>
      <h1 class="maintenance__title">{{ siteName }}</h1>
      <h2 class="maintenance__heading">Down for Maintenance</h2>
      <p class="maintenance__message">
        We're making improvements to give you a better experience.
        We'll be back online shortly — thank you for your patience.
      </p>
      <div class="maintenance__divider" />
      <div class="maintenance__footer">
        <a href="/" class="maintenance__check">Check again</a>
        <span class="maintenance__dot">·</span>
        <a href="/portal/" class="maintenance__staff">Staff login</a>
      </div>
    </div>
  </div>
</template>

<style lang="scss" scoped>
@use '../assets/styles/variables' as *;

.maintenance {
  min-height: 100vh;
  background: $bg;
  display: flex;
  align-items: center;
  justify-content: center;
  padding: $spacing-lg;
}

.maintenance__card {
  text-align: center;
  max-width: 480px;
  width: 100%;
  padding: $spacing-3xl $spacing-2xl;
  background: $bg-card;
  border: 1px solid $border;
  border-radius: $radius-xl;
}

.maintenance__icon {
  color: $primary;
  margin-bottom: $spacing-xl;
  display: flex;
  justify-content: center;
}

.maintenance__title {
  font-size: $font-size-sm;
  font-weight: 700;
  text-transform: uppercase;
  letter-spacing: 2px;
  color: $primary;
  margin-bottom: $spacing-md;
}

.maintenance__heading {
  font-size: $font-size-3xl;
  font-weight: 800;
  color: $text;
  margin-bottom: $spacing-lg;
  line-height: 1.2;
}

.maintenance__message {
  color: $text-muted;
  font-size: $font-size-base;
  line-height: 1.7;
}

.maintenance__divider {
  height: 1px;
  background: $border;
  margin: $spacing-xl 0;
}

.maintenance__footer {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 0.75rem;
  font-size: 0.75rem;
}

.maintenance__dot { color: $border; }

.maintenance__check {
  color: $text-secondary;
  transition: color $transition-fast;
  &:hover { color: $primary; }
}

.maintenance__staff {
  color: $text-muted;
  transition: color $transition-fast;
  &:hover { color: $text-secondary; }
}
</style>
