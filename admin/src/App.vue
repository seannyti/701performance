<template>
  <div class="admin-app">
    <router-view />
  </div>
</template>

<script setup lang="ts">
import { onMounted } from 'vue'
import { useAuthStore } from '@/stores/auth'

const authStore = useAuthStore()

onMounted(async () => {
  // Initialize authentication on app load
  await authStore.initializeAuth()
})
</script>

<style scoped>
.admin-app {
  min-height: 100vh;
}
</style>