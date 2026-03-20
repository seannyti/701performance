<script setup lang="ts">
import { onMounted, computed, watchEffect } from 'vue'
import AppHeader from '@/components/layout/AppHeader.vue'
import AppFooter from '@/components/layout/AppFooter.vue'
import ChatWidget from '@/components/chat/ChatWidget.vue'
import { loadSettings, useSettings } from '@/composables/useSettings'

const { settings } = useSettings()

onMounted(async () => {
  await loadSettings()
})

const bar = computed(() => settings.value?.content?.announcementBar)
const maintenance = computed(() => settings.value?.advanced?.maintenanceModeEnabled ?? false)
const maintenanceMessage = computed(() =>
  settings.value?.advanced?.maintenanceModeMessage ||
  "We're performing scheduled maintenance. We'll be back shortly.")

// Apply theme CSS custom properties whenever settings change
watchEffect(() => {
  const t = settings.value?.theme
  const root = document.documentElement
  root.style.setProperty('--color-primary',      t?.primary     || '#e63946')
  root.style.setProperty('--color-primary-dark', t?.primaryDark || '#c1121f')
  root.style.setProperty('--color-primary-rgb',  t?.primaryRgb  || '230, 57, 70')
})
</script>

<template>
  <!-- Maintenance mode overlay -->
  <div v-if="maintenance" class="maintenance-page">
    <div class="maintenance-card">
      <div class="maintenance-logo">M Performance <strong>Power</strong></div>
      <i class="pi pi-wrench maintenance-icon" />
      <h1>Under Maintenance</h1>
      <p>{{ maintenanceMessage }}</p>
    </div>
  </div>

  <!-- Normal site -->
  <template v-else>
    <div
      v-if="bar?.enabled && bar.text"
      class="announcement-bar"
      :style="{ background: bar.bgColor, color: bar.textColor }"
    >
      {{ bar.text }}
    </div>
    <AppHeader />
    <main>
      <RouterView />
    </main>
    <AppFooter />
    <ChatWidget />
  </template>
</template>

<style>
* { box-sizing: border-box; margin: 0; padding: 0; }
html { font-size: 16px; }
body { background: #0f0f0f; color: #f0f0f0; font-family: Inter, system-ui, sans-serif; min-height: 100vh; }

.announcement-bar {
  width: 100%;
  text-align: center;
  padding: 10px 16px;
  font-size: 0.875rem;
  font-weight: 600;
  position: sticky;
  top: 0;
  z-index: 200;
}

.maintenance-page {
  min-height: 100vh;
  display: flex;
  align-items: center;
  justify-content: center;
  background: #0f0f0f;
  padding: 24px;
}

.maintenance-card {
  text-align: center;
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 16px;
  max-width: 440px;
}

.maintenance-logo {
  font-size: 1rem;
  font-weight: 800;
  color: #f0f0f0;
  margin-bottom: 8px;
}
.maintenance-logo strong { color: #e63946; }

.maintenance-icon {
  font-size: 3rem;
  color: #e63946;
}

.maintenance-card h1 {
  font-size: 1.75rem;
  font-weight: 800;
  color: #f0f0f0;
}

.maintenance-card p {
  font-size: 0.95rem;
  color: #9a9a9a;
  line-height: 1.6;
}
</style>
