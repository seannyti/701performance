<script setup lang="ts">
import AppNavbar from '../components/navbar/AppNavbar.vue'
import AppFooter from '../components/common/AppFooter.vue'
import AnnouncementBanner from '../components/common/AnnouncementBanner.vue'
import { useSettingsStore } from '../stores/settings'

const settings = useSettingsStore()
</script>

<template>
  <div class="layout">
    <div class="site-header">
      <AnnouncementBanner v-if="settings.get('announcement_enabled') === 'true'" />
      <AppNavbar />
    </div>
    <main class="main-content">
      <RouterView />
    </main>
    <AppFooter />
  </div>
</template>

<style lang="scss" scoped>
.layout {
  min-height: 100vh;
  display: flex;
  flex-direction: column;
}
.site-header {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  z-index: 100;
}
.main-content {
  flex: 1;
}
</style>
