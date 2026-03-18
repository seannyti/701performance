import { defineStore } from 'pinia'
import { ref } from 'vue'
import * as signalR from '@microsoft/signalr'
import axios from 'axios'
import { useAuthStore } from './auth'

const API_URL = import.meta.env.VITE_API_URL || 'http://localhost:5226'
const HUB_URL = `${API_URL}/hubs/chat`
const STORAGE_KEY = 'chat_session'

export interface ChatMessage {
  id: number
  sessionId: number
  senderName: string
  senderRole: 'Customer' | 'Agent' | 'System'
  body: string
  sentAt: string
}

interface PersistedSession {
  sessionId: number
  sessionToken: string
  status: 'waiting' | 'active' | 'closed'
  messages: ChatMessage[]
}

function loadFromStorage(): PersistedSession | null {
  try {
    const raw = sessionStorage.getItem(STORAGE_KEY)
    return raw ? JSON.parse(raw) : null
  } catch {
    return null
  }
}

function saveToStorage(data: PersistedSession) {
  sessionStorage.setItem(STORAGE_KEY, JSON.stringify(data))
}

function clearStorage() {
  sessionStorage.removeItem(STORAGE_KEY)
}

export const useChatStore = defineStore('chat', () => {
  const authStore = useAuthStore()

  // ── State ─────────────────────────────────────────────────────────────────
  const isOpen = ref(false)
  const sessionId = ref<number | null>(null)
  const sessionToken = ref<string | null>(null)
  const messages = ref<ChatMessage[]>([])
  const status = ref<'idle' | 'waiting' | 'active' | 'closed'>('idle')
  const isConnecting = ref(false)
  const error = ref('')

  let connection: signalR.HubConnection | null = null

  // ── Helpers ───────────────────────────────────────────────────────────────
  function persist() {
    if (sessionId.value && sessionToken.value && status.value !== 'idle') {
      saveToStorage({
        sessionId: sessionId.value,
        sessionToken: sessionToken.value,
        status: status.value as 'waiting' | 'active' | 'closed',
        messages: messages.value
      })
    }
  }

  // Suppress noisy reconnect errors from flooding the console.
  // SignalR logs each failed negotiation attempt at Error level — we only
  // want to surface Critical-level messages (true hub faults, not transient 502s).
  const silentLogger: signalR.ILogger = {
    log(level: signalR.LogLevel, message: string) {
      if (level >= signalR.LogLevel.Critical) console.error('[SignalR]', message)
    }
  }

  function buildConnection(): signalR.HubConnection {
    const builder = new signalR.HubConnectionBuilder()
      .withUrl(HUB_URL, {
        accessTokenFactory: () => authStore.token ?? '',
        transport: signalR.HttpTransportType.WebSockets | signalR.HttpTransportType.LongPolling
      })
      .withAutomaticReconnect()
      .configureLogging(silentLogger)
      .build()

    builder.on('ReceiveMessage', (msg: ChatMessage) => {
      messages.value.push(msg)
      persist()
    })

    builder.on('SessionActivated', () => {
      status.value = 'active'
      persist()
    })

    builder.on('SessionClosed', () => {
      status.value = 'closed'
      persist()
    })

    builder.onreconnecting(() => { isConnecting.value = true })
    builder.onreconnected(() => { isConnecting.value = false })
    builder.onclose(() => { isConnecting.value = false })

    return builder
  }

  // ── Actions ───────────────────────────────────────────────────────────────

  /** Open the widget. */
  function openChat() { isOpen.value = true }

  /** Close/minimise the widget without ending the session. */
  function closeChat() { isOpen.value = false }

  /**
   * Called on app mount — restores an in-progress session from sessionStorage
   * and reconnects to the SignalR hub.
   */
  async function restoreSession() {
    const saved = loadFromStorage()
    if (!saved) return

    sessionId.value = saved.sessionId
    sessionToken.value = saved.sessionToken
    status.value = saved.status
    messages.value = saved.messages

    // If session was already closed, just restore UI — no need to reconnect
    if (saved.status === 'closed') return

    try {
      isConnecting.value = true
      connection = buildConnection()
      await connection.start()
      await connection.invoke('JoinSession', saved.sessionId, saved.sessionToken)
    } catch {
      // If reconnect fails the session is gone — reset to idle
      clearStorage()
      sessionId.value = null
      sessionToken.value = null
      messages.value = []
      status.value = 'idle'
    } finally {
      isConnecting.value = false
    }
  }

  /**
   * Start a new chat session (called when the user submits the start form).
   */
  async function startSession(name: string, email?: string) {
    isConnecting.value = true
    error.value = ''

    try {
      const res = await axios.post(`${API_URL}/api/v1/chat/sessions`, { name, email })
      sessionId.value = res.data.sessionId
      sessionToken.value = res.data.sessionToken
      status.value = 'waiting'
      messages.value = []

      persist()

      connection = buildConnection()
      await connection.start()
      await connection.invoke('JoinSession', sessionId.value, sessionToken.value)
    } catch (err: any) {
      error.value = err.message || 'Failed to start chat session'
      status.value = 'idle'
    } finally {
      isConnecting.value = false
    }
  }

  /** Send a message in the current session. */
  async function sendMessage(body: string) {
    if (!connection || !sessionId.value || !body.trim()) return
    try {
      await connection.invoke('SendMessage', sessionId.value, body)
    } catch (err: any) {
      error.value = err.message || 'Failed to send message'
    }
  }

  /** Disconnect and reset the widget to idle. */
  async function endSession() {
    if (connection) {
      await connection.stop()
      connection = null
    }
    clearStorage()
    sessionId.value = null
    sessionToken.value = null
    messages.value = []
    status.value = 'idle'
    isOpen.value = false
  }

  return {
    isOpen,
    sessionId,
    sessionToken,
    messages,
    status,
    isConnecting,
    error,
    openChat,
    closeChat,
    restoreSession,
    startSession,
    sendMessage,
    endSession
  }
})
