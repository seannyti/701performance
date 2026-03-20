<script setup lang="ts">
import { onMounted } from 'vue'
import { RouterView } from 'vue-router'
import Toast from 'primevue/toast'
import { useChatStore } from '@/stores/chat.store'

const chatStore = useChatStore()

onMounted(async () => {
  const token = localStorage.getItem('mpp_token')
  if (token) {
    await chatStore.startHub(token)
    await chatStore.loadSessions()
  }
})
</script>

<template>
  <Toast />
  <RouterView />
</template>

<style>
* { box-sizing: border-box; margin: 0; padding: 0; }
html { font-size: 16px; }
body { background: #0f0f0f; color: #f0f0f0; font-family: Inter, system-ui, sans-serif; min-height: 100vh; }
</style>
