<script setup lang="ts">
import { ref, onMounted, onUnmounted, nextTick, watch } from 'vue'
import * as signalR from '@microsoft/signalr'

const API_URL = import.meta.env.VITE_API_URL as string
const HUB_URL = API_URL.replace('/api', '') + '/api/hubs/chat'

interface Message {
  id?: number
  senderType: 'Visitor' | 'Admin'
  senderName: string
  content: string
  sentAt: string
}

const open = ref(false)
const sessionId = ref<string | null>(localStorage.getItem('mpp_chat_session'))
const visitorName = ref(localStorage.getItem('mpp_chat_name') ?? '')
const nameInput = ref('')
const emailInput = ref('')
const messageInput = ref('')
const messages = ref<Message[]>([])
const unread = ref(0)
const closed = ref(false)
const connecting = ref(false)
const scrollEl = ref<HTMLElement | null>(null)

let connection: signalR.HubConnection | null = null

onMounted(() => {
  if (sessionId.value) {
    connect().then(() => loadHistory())
  }
})

onUnmounted(() => {
  connection?.stop()
})

watch(open, (val) => {
  if (val) {
    unread.value = 0
    nextTick(scrollToBottom)
  }
})

async function connect() {
  if (connection?.state === signalR.HubConnectionState.Connected) return

  connection = new signalR.HubConnectionBuilder()
    .withUrl(HUB_URL)
    .withAutomaticReconnect()
    .build()

  connection.on('ReceiveMessage', (msg: Message) => {
    messages.value.push(msg)
    if (!open.value && msg.senderType === 'Admin') unread.value++
    nextTick(scrollToBottom)
  })

  connection.on('SessionClosed', () => {
    closed.value = true
  })

  await connection.start()
}

async function loadHistory() {
  if (!sessionId.value) return
  try {
    // Visitors don't have auth — just rejoin the group via hub, no REST history load
  } catch {}

  if (connection?.state === signalR.HubConnectionState.Connected) {
    await connection.invoke('RejoinSession', sessionId.value)
  }
}

async function startSession() {
  if (!nameInput.value.trim()) return
  connecting.value = true
  try {
    await connect()
    const result = await connection!.invoke<{ sessionId: string }>('StartSession', nameInput.value.trim(), emailInput.value.trim())
    sessionId.value = result.sessionId
    visitorName.value = nameInput.value.trim()
    localStorage.setItem('mpp_chat_session', result.sessionId)
    localStorage.setItem('mpp_chat_name', visitorName.value)
  } finally {
    connecting.value = false
  }
}

async function send() {
  const content = messageInput.value.trim()
  if (!content || !sessionId.value || closed.value) return
  messageInput.value = ''

  // Optimistic local message
  messages.value.push({
    senderType: 'Visitor',
    senderName: visitorName.value,
    content,
    sentAt: new Date().toISOString(),
  })
  nextTick(scrollToBottom)

  await connection!.invoke('SendMessage', sessionId.value, content)
}

function onKeydown(e: KeyboardEvent) {
  if (e.key === 'Enter' && !e.shiftKey) {
    e.preventDefault()
    send()
  }
}

function resetChat() {
  sessionId.value = null
  visitorName.value = ''
  messages.value = []
  closed.value = false
  nameInput.value = ''
  emailInput.value = ''
  localStorage.removeItem('mpp_chat_session')
  localStorage.removeItem('mpp_chat_name')
  connection?.stop()
  connection = null
}

function scrollToBottom() {
  if (scrollEl.value) scrollEl.value.scrollTop = scrollEl.value.scrollHeight
}

function formatTime(iso: string) {
  return new Date(iso).toLocaleTimeString('en-US', { hour: 'numeric', minute: '2-digit' })
}
</script>

<template>
  <!-- Bubble button -->
  <button class="chat-bubble" @click="open = !open" aria-label="Live chat">
    <svg v-if="!open" width="24" height="24" viewBox="0 0 24 24" fill="currentColor">
      <path d="M20 2H4a2 2 0 0 0-2 2v18l4-4h14a2 2 0 0 0 2-2V4a2 2 0 0 0-2-2z"/>
    </svg>
    <svg v-else width="24" height="24" viewBox="0 0 24 24" fill="currentColor">
      <path d="M19 6.41L17.59 5 12 10.59 6.41 5 5 6.41 10.59 12 5 17.59 6.41 19 12 13.41 17.59 19 19 17.59 13.41 12z"/>
    </svg>
    <span v-if="unread > 0 && !open" class="chat-bubble__badge">{{ unread }}</span>
  </button>

  <!-- Chat window -->
  <Transition name="chat-slide">
    <div v-if="open" class="chat-window">
      <!-- Header -->
      <div class="chat-header">
        <div class="chat-header__info">
          <div class="chat-header__avatar">M</div>
          <div>
            <div class="chat-header__name">M Performance Power</div>
            <div class="chat-header__status">
              <span class="status-dot" />
              Live Support
            </div>
          </div>
        </div>
        <button v-if="sessionId" class="chat-header__end" @click="resetChat" title="End chat">✕</button>
      </div>

      <!-- Start form (no session yet) -->
      <div v-if="!sessionId" class="chat-start">
        <p class="chat-start__intro">Hi there! 👋 Start a chat and we'll get back to you right away.</p>
        <div class="chat-field">
          <label>Your Name</label>
          <input v-model="nameInput" type="text" placeholder="John Doe" @keydown.enter="startSession" />
        </div>
        <div class="chat-field">
          <label>Email (optional)</label>
          <input v-model="emailInput" type="email" placeholder="john@example.com" @keydown.enter="startSession" />
        </div>
        <button class="chat-start__btn" :disabled="connecting || !nameInput.trim()" @click="startSession">
          {{ connecting ? 'Connecting...' : 'Start Chat' }}
        </button>
      </div>

      <!-- Active chat -->
      <template v-else>
        <div class="chat-messages" ref="scrollEl">
          <div v-if="messages.length === 0" class="chat-messages__empty">
            Say hello! We'll reply as soon as possible.
          </div>

          <div
            v-for="(msg, i) in messages"
            :key="i"
            class="chat-msg"
            :class="msg.senderType === 'Visitor' ? 'chat-msg--visitor' : 'chat-msg--admin'"
          >
            <div class="chat-msg__bubble">{{ msg.content }}</div>
            <div class="chat-msg__time">{{ formatTime(msg.sentAt) }}</div>
          </div>

          <div v-if="closed" class="chat-closed">Chat ended. <button @click="resetChat">Start a new chat</button></div>
        </div>

        <div class="chat-input" v-if="!closed">
          <textarea
            v-model="messageInput"
            placeholder="Type a message..."
            rows="1"
            @keydown="onKeydown"
          />
          <button class="chat-send" @click="send" :disabled="!messageInput.trim()">
            <svg width="18" height="18" viewBox="0 0 24 24" fill="currentColor">
              <path d="M2.01 21L23 12 2.01 3 2 10l15 2-15 2z"/>
            </svg>
          </button>
        </div>
      </template>
    </div>
  </Transition>
</template>

<style scoped>
/* Bubble */
.chat-bubble {
  position: fixed;
  bottom: 24px;
  right: 24px;
  width: 56px;
  height: 56px;
  border-radius: 50%;
  background: #e63946;
  color: #fff;
  border: none;
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  box-shadow: 0 4px 20px rgba(230, 57, 70, 0.45);
  z-index: 9999;
  transition: transform 0.2s, box-shadow 0.2s;
}
.chat-bubble:hover {
  transform: scale(1.08);
  box-shadow: 0 6px 28px rgba(230, 57, 70, 0.55);
}
.chat-bubble__badge {
  position: absolute;
  top: 2px;
  right: 2px;
  background: #fff;
  color: #e63946;
  font-size: 0.65rem;
  font-weight: 800;
  width: 18px;
  height: 18px;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
}

/* Window */
.chat-window {
  position: fixed;
  bottom: 92px;
  right: 24px;
  width: 340px;
  max-height: 520px;
  background: #111;
  border: 1px solid #222;
  border-radius: 16px;
  box-shadow: 0 8px 40px rgba(0, 0, 0, 0.6);
  display: flex;
  flex-direction: column;
  overflow: hidden;
  z-index: 9998;
}

@media (max-width: 420px) {
  .chat-window {
    width: calc(100vw - 32px);
    right: 16px;
    bottom: 88px;
  }
}

/* Transition */
.chat-slide-enter-active,
.chat-slide-leave-active {
  transition: opacity 0.2s, transform 0.2s;
}
.chat-slide-enter-from,
.chat-slide-leave-to {
  opacity: 0;
  transform: translateY(12px) scale(0.97);
}

/* Header */
.chat-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 14px 16px;
  background: #e63946;
  color: #fff;
}
.chat-header__info { display: flex; align-items: center; gap: 10px; }
.chat-header__avatar {
  width: 36px;
  height: 36px;
  border-radius: 50%;
  background: rgba(255,255,255,0.2);
  display: flex;
  align-items: center;
  justify-content: center;
  font-weight: 800;
  font-size: 1rem;
}
.chat-header__name { font-weight: 700; font-size: 0.9rem; }
.chat-header__status { display: flex; align-items: center; gap: 5px; font-size: 0.72rem; opacity: 0.9; }
.status-dot {
  width: 7px; height: 7px;
  border-radius: 50%;
  background: #4ade80;
  box-shadow: 0 0 6px #4ade80;
}
.chat-header__end {
  background: transparent;
  border: none;
  color: rgba(255,255,255,0.7);
  cursor: pointer;
  font-size: 0.9rem;
  padding: 4px;
}
.chat-header__end:hover { color: #fff; }

/* Start form */
.chat-start {
  padding: 20px 16px;
  display: flex;
  flex-direction: column;
  gap: 12px;
}
.chat-start__intro { font-size: 0.85rem; color: #9a9a9a; line-height: 1.5; }
.chat-field { display: flex; flex-direction: column; gap: 4px; }
.chat-field label { font-size: 0.75rem; color: #777; font-weight: 500; }
.chat-field input {
  background: #0d0d0d;
  border: 1px solid #2a2a2a;
  border-radius: 8px;
  padding: 9px 12px;
  color: #f0f0f0;
  font-size: 0.875rem;
}
.chat-field input:focus { outline: none; border-color: #e63946; }
.chat-start__btn {
  padding: 10px;
  background: #e63946;
  color: #fff;
  border: none;
  border-radius: 8px;
  font-size: 0.9rem;
  font-weight: 700;
  cursor: pointer;
  transition: opacity 0.2s;
}
.chat-start__btn:disabled { opacity: 0.5; cursor: not-allowed; }
.chat-start__btn:not(:disabled):hover { opacity: 0.85; }

/* Messages */
.chat-messages {
  flex: 1;
  overflow-y: auto;
  padding: 12px 14px;
  display: flex;
  flex-direction: column;
  gap: 8px;
  min-height: 200px;
  max-height: 340px;
  scroll-behavior: smooth;
}
.chat-messages__empty {
  margin: auto;
  text-align: center;
  font-size: 0.8rem;
  color: #555;
}
.chat-msg { display: flex; flex-direction: column; max-width: 80%; }
.chat-msg--visitor { align-self: flex-end; align-items: flex-end; }
.chat-msg--admin  { align-self: flex-start; align-items: flex-start; }

.chat-msg__bubble {
  padding: 8px 12px;
  border-radius: 14px;
  font-size: 0.875rem;
  line-height: 1.45;
  word-break: break-word;
}
.chat-msg--visitor .chat-msg__bubble {
  background: #e63946;
  color: #fff;
  border-bottom-right-radius: 4px;
}
.chat-msg--admin .chat-msg__bubble {
  background: #1e1e1e;
  color: #f0f0f0;
  border-bottom-left-radius: 4px;
}
.chat-msg__time { font-size: 0.65rem; color: #555; margin-top: 2px; padding: 0 4px; }

.chat-closed {
  text-align: center;
  font-size: 0.8rem;
  color: #555;
  padding: 8px 0;
}
.chat-closed button {
  color: #e63946;
  background: none;
  border: none;
  cursor: pointer;
  font-size: 0.8rem;
  text-decoration: underline;
}

/* Input */
.chat-input {
  display: flex;
  align-items: flex-end;
  gap: 8px;
  padding: 10px 12px;
  border-top: 1px solid #1a1a1a;
}
.chat-input textarea {
  flex: 1;
  background: #0d0d0d;
  border: 1px solid #2a2a2a;
  border-radius: 10px;
  padding: 8px 12px;
  color: #f0f0f0;
  font-size: 0.875rem;
  resize: none;
  max-height: 100px;
  overflow-y: auto;
  line-height: 1.4;
  font-family: inherit;
}
.chat-input textarea:focus { outline: none; border-color: #e63946; }
.chat-send {
  width: 36px;
  height: 36px;
  border-radius: 50%;
  background: #e63946;
  color: #fff;
  border: none;
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
  transition: opacity 0.2s;
}
.chat-send:disabled { opacity: 0.4; cursor: not-allowed; }
.chat-send:not(:disabled):hover { opacity: 0.85; }
</style>
