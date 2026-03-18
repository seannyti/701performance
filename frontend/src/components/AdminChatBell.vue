<template>
  <Teleport to="body">
    <button
      v-if="!hideOnPage"
      class="admin-bell"
      :class="{ 'has-sessions': openCount > 0 }"
      :title="openCount > 0 ? `${openCount} active chat session${openCount !== 1 ? 's' : ''}` : 'Live Chat — no active sessions'"
      @click="openAdminChat"
      aria-label="Live chat admin"
    >
      💬
      <span v-if="openCount > 0" class="bell-badge">{{ openCount > 99 ? '99+' : openCount }}</span>
    </button>
  </Teleport>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, onUnmounted } from 'vue'
import { useRoute } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import * as signalR from '@microsoft/signalr'
import axios from 'axios'

const API_URL  = import.meta.env.VITE_API_URL  || 'http://localhost:5226'
const ADMIN_URL = import.meta.env.VITE_ADMIN_URL || 'http://localhost:81'
const HUB_URL  = `${API_URL}/hubs/chat`

const authStore = useAuthStore()
const route = useRoute()

const openCount = ref(0)
let connection: signalR.HubConnection | null = null

const hideOnPage = computed(() => {
  const hidden = ['/maintenance', '/login', '/signup']
  return hidden.includes(route.path)
})

function openAdminChat() {
  window.open(`${ADMIN_URL}/live-chat`, '_blank', 'noopener')
}

async function loadCount() {
  try {
    const res = await axios.get(`${API_URL}/api/v1/chat/sessions`, {
      headers: { Authorization: `Bearer ${authStore.token}` }
    })
    openCount.value = (res.data as { status: string }[]).filter(
      s => s.status === 'Waiting' || s.status === 'Active'
    ).length
  } catch {
    // silently ignore — count stays at 0
  }
}

async function connectHub() {
  connection = new signalR.HubConnectionBuilder()
    .withUrl(HUB_URL, {
      accessTokenFactory: () => authStore.token ?? '',
      transport: signalR.HttpTransportType.WebSockets | signalR.HttpTransportType.LongPolling
    })
    .withAutomaticReconnect()
    .configureLogging(signalR.LogLevel.Warning)
    .build()

  connection.on('NewSession', () => {
    openCount.value++
  })

  connection.on('SessionClosed', () => {
    openCount.value = Math.max(0, openCount.value - 1)
  })

  try {
    await connection.start()
    await connection.invoke('JoinAgentsRoom')
  } catch {
    // Hub connection failed — bell still shows last known count
  }
}

onMounted(async () => {
  await Promise.all([loadCount(), connectHub()])
})

onUnmounted(async () => {
  if (connection) await connection.stop()
})
</script>

<style scoped>
.admin-bell {
  position: fixed;
  bottom: 1.25rem;
  left: 1.5rem;
  z-index: 10001;
  width: 3.25rem;
  height: 3.25rem;
  border-radius: 50%;
  border: none;
  background: var(--color-primary, #6366f1);
  color: #fff;
  font-size: 1.25rem;
  cursor: pointer;
  box-shadow: 0 4px 16px rgba(0, 0, 0, 0.25);
  display: flex;
  align-items: center;
  justify-content: center;
  transition: transform 0.2s, background 0.2s;
}

.admin-bell:hover {
  transform: scale(1.08);
  background: var(--color-secondary, #ec4899);
}

.admin-bell.has-sessions {
  animation: pulse 2s ease-in-out infinite;
}

@keyframes pulse {
  0%, 100% { box-shadow: 0 4px 16px rgba(0, 0, 0, 0.25); }
  50%       { box-shadow: 0 4px 24px rgba(99, 102, 241, 0.6); }
}

.bell-badge {
  position: absolute;
  top: -0.2rem;
  right: -0.2rem;
  background: #ef4444;
  color: #fff;
  font-size: 0.65rem;
  font-weight: 700;
  border-radius: 999px;
  min-width: 1.1rem;
  height: 1.1rem;
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 0 0.2rem;
  line-height: 1;
  border: 2px solid #fff;
}
</style>
