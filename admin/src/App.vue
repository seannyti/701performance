<template>
  <div class="admin-app">
    <router-view />
  </div>
</template>

<script setup lang="ts">
import { onMounted, onUnmounted } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth'

const router = useRouter()
const authStore = useAuthStore()

// ── Inactivity / session-timeout ────────────────────────────────────────────
// Reads the session_timeout setting (minutes) from the API and auto-logs out
// after that period of inactivity. Resets on any mouse/keyboard/touch event.

const API_URL = `${import.meta.env.VITE_API_URL || 'http://localhost:5226'}/api/v1`
const DEFAULT_TIMEOUT_MS = 480 * 60_000 // 8 hours fallback

let inactivityTimer: ReturnType<typeof setTimeout> | null = null
let timeoutMs = DEFAULT_TIMEOUT_MS

const resetTimer = () => {
  if (!authStore.isAuthenticated) return
  if (inactivityTimer) clearTimeout(inactivityTimer)
  inactivityTimer = setTimeout(() => {
    authStore.logout()
    router.push('/login')
  }, timeoutMs)
}

const ACTIVITY_EVENTS = ['mousemove', 'mousedown', 'keydown', 'touchstart', 'scroll', 'click']

onMounted(async () => {
  // Fetch session_timeout setting from API
  try {
    const res = await fetch(`${API_URL}/settings`)
    if (res.ok) {
      const settings: { key: string; value: string }[] = await res.json()
      const entry = settings.find(s => s.key === 'session_timeout')
      if (entry && Number(entry.value) > 0) {
        timeoutMs = Number(entry.value) * 60_000
      }
    }
  } catch { /* use default */ }

  ACTIVITY_EVENTS.forEach(e => window.addEventListener(e, resetTimer, { passive: true }))
  resetTimer()
})

onUnmounted(() => {
  if (inactivityTimer) clearTimeout(inactivityTimer)
  ACTIVITY_EVENTS.forEach(e => window.removeEventListener(e, resetTimer))
})
</script>

<style scoped>
.admin-app {
  min-height: 100vh;
}
</style>