<template>
  <!-- Floating chat button -->
  <Teleport to="body">
    <div class="chat-widget" v-if="!hideOnPage">

      <!-- Collapsed button -->
      <button
        v-if="!isOpen"
        class="chat-toggle"
        @click="openChat"
        aria-label="Open live chat"
      >
        💬
        <span v-if="hasUnread" class="chat-badge">!</span>
      </button>

      <!-- Expanded panel -->
      <div v-else class="chat-panel">
        <!-- Header -->
        <div class="chat-header">
          <div class="chat-header-info">
            <span class="chat-status-dot" :class="statusClass"></span>
            <span class="chat-title">Live Chat</span>
            <span class="chat-subtitle">{{ statusLabel }}</span>
          </div>
          <button class="chat-close" @click="closeChat" aria-label="Minimise chat">✕</button>
        </div>

        <!-- Start form (idle state) -->
        <div v-if="status === 'idle'" class="chat-start-form">
          <p class="chat-intro">Hi! Start a chat and we'll get back to you shortly.</p>
          <input
            v-model="guestName"
            class="chat-input-field"
            placeholder="Your name"
            maxlength="80"
            @keydown.enter="attemptStart"
          />
          <input
            v-model="guestEmail"
            class="chat-input-field"
            placeholder="Email (optional)"
            type="email"
            maxlength="200"
            @keydown.enter="attemptStart"
          />
          <p v-if="error" class="chat-error">{{ error }}</p>
          <button
            class="chat-send-btn"
            :disabled="isConnecting || !guestName.trim()"
            @click="attemptStart"
          >
            {{ isConnecting ? 'Connecting…' : 'Start Chat' }}
          </button>
        </div>

        <!-- Message thread (waiting / active / closed) -->
        <template v-else>
          <div class="chat-messages" ref="messageListEl">
            <div v-if="status === 'waiting'" class="chat-system-msg">
              Waiting for an agent to join…
            </div>

            <div
              v-for="msg in messages"
              :key="msg.id"
              class="chat-bubble-wrap"
              :class="msg.senderRole === 'Customer' ? 'is-customer' : 'is-agent'"
            >
              <div class="chat-bubble">
                <span class="chat-sender">{{ msg.senderRole === 'Customer' ? 'You' : msg.senderName }}</span>
                <p>{{ msg.body }}</p>
                <span class="chat-time">{{ formatTime(msg.sentAt) }}</span>
              </div>
            </div>

            <div v-if="status === 'closed'" class="chat-system-msg">
              This chat has been closed. Thank you!
            </div>
          </div>

          <!-- Input row -->
          <div v-if="status !== 'closed'" class="chat-input-row">
            <input
              v-model="draft"
              class="chat-input-field"
              placeholder="Type a message…"
              maxlength="2000"
              @keydown.enter.prevent="send"
            />
            <button class="chat-send-btn icon-only" :disabled="!draft.trim()" @click="send">➤</button>
          </div>

          <div v-if="status === 'closed'" class="chat-restart">
            <button class="chat-send-btn" @click="restart">Start New Chat</button>
          </div>
        </template>
      </div>

    </div>
  </Teleport>
</template>

<script setup lang="ts">
import { ref, computed, watch, nextTick, onMounted } from 'vue'
import { storeToRefs } from 'pinia'
import { useRoute } from 'vue-router'
import { useChatStore } from '@/stores/chat'
import { useAuthStore } from '@/stores/auth'

const route = useRoute()
const chatStore = useChatStore()
const authStore = useAuthStore()

const { isOpen, status, messages, isConnecting, error } = storeToRefs(chatStore)
const { openChat, closeChat, restoreSession, startSession, sendMessage, endSession } = chatStore

onMounted(() => { restoreSession() })

// Hide on maintenance / login / signup pages, and for admin users (they get the AdminChatBell instead)
const hideOnPage = computed(() => {
  const hidden = ['/maintenance', '/login', '/signup']
  return hidden.includes(route.path) || authStore.hasAdminAccess
})

const hasUnread = ref(false)

// Local form fields
const guestName = ref(authStore.user ? `${authStore.user.firstName} ${authStore.user.lastName}` : '')
const guestEmail = ref(authStore.user?.email ?? '')
const draft = ref('')

const messageListEl = ref<HTMLElement | null>(null)

// Scroll to bottom when new messages arrive
watch(messages, async () => {
  await nextTick()
  if (messageListEl.value) {
    messageListEl.value.scrollTop = messageListEl.value.scrollHeight
  }
  if (!isOpen.value) hasUnread.value = true
}, { deep: true })

const statusClass = computed(() => ({
  'dot-waiting': status.value === 'waiting',
  'dot-active': status.value === 'active',
  'dot-closed': status.value === 'closed',
  'dot-idle': status.value === 'idle'
}))

const statusLabel = computed(() => {
  if (status.value === 'waiting') return 'Waiting for agent…'
  if (status.value === 'active') return 'Connected'
  if (status.value === 'closed') return 'Session ended'
  return 'Start a conversation'
})

function formatTime(iso: string) {
  return new Date(iso).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })
}

async function attemptStart() {
  if (!guestName.value.trim()) return
  await startSession(guestName.value.trim(), guestEmail.value.trim() || undefined)
}

async function send() {
  if (!draft.value.trim()) return
  await sendMessage(draft.value.trim())
  draft.value = ''
}

function restart() {
  endSession()
  guestName.value = authStore.user ? `${authStore.user.firstName} ${authStore.user.lastName}` : ''
  guestEmail.value = authStore.user?.email ?? ''
}
</script>

<style scoped>
.chat-widget {
  position: fixed;
  bottom: 1.25rem;
  left: 1.5rem;
  z-index: 10001;
  display: flex;
  flex-direction: column;
  align-items: flex-start;
  gap: 0.75rem;
}

/* Floating toggle button */
.chat-toggle {
  width: 3.25rem;
  height: 3.25rem;
  border-radius: 50%;
  background: var(--color-primary, #6366f1);
  color: #fff;
  font-size: 1.4rem;
  border: none;
  cursor: pointer;
  box-shadow: 0 4px 14px rgba(0, 0, 0, 0.25);
  transition: transform 0.2s ease, box-shadow 0.2s ease;
  position: relative;
}
.chat-toggle:hover {
  transform: scale(1.08);
  box-shadow: 0 6px 18px rgba(0, 0, 0, 0.3);
}

.chat-badge {
  position: absolute;
  top: -2px;
  right: -2px;
  width: 1rem;
  height: 1rem;
  background: #ef4444;
  border-radius: 50%;
  font-size: 0.6rem;
  display: flex;
  align-items: center;
  justify-content: center;
}

/* Panel */
.chat-panel {
  width: 22rem;
  max-height: 32rem;
  background: var(--color-bg-secondary, #fff);
  border-radius: var(--card-radius, 12px);
  box-shadow: 0 8px 30px rgba(0, 0, 0, 0.2);
  display: flex;
  flex-direction: column;
  overflow: hidden;
  border: 1px solid var(--color-border, #e5e7eb);
}

/* Header */
.chat-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 0.75rem 1rem;
  background: var(--color-primary, #6366f1);
  color: #fff;
}
.chat-header-info {
  display: flex;
  flex-direction: column;
  gap: 0.1rem;
}
.chat-title { font-weight: 700; font-size: 0.95rem; }
.chat-subtitle { font-size: 0.72rem; opacity: 0.85; }
.chat-close {
  background: none;
  border: none;
  color: #fff;
  font-size: 1rem;
  cursor: pointer;
  opacity: 0.8;
  padding: 0.25rem;
}
.chat-close:hover { opacity: 1; }

.chat-status-dot {
  width: 0.5rem;
  height: 0.5rem;
  border-radius: 50%;
  display: inline-block;
  margin-bottom: 0.1rem;
}
.dot-waiting { background: #f59e0b; }
.dot-active  { background: #22c55e; }
.dot-closed  { background: #6b7280; }
.dot-idle    { background: rgba(255,255,255,0.5); }

/* Start form */
.chat-start-form {
  padding: 1rem;
  display: flex;
  flex-direction: column;
  gap: 0.625rem;
}
.chat-intro {
  font-size: 0.875rem;
  color: var(--color-text-secondary, #555);
}
.chat-error {
  font-size: 0.8rem;
  color: #ef4444;
}

/* Messages */
.chat-messages {
  flex: 1;
  overflow-y: auto;
  padding: 0.75rem;
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
}

.chat-system-msg {
  font-size: 0.75rem;
  text-align: center;
  color: var(--color-text-muted, #9ca3af);
  padding: 0.25rem 0;
}

.chat-bubble-wrap {
  display: flex;
}
.chat-bubble-wrap.is-customer {
  justify-content: flex-end;
}

.chat-bubble {
  max-width: 80%;
  padding: 0.5rem 0.75rem;
  border-radius: 12px;
  font-size: 0.875rem;
  line-height: 1.4;
  overflow-wrap: break-word;
  word-break: break-word;
}
.is-customer .chat-bubble {
  background: var(--color-primary, #6366f1);
  color: #fff;
  border-bottom-right-radius: 3px;
}
.is-agent .chat-bubble {
  background: var(--color-bg-muted, #f3f4f6);
  color: var(--color-text-primary, #111);
  border-bottom-left-radius: 3px;
}
.chat-sender {
  display: block;
  font-size: 0.7rem;
  font-weight: 600;
  opacity: 0.7;
  margin-bottom: 0.15rem;
}
.chat-time {
  display: block;
  font-size: 0.65rem;
  opacity: 0.6;
  margin-top: 0.2rem;
  text-align: right;
}

/* Input row */
.chat-input-row {
  display: flex;
  gap: 0.5rem;
  padding: 0.625rem 0.75rem;
  border-top: 1px solid var(--color-border, #e5e7eb);
}

.chat-input-field {
  flex: 1;
  padding: 0.5rem 0.75rem;
  border: 1px solid var(--color-border, #ddd);
  border-radius: var(--input-radius, 8px);
  font-size: 0.875rem;
  font-family: var(--font-body, inherit);
  background: var(--color-bg, #fff);
  color: var(--color-text-primary, #333);
  width: 100%;
}
.chat-input-field:focus {
  outline: none;
  border-color: var(--color-primary, #6366f1);
  box-shadow: 0 0 0 2px rgba(99, 102, 241, 0.15);
}

.chat-send-btn {
  padding: 0.5rem 1rem;
  background: var(--color-primary, #6366f1);
  color: #fff;
  border: none;
  border-radius: var(--button-radius, 8px);
  font-size: 0.875rem;
  font-weight: 600;
  cursor: pointer;
  transition: opacity 0.15s;
  white-space: nowrap;
}
.chat-send-btn:hover:not(:disabled) { opacity: 0.88; }
.chat-send-btn:disabled { opacity: 0.45; cursor: default; }
.chat-send-btn.icon-only { padding: 0.5rem 0.75rem; }

.chat-restart {
  padding: 0.625rem 0.75rem;
  border-top: 1px solid var(--color-border, #e5e7eb);
}
.chat-restart .chat-send-btn { width: 100%; }

@media (max-width: 480px) {
  .chat-panel { width: calc(100vw - 2rem); }
}
</style>
