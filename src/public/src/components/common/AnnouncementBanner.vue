<script setup lang="ts">
import { ref } from 'vue'
import { useSettingsStore } from '../../stores/settings'
const settings = useSettingsStore()
const dismissed = ref(false)
</script>
<template>
  <div v-if="!dismissed" class="banner">
    <span>{{ settings.get('announcement_text') }}</span>
    <button @click="dismissed = true" aria-label="Dismiss">✕</button>
  </div>
</template>
<style scoped>
.banner { background: #e53935; color: white; text-align: center; padding: 0.5rem 1rem; font-size: 0.875rem; font-weight: 500; display: flex; align-items: center; justify-content: center; gap: 1rem; }
button { background: none; border: none; color: white; cursor: pointer; font-size: 1rem; }
</style>
