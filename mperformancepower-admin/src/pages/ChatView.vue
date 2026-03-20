<script setup lang="ts">
import { ref, computed, watch, nextTick } from 'vue'
import AdminShell from '@/components/layout/AdminShell.vue'
import Button from 'primevue/button'
import Textarea from 'primevue/textarea'
import { useChatStore } from '@/stores/chat.store'

const chatStore = useChatStore()

const QUICK_REPLIES = [
  'Hi! Thanks for reaching out. How can I help you today?',
  'Thanks for your patience! Let me look into that for you.',
  'Could you please provide more details so I can assist you better?',
  'We have a great selection available. Would you like to schedule a test drive?',
  "I can get you a quote on that vehicle — what's your budget?",
  'Our financing options are very competitive. Would you like more info?',
  'Feel free to visit us anytime during business hours!',
  "I'll follow up with you shortly via email.",
]

const tab = ref<'active' | 'closed'>('active')
const replyText = ref('')
const showPresets = ref(false)
const deletingId = ref<string | null>(null)
const messagesEl = ref<HTMLElement | null>(null)

// Scroll to bottom whenever messages change
watch(() => chatStore.messages.length, () => nextTick(() => {
  if (messagesEl.value) messagesEl.value.scrollTop = messagesEl.value.scrollHeight
}))

const liveSessions = computed(() => chatStore.sessions.filter(s => s.status === 'Active'))
const closedSessions = computed(() => chatStore.sessions.filter(s => s.status === 'Closed'))
const visibleSessions = computed(() => tab.value === 'active' ? liveSessions.value : closedSessions.value)

async function selectSession(session: typeof chatStore.sessions[0]) {
  showPresets.value = false
  replyText.value = ''
  await chatStore.selectSession(session)
}

async function sendReply() {
  const content = replyText.value.trim()
  if (!content) return
  replyText.value = ''
  showPresets.value = false
  await chatStore.sendReply(content)
}

async function handleDelete(sessionId: string) {
  deletingId.value = sessionId
  try {
    await chatStore.deleteSession(sessionId)
  } finally {
    deletingId.value = null
  }
}

function usePreset(text: string) {
  replyText.value = text
  showPresets.value = false
}

function onKeydown(e: KeyboardEvent) {
  if (e.key === 'Enter' && !e.shiftKey) {
    e.preventDefault()
    sendReply()
  }
}

function formatTime(iso: string) {
  const d = new Date(iso)
  const now = new Date()
  return d.toDateString() === now.toDateString()
    ? d.toLocaleTimeString('en-US', { hour: 'numeric', minute: '2-digit' })
    : d.toLocaleDateString('en-US', { month: 'short', day: 'numeric', hour: 'numeric', minute: '2-digit' })
}

function timeAgo(iso: string) {
  const diff = Date.now() - new Date(iso).getTime()
  const m = Math.floor(diff / 60000)
  if (m < 1) return 'just now'
  if (m < 60) return `${m}m ago`
  const h = Math.floor(m / 60)
  if (h < 24) return `${h}h ago`
  return `${Math.floor(h / 24)}d ago`
}
</script>

<template>
  <AdminShell>
    <div class="chat-admin">

      <!-- ── Left panel ─────────────────────────────────────── -->
      <div class="sessions-panel">
        <div class="sessions-panel__header">
          <h2>Live Chat</h2>
          <span v-if="chatStore.totalUnread > 0" class="unread-total">{{ chatStore.totalUnread }}</span>
        </div>

        <!-- Tabs -->
        <div class="tabs">
          <button class="tab" :class="{ 'tab--active': tab === 'active' }" @click="tab = 'active'">
            Active
            <span v-if="liveSessions.length" class="tab-count">{{ liveSessions.length }}</span>
          </button>
          <button class="tab" :class="{ 'tab--active': tab === 'closed' }" @click="tab = 'closed'">
            Closed
            <span v-if="closedSessions.length" class="tab-count tab-count--closed">{{ closedSessions.length }}</span>
          </button>
        </div>

        <div class="sessions-panel__list">
          <div v-if="visibleSessions.length === 0" class="sessions-empty">
            {{ tab === 'active' ? 'No active chats' : 'No closed chats' }}
          </div>

          <div
            v-for="s in visibleSessions"
            :key="s.id"
            class="session-row"
            :class="{ 'session-row--active': chatStore.activeSession?.id === s.id }"
            @click="selectSession(s)"
          >
            <div class="session-row__avatar">{{ s.visitorName[0]?.toUpperCase() }}</div>
            <div class="session-row__info">
              <div class="session-row__name">{{ s.visitorName }}</div>
              <div class="session-row__meta">{{ timeAgo(s.lastMessageAt) }}</div>
            </div>
            <div class="session-row__right">
              <!-- Active tab indicators -->
              <span v-if="tab === 'active' && chatStore.unreadMap[s.id] > 0" class="unread-badge">
                {{ chatStore.unreadMap[s.id] }}
              </span>
              <span v-else-if="tab === 'active' && chatStore.activeSession?.id !== s.id" class="new-dot" />
              <!-- Closed tab delete button -->
              <Button
                v-if="tab === 'closed'"
                icon="pi pi-trash"
                text
                severity="danger"
                size="small"
                :disabled="deletingId === s.id"
                @click.stop="handleDelete(s.id)"
                v-tooltip.top="'Delete conversation'"
              />
            </div>
          </div>
        </div>
      </div>

      <!-- ── Right panel ────────────────────────────────────── -->
      <div class="message-panel">
        <div v-if="!chatStore.activeSession" class="message-panel__empty">
          <svg width="48" height="48" viewBox="0 0 24 24" fill="#333">
            <path d="M20 2H4a2 2 0 0 0-2 2v18l4-4h14a2 2 0 0 0 2-2V4a2 2 0 0 0-2-2z"/>
          </svg>
          <p>Select a conversation</p>
        </div>

        <template v-else>
          <!-- Header -->
          <div class="chat-header">
            <div class="chat-header__info">
              <div class="chat-avatar">{{ chatStore.activeSession.visitorName[0]?.toUpperCase() }}</div>
              <div>
                <div class="chat-header__name">{{ chatStore.activeSession.visitorName }}</div>
                <div class="chat-header__email">{{ chatStore.activeSession.visitorEmail || 'No email' }}</div>
              </div>
            </div>
            <div class="chat-header__actions">
              <span class="status-badge" :style="{ '--c': chatStore.activeSession.status === 'Active' ? '#22c55e' : '#ef4444' }">
                {{ chatStore.activeSession.status }}
              </span>
              <Button
                v-if="chatStore.activeSession.status === 'Active'"
                label="Close Chat"
                severity="secondary"
                outlined
                size="small"
                @click="chatStore.closeSession()"
              />
              <Button
                v-else
                label="Delete"
                severity="danger"
                outlined
                size="small"
                @click="handleDelete(chatStore.activeSession.id)"
              />
            </div>
          </div>

          <!-- Messages -->
          <div class="messages" ref="messagesEl">
            <div v-if="chatStore.messages.length === 0" class="messages__empty">No messages yet</div>
            <div
              v-for="(msg, i) in chatStore.messages"
              :key="i"
              class="msg"
              :class="msg.senderType === 'Admin' ? 'msg--admin' : 'msg--visitor'"
            >
              <div class="msg__bubble">{{ msg.content }}</div>
              <div class="msg__meta">{{ msg.senderName }} · {{ formatTime(msg.sentAt) }}</div>
            </div>
          </div>

          <!-- Reply area (active only) -->
          <div v-if="chatStore.activeSession.status === 'Active'" class="reply-area">
            <!-- Quick replies panel -->
            <div v-if="showPresets" class="presets">
              <div class="presets__header">
                <span>Quick Replies</span>
                <Button icon="pi pi-times" text size="small" class="presets__close-btn" @click="showPresets = false" />
              </div>
              <Button
                v-for="(text, i) in QUICK_REPLIES"
                :key="i"
                :label="text"
                class="preset-chip"
                severity="secondary"
                outlined
                @click="usePreset(text)"
              />
            </div>

            <div class="reply-bar">
              <Button
                :icon="showPresets ? 'pi pi-times' : 'pi pi-align-justify'"
                :severity="showPresets ? 'danger' : 'secondary'"
                text
                class="preset-toggle"
                v-tooltip.top="'Quick replies'"
                @click="showPresets = !showPresets"
              />
              <Textarea
                v-model="replyText"
                placeholder="Reply to visitor... (Enter to send)"
                rows="2"
                @keydown="onKeydown"
                style="flex:1; resize:none; max-height:120px; overflow-y:auto;"
              />
              <Button
                label="Send"
                severity="warn"
                :disabled="!replyText.trim()"
                @click="sendReply"
              />
            </div>
          </div>
          <div v-else class="reply-closed">This chat is closed.</div>
        </template>
      </div>

    </div>
  </AdminShell>
</template>

<style scoped>
.chat-admin {
  display: grid;
  grid-template-columns: 290px 1fr;
  height: calc(100vh - 120px);
  background: #111;
  border: 1px solid #222;
  border-radius: 10px;
  overflow: hidden;
}

/* ── Sessions panel ─────────────────────────────────── */
.sessions-panel {
  border-right: 1px solid #1a1a1a;
  display: flex;
  flex-direction: column;
  overflow: hidden;
}
.sessions-panel__header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 16px 16px 10px;
  border-bottom: 1px solid #1a1a1a;
}
.sessions-panel__header h2 { font-size: 1rem; font-weight: 700; }
.unread-total {
  background: #e63946;
  color: #fff;
  font-size: 0.7rem;
  font-weight: 800;
  min-width: 20px;
  height: 20px;
  border-radius: 999px;
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 0 6px;
}

/* Tabs */
.tabs {
  display: flex;
  border-bottom: 1px solid #1a1a1a;
}
.tab {
  flex: 1;
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 6px;
  padding: 10px 0;
  background: none;
  border: none;
  color: #555;
  font-size: 0.82rem;
  font-weight: 600;
  cursor: pointer;
  border-bottom: 2px solid transparent;
  transition: color 0.15s, border-color 0.15s;
}
.tab:hover { color: #9a9a9a; }
.tab--active { color: #f0f0f0; border-bottom-color: #e63946; }

.tab-count {
  background: #e63946;
  color: #fff;
  font-size: 0.6rem;
  font-weight: 800;
  min-width: 16px;
  height: 16px;
  border-radius: 999px;
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 0 4px;
}
.tab-count--closed { background: #333; color: #777; }

/* Session list */
.sessions-panel__list { overflow-y: auto; flex: 1; }
.sessions-empty { padding: 32px 16px; text-align: center; font-size: 0.8rem; color: #555; }

.session-row {
  display: flex;
  align-items: center;
  gap: 10px;
  padding: 11px 14px;
  cursor: pointer;
  border-bottom: 1px solid #161616;
  transition: background 0.1s;
}
.session-row:hover { background: #151515; }
.session-row--active { background: #1a1a1a; }

.session-row__avatar {
  width: 34px;
  height: 34px;
  border-radius: 50%;
  background: #222;
  color: #f0f0f0;
  display: flex;
  align-items: center;
  justify-content: center;
  font-weight: 700;
  font-size: 0.82rem;
  flex-shrink: 0;
}
.session-row__info { flex: 1; min-width: 0; }
.session-row__name { font-size: 0.875rem; color: #f0f0f0; font-weight: 500; white-space: nowrap; overflow: hidden; text-overflow: ellipsis; }
.session-row__meta { font-size: 0.7rem; color: #555; }
.session-row__right { flex-shrink: 0; display: flex; align-items: center; }

.unread-badge {
  background: #e63946;
  color: #fff;
  font-size: 0.62rem;
  font-weight: 800;
  min-width: 18px;
  height: 18px;
  border-radius: 999px;
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 0 5px;
}
.new-dot {
  width: 7px;
  height: 7px;
  border-radius: 50%;
  background: #22c55e;
  box-shadow: 0 0 5px #22c55e;
}

/* ── Message panel ──────────────────────────────────── */
.message-panel { display: flex; flex-direction: column; overflow: hidden; }
.message-panel__empty {
  flex: 1;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: 12px;
  color: #444;
  font-size: 0.875rem;
}

.chat-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 13px 18px;
  border-bottom: 1px solid #1a1a1a;
  background: #0d0d0d;
  flex-shrink: 0;
}
.chat-header__info { display: flex; align-items: center; gap: 10px; }
.chat-avatar {
  width: 36px; height: 36px; border-radius: 50%; background: #222; color: #f0f0f0;
  display: flex; align-items: center; justify-content: center; font-weight: 700; font-size: 0.875rem;
}
.chat-header__name { font-size: 0.9rem; font-weight: 700; color: #f0f0f0; }
.chat-header__email { font-size: 0.75rem; color: #555; }
.chat-header__actions { display: flex; align-items: center; gap: 10px; }

.status-badge {
  font-size: 0.7rem; font-weight: 700; padding: 3px 9px; border-radius: 999px;
  background: color-mix(in srgb, var(--c) 15%, transparent); color: var(--c);
}

/* Messages */
.messages {
  flex: 1; overflow-y: auto; padding: 16px 18px;
  display: flex; flex-direction: column; gap: 10px; scroll-behavior: smooth;
}
.messages__empty { margin: auto; color: #555; font-size: 0.875rem; }
.msg { display: flex; flex-direction: column; max-width: 65%; }
.msg--admin { align-self: flex-end; align-items: flex-end; }
.msg--visitor { align-self: flex-start; align-items: flex-start; }
.msg__bubble {
  padding: 9px 14px; border-radius: 14px; font-size: 0.875rem; line-height: 1.5; word-break: break-word;
}
.msg--admin .msg__bubble { background: #e63946; color: #fff; border-bottom-right-radius: 4px; }
.msg--visitor .msg__bubble { background: #1e1e1e; color: #f0f0f0; border-bottom-left-radius: 4px; }
.msg__meta { font-size: 0.65rem; color: #555; margin-top: 3px; padding: 0 4px; }

/* Reply area */
.reply-area { flex-shrink: 0; border-top: 1px solid #1a1a1a; background: #0d0d0d; }

.presets {
  padding: 10px 14px; border-bottom: 1px solid #1a1a1a;
  display: flex; flex-direction: column; gap: 5px; max-height: 190px; overflow-y: auto;
}
.presets__header {
  display: flex; align-items: center; justify-content: space-between;
  font-size: 0.68rem; color: #555; font-weight: 600; text-transform: uppercase;
  letter-spacing: 0.05em; margin-bottom: 2px;
}

:deep(.presets__close-btn.p-button) {
  padding: 2px 4px;
  color: #555;
}

:deep(.preset-chip.p-button) {
  justify-content: flex-start;
  text-align: left;
  white-space: normal;
  height: auto;
  line-height: 1.4;
  font-size: 0.8rem;
  padding: 7px 10px;
}

.reply-bar { display: flex; align-items: flex-end; gap: 8px; padding: 10px 12px; }

:deep(.preset-toggle.p-button) {
  width: 34px;
  height: 34px;
  flex-shrink: 0;
}

.reply-closed { padding: 14px 18px; font-size: 0.8rem; color: #555; text-align: center; }
</style>
