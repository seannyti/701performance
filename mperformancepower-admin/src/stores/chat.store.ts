import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import * as signalR from '@microsoft/signalr'

const API_URL = import.meta.env.VITE_API_URL as string
const HUB_URL = API_URL.replace('/api', '') + '/api/hubs/chat'

export interface ChatSession {
  id: string
  visitorName: string
  visitorEmail: string
  status: 'Active' | 'Closed'
  startedAt: string
  lastMessageAt: string
}

export interface ChatMessage {
  id?: number
  sessionId: string
  senderType: 'Visitor' | 'Admin'
  senderName: string
  content: string
  sentAt: string
}

export const useChatStore = defineStore('chat', () => {
  // ── Unread counts ─────────────────────────────────────────────────
  const unreadMap = ref<Record<string, number>>({})
  const totalUnread = computed(() =>
    Object.values(unreadMap.value).reduce((a, b) => a + b, 0)
  )

  // ── Session + message state (used by ChatView) ────────────────────
  const sessions = ref<ChatSession[]>([])
  const activeSession = ref<ChatSession | null>(null)
  const messages = ref<ChatMessage[]>([])

  // ── Hub connection ────────────────────────────────────────────────
  let connection: signalR.HubConnection | null = null

  async function startHub(token: string) {
    if (connection?.state === signalR.HubConnectionState.Connected) return

    connection = new signalR.HubConnectionBuilder()
      .withUrl(HUB_URL, { accessTokenFactory: () => token })
      .withAutomaticReconnect()
      .configureLogging(signalR.LogLevel.Warning)
      .build()

    // ── Always-on handlers (sidebar badge, etc.) ───────────────────
    connection.on('NewVisitorMessage', ({ sessionId, lastMessageAt }: { sessionId: string; lastMessageAt: string }) => {
      const s = sessions.value.find(s => s.id === sessionId)
      if (s) {
        s.lastMessageAt = lastMessageAt
        sortSessions()
      }
      if (activeSession.value?.id !== sessionId) {
        unreadMap.value[sessionId] = (unreadMap.value[sessionId] ?? 0) + 1
      }
    })

    connection.on('SessionStarted', (session: ChatSession) => {
      if (!sessions.value.find(s => s.id === session.id)) {
        sessions.value.unshift(session)
      }
      // Count as unread since no admin is viewing it yet
      unreadMap.value[session.id] = (unreadMap.value[session.id] ?? 0) + 1
    })

    connection.on('SessionUpdated', ({ sessionId, lastMessageAt }: { sessionId: string; lastMessageAt: string }) => {
      const s = sessions.value.find(s => s.id === sessionId)
      if (s) {
        s.lastMessageAt = lastMessageAt
        sortSessions()
      }
    })

    connection.on('ReceiveMessage', (msg: ChatMessage) => {
      if (activeSession.value?.id === msg.sessionId) {
        messages.value.push(msg)
      }
    })

    connection.on('SessionClosed', (sessionId: string) => {
      const s = sessions.value.find(s => s.id === sessionId)
      if (s) s.status = 'Closed'
      if (activeSession.value?.id === sessionId) activeSession.value.status = 'Closed'
    })

    await connection.start()
    await connection.invoke('AdminJoin')
  }

  async function stopHub() {
    await connection?.stop()
    connection = null
  }

  // ── Actions used by ChatView ──────────────────────────────────────
  async function loadSessions() {
    const token = localStorage.getItem('mpp_token')
    const res = await fetch(`${API_URL}/chat/sessions`, {
      headers: { Authorization: `Bearer ${token}` },
    })
    sessions.value = await res.json()
  }

  async function selectSession(session: ChatSession) {
    activeSession.value = session
    unreadMap.value[session.id] = 0
    messages.value = []

    const token = localStorage.getItem('mpp_token')
    const res = await fetch(`${API_URL}/chat/sessions/${session.id}/messages`, {
      headers: { Authorization: `Bearer ${token}` },
    })
    const data = await res.json()
    messages.value = data.messages

    await connection?.invoke('AdminJoinSession', session.id)
  }

  async function sendReply(content: string) {
    if (!content || !activeSession.value) return
    await connection?.invoke('AdminReply', activeSession.value.id, content)
  }

  async function closeSession() {
    if (!activeSession.value) return
    await connection?.invoke('AdminCloseSession', activeSession.value.id)
  }

  async function deleteSession(sessionId: string) {
    const token = localStorage.getItem('mpp_token')
    await fetch(`${API_URL}/chat/sessions/${sessionId}`, {
      method: 'DELETE',
      headers: { Authorization: `Bearer ${token}` },
    })
    sessions.value = sessions.value.filter(s => s.id !== sessionId)
    if (activeSession.value?.id === sessionId) {
      activeSession.value = null
      messages.value = []
    }
    delete unreadMap.value[sessionId]
  }

  function sortSessions() {
    sessions.value.sort((a, b) => new Date(b.lastMessageAt).getTime() - new Date(a.lastMessageAt).getTime())
  }

  return {
    unreadMap, totalUnread,
    sessions, activeSession, messages,
    startHub, stopHub,
    loadSessions, selectSession, sendReply, closeSession, deleteSession,
  }
})
