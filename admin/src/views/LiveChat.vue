<template>
  <AdminLayout>
    <div class="livechat-page">
      <h1 class="page-title">💬 Live Chat</h1>

      <div class="livechat-layout" :class="{ 'has-session': activeSessionId }">
        <!-- ── Session sidebar ───────────────────────────────────────── -->
        <aside class="session-sidebar">
          <div class="sidebar-toolbar">
            <button
              class="filter-btn"
              :class="{ active: filter === 'open' }"
              @click="filter = 'open'"
            >Open</button>
            <button
              class="filter-btn"
              :class="{ active: filter === 'closed' }"
              @click="filter = 'closed'"
            >Closed</button>
          </div>

          <div v-if="loadingSessions" class="sidebar-state">Loading…</div>
          <div v-else-if="filteredSessions.length === 0" class="sidebar-state muted">
            No {{ filter }} sessions
          </div>

          <ul v-else class="session-list">
            <li
              v-for="s in filteredSessions"
              :key="s.id"
              class="session-item"
              :class="{ active: activeSessionId === s.id }"
              @click="openSession(s.id)"
            >
              <div class="session-name">
                {{ s.guestName || s.userId && `User #${s.userId}` || 'Guest' }}
                <span v-if="s.unreadCount && s.status !== 'Closed'" class="unread-badge">{{ s.unreadCount }}</span>
              </div>
              <div class="session-meta">
                <span class="session-status" :class="`status-${s.status.toLowerCase()}`">
                  {{ s.status }}
                </span>
                <span class="session-time">{{ formatTime(s.createdAt) }}</span>
              </div>
              <div v-if="s.lastMessage" class="session-preview">{{ s.lastMessage }}</div>
              <button
                v-if="s.status === 'Closed' && authStore.isSuperAdmin"
                class="delete-session-btn"
                @click.stop="deleteSession(s.id)"
                title="Permanently delete"
              >Delete</button>
            </li>
          </ul>
        </aside>

        <!-- ── Message panel ─────────────────────────────────────────── -->
        <section class="message-panel">
          <div v-if="!activeSessionId" class="panel-empty">
            Select a session to view the conversation.
          </div>

          <template v-else>
            <!-- Panel header -->
            <div class="panel-header">
              <button class="back-btn" @click="activeSessionId = null">← Back</button>
              <div>
                <span class="panel-title">
                  {{ activeSession?.guestName || (activeSession?.userId ? `User #${activeSession.userId}` : 'Guest') }}
                </span>
                <span v-if="activeSession?.guestEmail" class="panel-email">
                  — {{ activeSession.guestEmail }}
                </span>
                <span class="panel-status" :class="`status-${activeSession?.status.toLowerCase()}`">
                  {{ activeSession?.status }}
                </span>
              </div>
              <button
                v-if="activeSession?.status !== 'Closed'"
                class="close-btn"
                @click="closeSession"
              >
                Close Session
              </button>
            </div>

            <!-- Messages -->
            <div class="messages-area" ref="messagesEl">
              <div v-if="loadingMessages" class="panel-state">Loading messages…</div>

              <template v-else>
                <div v-if="activeSession?.status === 'Waiting'" class="system-msg">
                  Waiting — customer is in queue.
                </div>

                <div
                  v-for="msg in activeMessages"
                  :key="msg.id"
                  class="bubble-wrap"
                  :class="msg.senderRole === 'Customer' ? 'is-customer' : 'is-agent'"
                >
                  <div class="bubble">
                    <span class="bubble-sender">
                      {{ msg.senderRole === 'Agent' ? msg.senderName : (activeSession?.guestName || 'Customer') }}
                    </span>
                    <p>{{ msg.body }}</p>
                    <span class="bubble-time">{{ formatTimeFull(msg.sentAt) }}</span>
                  </div>
                </div>

                <div v-if="activeSession?.status === 'Closed'" class="system-msg">
                  Session closed.
                </div>
              </template>
            </div>

            <!-- Reply input -->
            <div v-if="activeSession?.status !== 'Closed'" class="reply-area">
              <div class="quick-replies">
                <button
                  v-for="qr in quickReplies"
                  :key="qr"
                  class="qr-btn"
                  @click="draft = qr"
                >{{ qr }}</button>
              </div>
              <div class="reply-row">
                <input
                  v-model="draft"
                  class="reply-input"
                  placeholder="Reply to customer…"
                  maxlength="2000"
                  @keydown.enter.prevent="sendReply"
                />
                <button class="reply-btn" :disabled="!draft.trim()" @click="sendReply">Send</button>
              </div>
            </div>
          </template>
        </section>
      </div>
    </div>

    <!-- Confirm Modal -->
    <div v-if="confirmModal.show" class="confirm-modal-backdrop" @click.self="closeConfirmModal">
      <div class="confirm-modal-box">
        <div class="confirm-modal-header">
          <h2>{{ confirmModal.title }}</h2>
          <button @click="closeConfirmModal" class="confirm-close-btn">&times;</button>
        </div>
        <div class="confirm-modal-body">
          <p>{{ confirmModal.message }}</p>
          <div class="confirm-modal-footer">
            <button @click="closeConfirmModal" class="confirm-btn confirm-btn-secondary">Cancel</button>
            <button @click="executeConfirmModal" :class="confirmModal.dangerous ? 'confirm-btn confirm-btn-danger' : 'confirm-btn confirm-btn-primary'">Confirm</button>
          </div>
        </div>
      </div>
    </div>
  </AdminLayout>
</template>

<script setup lang="ts">
import { ref, reactive, computed, watch, onMounted, onUnmounted, nextTick } from 'vue'
import * as signalR from '@microsoft/signalr'
import AdminLayout from '@/components/AdminLayout.vue'
import { apiGet, apiPatch, apiDelete } from '@/utils/apiClient'
import { useAuthStore } from '@/stores/auth'
import { useToast } from '@/composables/useToast'
import { logError } from '@/services/logger'

const HUB_URL = `${import.meta.env.VITE_API_URL || 'http://localhost:5226'}/hubs/chat`

const authStore = useAuthStore()
const toast = useToast()

// ── State ─────────────────────────────────────────────────────────────────
interface Session {
  id: number
  userId?: number
  guestName?: string
  guestEmail?: string
  status: string
  createdAt: string
  closedAt?: string
  unreadCount: number
  lastMessage?: string
}

interface Message {
  id: number
  senderName: string
  senderRole: 'Customer' | 'Agent' | 'System'
  body: string
  sentAt: string
  isRead: boolean
}

interface SessionDetail extends Session {
  messages: Message[]
}

const confirmModal = reactive({ show: false, title: '', message: '', dangerous: false, onConfirm: null as (() => void) | null })
const showConfirmModal = (title: string, message: string, onConfirm: () => void, dangerous = false) => {
  Object.assign(confirmModal, { show: true, title, message, dangerous, onConfirm })
}
const closeConfirmModal = () => { confirmModal.show = false; confirmModal.onConfirm = null }
const executeConfirmModal = () => { confirmModal.onConfirm?.(); closeConfirmModal() }

const sessions = ref<Session[]>([])
const loadingSessions = ref(true)
const activeSessionId = ref<number | null>(null)
const activeSession = ref<SessionDetail | null>(null)
const activeMessages = ref<Message[]>([])
const loadingMessages = ref(false)
const draft = ref('')

const quickReplies = [
  'Hi! How can I help you today?',
  'Thanks for reaching out! Let me look into that for you.',
  'Could you provide more details so I can assist you better?',
  'I\'ll be right with you — just a moment!',
  'Is there anything else I can help you with?',
]
const filter = ref<'open' | 'closed'>('open')
const messagesEl = ref<HTMLElement | null>(null)

let connection: signalR.HubConnection | null = null
let previousSessionId: number | null = null

// ── Computed ──────────────────────────────────────────────────────────────
const filteredSessions = computed(() =>
  sessions.value.filter(s =>
    filter.value === 'open'
      ? s.status === 'Waiting' || s.status === 'Active'
      : s.status === 'Closed'
  )
)

// ── SignalR ───────────────────────────────────────────────────────────────
async function connectHub() {
  connection = new signalR.HubConnectionBuilder()
    .withUrl(HUB_URL, {
      accessTokenFactory: () => authStore.token ?? '',
      transport: signalR.HttpTransportType.WebSockets | signalR.HttpTransportType.LongPolling
    })
    .withAutomaticReconnect()
    .configureLogging(signalR.LogLevel.Warning)
    .build()

  connection.on('ReceiveMessage', (msg: Message & { sessionId: number }) => {
    // Update active message thread
    if (msg.sessionId === activeSessionId.value) {
      activeMessages.value.push(msg)
      scrollToBottom()
    }
    // Update sidebar unread count
    const s = sessions.value.find(x => x.id === msg.sessionId)
    if (s && msg.senderRole === 'Customer') {
      s.unreadCount++
      s.lastMessage = msg.body
    }
  })

  connection.on('SessionUpdated', (data: { sessionId: number; lastMessage: string }) => {
    const s = sessions.value.find(x => x.id === data.sessionId)
    if (s) s.lastMessage = data.lastMessage
  })

  connection.on('SessionActivated', (sessionId: number) => {
    const s = sessions.value.find(x => x.id === sessionId)
    if (s) s.status = 'Active'
    if (activeSession.value?.id === sessionId)
      activeSession.value.status = 'Active'
  })

  connection.on('SessionClosed', (sessionId: number) => {
    const s = sessions.value.find(x => x.id === sessionId)
    if (s) s.status = 'Closed'
    if (activeSession.value?.id === sessionId)
      activeSession.value.status = 'Closed'
  })

  // Broadcast from customers starting new sessions
  connection.on('NewSession', (session: Session) => {
    sessions.value.unshift(session)
  })

  await connection.start()
  await connection.invoke('JoinAgentsRoom')
}

// ── Data loading ──────────────────────────────────────────────────────────
async function loadSessions() {
  loadingSessions.value = true
  try {
    sessions.value = await apiGet<Session[]>('/chat/sessions')
  } catch (err) {
    logError('Failed to load sessions', err)
    toast.error('Failed to load chat sessions')
  } finally {
    loadingSessions.value = false
  }
}

async function openSession(id: number) {
  activeSessionId.value = id
  loadingMessages.value = true
  activeMessages.value = []
  activeSession.value = null
  draft.value = ''

  try {
    const detail = await apiGet<SessionDetail>(`/chat/sessions/${id}`)
    activeSession.value = detail
    activeMessages.value = detail.messages

    if (connection) {
      if (previousSessionId !== null && previousSessionId !== id) {
        connection.invoke('LeaveSessionAsAgent', previousSessionId).catch(() => {})
      }
      previousSessionId = id
      await connection.invoke('JoinSessionAsAgent', id)
    }

    const s = sessions.value.find(x => x.id === id)
    if (s && s.unreadCount > 0) {
      s.unreadCount = 0
      apiPatch(`/chat/sessions/${id}/mark-read`).catch(() => {})
    }

    await scrollToBottom()
  } catch (err) {
    logError('Failed to load session', err)
    toast.error('Failed to load session messages')
  } finally {
    loadingMessages.value = false
  }
}

async function sendReply() {
  if (!draft.value.trim() || !connection || !activeSessionId.value) return
  try {
    await connection.invoke('SendMessage', activeSessionId.value, draft.value.trim())
    draft.value = ''
  } catch (err) {
    logError('Failed to send message', err)
    toast.error('Failed to send message')
  }
}

async function closeSession() {
  if (!connection || !activeSessionId.value) return
  try {
    await connection.invoke('CloseSession', activeSessionId.value)
  } catch (err) {
    logError('Failed to close session', err)
    toast.error('Failed to close session')
  }
}

function deleteSession(id: number) {
  showConfirmModal('Delete Session', 'Permanently delete this chat session and all its messages? This cannot be undone.', async () => {
    try {
      await apiDelete(`/chat/sessions/${id}/permanent`)
      sessions.value = sessions.value.filter(s => s.id !== id)
      if (activeSessionId.value === id) {
        activeSessionId.value = null
        activeSession.value = null
        activeMessages.value = []
      }
      toast.success('Session deleted')
    } catch (err) {
      logError('Failed to delete session', err)
      toast.error('Failed to delete session')
    }
  }, true)
}

async function scrollToBottom() {
  await nextTick()
  if (messagesEl.value) {
    messagesEl.value.scrollTop = messagesEl.value.scrollHeight
  }
}

// ── Formatting ────────────────────────────────────────────────────────────
function formatTime(iso: string) {
  return new Date(iso).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })
}
function formatTimeFull(iso: string) {
  return new Date(iso).toLocaleString([], {
    month: 'short', day: 'numeric',
    hour: '2-digit', minute: '2-digit'
  })
}

// ── Lifecycle ─────────────────────────────────────────────────────────────
onMounted(async () => {
  await Promise.all([loadSessions(), connectHub()])
})

onUnmounted(async () => {
  if (connection) await connection.stop()
})

watch(activeMessages, scrollToBottom, { deep: true })
</script>

<style scoped>
.livechat-page {
  padding: 2rem;
  max-width: 1200px;
  margin: 0 auto;
  height: 100%;
}

.page-title {
  font-size: 1.75rem;
  font-weight: 700;
  margin-bottom: 1.5rem;
  color: #1e293b;
}

.livechat-layout {
  display: grid;
  grid-template-columns: 280px 1fr;
  gap: 1rem;
  height: calc(100vh - 12rem);
  min-height: 500px;
}

/* Sidebar */
.session-sidebar {
  background: #fff;
  border-radius: 12px;
  border: 1px solid #e2e8f0;
  display: flex;
  flex-direction: column;
  overflow: hidden;
}

.sidebar-toolbar {
  display: flex;
  border-bottom: 1px solid #e2e8f0;
}
.filter-btn {
  flex: 1;
  padding: 0.625rem;
  font-size: 0.85rem;
  font-weight: 600;
  border: none;
  background: none;
  cursor: pointer;
  color: #64748b;
  transition: background 0.15s;
}
.filter-btn.active {
  color: #4f46e5;
  border-bottom: 2px solid #4f46e5;
  background: #f5f3ff;
}
.filter-btn:hover:not(.active) { background: #f8fafc; }

.sidebar-state {
  padding: 1.5rem;
  font-size: 0.875rem;
  color: #64748b;
  text-align: center;
}
.sidebar-state.muted { color: #94a3b8; }

.session-list {
  flex: 1;
  overflow-y: auto;
  list-style: none;
  padding: 0;
  margin: 0;
}

.session-item {
  padding: 0.75rem 1rem;
  border-bottom: 1px solid #f1f5f9;
  cursor: pointer;
  transition: background 0.1s;
}
.session-item:hover { background: #f8fafc; }
.session-item.active { background: #f0f4ff; border-left: 3px solid #4f46e5; }

.session-name {
  font-weight: 600;
  font-size: 0.875rem;
  color: #1e293b;
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.unread-badge {
  background: #ef4444;
  color: #fff;
  font-size: 0.65rem;
  font-weight: 700;
  border-radius: 50%;
  min-width: 1.1rem;
  height: 1.1rem;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  padding: 0 0.2rem;
}

.session-meta {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-top: 0.2rem;
}

.session-status {
  font-size: 0.7rem;
  font-weight: 600;
  padding: 0.1rem 0.4rem;
  border-radius: 4px;
}
.status-waiting { background: #fef3c7; color: #92400e; }
.status-active  { background: #d1fae5; color: #065f46; }
.status-closed  { background: #f1f5f9; color: #475569; }

.session-time {
  font-size: 0.7rem;
  color: #94a3b8;
}

.session-preview {
  font-size: 0.75rem;
  color: #64748b;
  margin-top: 0.2rem;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.delete-session-btn {
  margin-top: 0.4rem;
  padding: 0.2rem 0.6rem;
  font-size: 0.7rem;
  font-weight: 600;
  color: #dc2626;
  background: #fff1f2;
  border: 1px solid #fecaca;
  border-radius: 4px;
  cursor: pointer;
  transition: background 0.15s;
}
.delete-session-btn:hover { background: #fee2e2; }

/* Message panel */
.message-panel {
  background: #fff;
  border-radius: 12px;
  border: 1px solid #e2e8f0;
  display: flex;
  flex-direction: column;
  overflow: hidden;
}

.panel-empty {
  flex: 1;
  display: flex;
  align-items: center;
  justify-content: center;
  color: #94a3b8;
  font-size: 0.9rem;
}

.panel-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 0.875rem 1.25rem;
  border-bottom: 1px solid #e2e8f0;
  background: #fafafa;
  gap: 1rem;
}
.panel-title { font-weight: 700; font-size: 0.95rem; color: #1e293b; }
.panel-email { font-size: 0.8rem; color: #64748b; }
.panel-status {
  font-size: 0.75rem;
  font-weight: 600;
  padding: 0.15rem 0.5rem;
  border-radius: 4px;
  margin-left: 0.5rem;
}

.close-btn {
  padding: 0.4rem 0.875rem;
  background: #ef4444;
  color: #fff;
  border: none;
  border-radius: 6px;
  font-size: 0.8rem;
  font-weight: 600;
  cursor: pointer;
  transition: opacity 0.15s;
  white-space: nowrap;
}
.close-btn:hover { opacity: 0.85; }

.messages-area {
  flex: 1;
  overflow-y: auto;
  padding: 1rem;
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
}

.panel-state {
  color: #94a3b8;
  font-size: 0.875rem;
  padding: 1rem 0;
  text-align: center;
}

.system-msg {
  text-align: center;
  font-size: 0.75rem;
  color: #94a3b8;
  padding: 0.25rem 0;
}

.bubble-wrap { display: flex; }
.bubble-wrap.is-agent { justify-content: flex-end; }

.bubble {
  max-width: 75%;
  padding: 0.5rem 0.875rem;
  border-radius: 12px;
  font-size: 0.875rem;
  line-height: 1.45;
}
.is-customer .bubble {
  background: #f1f5f9;
  color: #1e293b;
  border-bottom-left-radius: 3px;
}
.is-agent .bubble {
  background: #4f46e5;
  color: #fff;
  border-bottom-right-radius: 3px;
}
.bubble-sender {
  display: block;
  font-size: 0.7rem;
  font-weight: 700;
  opacity: 0.65;
  margin-bottom: 0.15rem;
}
.bubble-time {
  display: block;
  font-size: 0.65rem;
  opacity: 0.55;
  margin-top: 0.2rem;
  text-align: right;
}

.reply-area {
  border-top: 1px solid #e2e8f0;
}

.quick-replies {
  display: flex;
  flex-wrap: wrap;
  gap: 0.4rem;
  padding: 0.5rem 1rem 0;
}

.qr-btn {
  font-size: 0.72rem;
  padding: 0.25rem 0.6rem;
  border: 1px solid #c7d2fe;
  border-radius: 999px;
  background: #eef2ff;
  color: #4338ca;
  cursor: pointer;
  white-space: nowrap;
  transition: background 0.15s;
}
.qr-btn:hover { background: #c7d2fe; }

.reply-row {
  display: flex;
  gap: 0.5rem;
  padding: 0.5rem 1rem 0.75rem;
}
.reply-input {
  flex: 1;
  padding: 0.55rem 0.875rem;
  border: 1px solid #e2e8f0;
  border-radius: 8px;
  font-size: 0.875rem;
  font-family: inherit;
}
.reply-input:focus {
  outline: none;
  border-color: #4f46e5;
  box-shadow: 0 0 0 2px rgba(79, 70, 229, 0.15);
}

.reply-btn {
  padding: 0.55rem 1.25rem;
  background: #4f46e5;
  color: #fff;
  border: none;
  border-radius: 8px;
  font-weight: 600;
  font-size: 0.875rem;
  cursor: pointer;
  transition: opacity 0.15s;
}
.reply-btn:hover:not(:disabled) { opacity: 0.88; }
.reply-btn:disabled { opacity: 0.45; cursor: default; }

/* Confirm Modal */
.confirm-modal-backdrop {
  position: fixed;
  inset: 0;
  background: rgba(0, 0, 0, 0.5);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 1000;
}
.confirm-modal-box {
  background: #fff;
  border-radius: 8px;
  width: 90%;
  max-width: 480px;
  box-shadow: 0 8px 32px rgba(0,0,0,0.2);
}
.confirm-modal-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 1.25rem 1.5rem;
  border-bottom: 1px solid #e5e7eb;
}
.confirm-modal-header h2 { margin: 0; font-size: 1.125rem; color: #111; }
.confirm-close-btn {
  background: none;
  border: none;
  font-size: 1.5rem;
  cursor: pointer;
  color: #6b7280;
  line-height: 1;
}
.confirm-modal-body { padding: 1.25rem 1.5rem; }
.confirm-modal-body p { margin: 0 0 1.25rem; color: #374151; }
.confirm-modal-footer { display: flex; justify-content: flex-end; gap: 0.75rem; }
.confirm-btn {
  padding: 0.5rem 1.25rem;
  border: none;
  border-radius: 6px;
  font-weight: 600;
  cursor: pointer;
  font-size: 0.875rem;
}
.confirm-btn-secondary { background: #e5e7eb; color: #374151; }
.confirm-btn-secondary:hover { background: #d1d5db; }
.confirm-btn-primary { background: #4f46e5; color: #fff; }
.confirm-btn-primary:hover { background: #4338ca; }
.confirm-btn-danger { background: #ef4444; color: #fff; }
.confirm-btn-danger:hover { background: #dc2626; }

/* Back button — mobile only */
.back-btn {
  display: none;
}

@media (max-width: 768px) {
  .livechat-layout {
    grid-template-columns: 1fr;
    height: calc(100vh - 8rem);
  }

  /* No session selected: show sidebar, hide message panel */
  .livechat-layout:not(.has-session) .message-panel {
    display: none;
  }

  /* Session selected: hide sidebar, show message panel */
  .livechat-layout.has-session .session-sidebar {
    display: none;
  }

  .back-btn {
    display: inline-flex;
    align-items: center;
    background: none;
    border: none;
    color: #4f46e5;
    font-weight: 600;
    font-size: 0.9rem;
    cursor: pointer;
    padding: 0;
    margin-bottom: 0.5rem;
  }
}
</style>
