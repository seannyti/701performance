<template>
  <div id="app">
    <!-- Header component - hidden during maintenance and on special pages -->
    <Header v-if="!hideHeaderFooter" />
    
    <!-- Main content area -->
    <main class="main-content" :class="{ 'full-viewport': hideHeaderFooter }">
      <router-view v-slot="{ Component, route }">
        <Transition name="fade" mode="out-in">
          <div :key="route.path">
            <component :is="Component" v-if="Component" />
          </div>
        </Transition>
      </router-view>
    </main>
    
    <!-- Footer component - hidden during maintenance and on special pages -->
    <Footer v-if="!hideHeaderFooter" />

    <!-- Live chat widget - floating, hidden on special pages and for admins -->
    <ChatWidget />

    <!-- Admin chat bell - shown only for admins, links to admin live chat panel -->
    <AdminChatBell v-if="authStore.hasAdminAccess" />

    <!-- Toast notifications -->
    <Teleport to="body">
      <div class="toast-container">
        <TransitionGroup name="toast">
          <div
            v-for="toast in toasts"
            :key="toast.id"
            class="toast"
            :class="`toast--${toast.type}`"
          >
            <span class="toast-icon">
              <span v-if="toast.type === 'success'">✓</span>
              <span v-else-if="toast.type === 'error'">✕</span>
              <span v-else-if="toast.type === 'warning'">⚠</span>
              <span v-else>ℹ</span>
            </span>
            {{ toast.message }}
          </div>
        </TransitionGroup>
      </div>
    </Teleport>
  </div>
</template>

<script setup lang="ts">
import { computed, onMounted, onUnmounted } from 'vue'
import { useRoute } from 'vue-router'
import Header from './components/Header.vue';
import Footer from './components/Footer.vue';
import ChatWidget from './components/ChatWidget.vue';
import AdminChatBell from './components/AdminChatBell.vue';
import { useTheme } from './composables/useTheme';
import { useToast } from './composables/useToast';
import { useAuthStore } from './stores/auth';

// Initialize theme
useTheme()

const { toasts } = useToast()
const route = useRoute()
const authStore = useAuthStore()

// Hide header/footer only on maintenance, login, and signup pages
const hideHeaderFooter = computed(() => {
  const specialPages = ['/maintenance', '/login', '/signup']
  return specialPages.includes(route.path)
})

// ── Visitor heartbeat ───────────────────────────────────────
// Lets the admin dashboard show a live visitor count. Pings every 60s.
const API_URL = import.meta.env.VITE_API_URL || 'http://localhost:5226'

function getVisitorSessionId(): string {
  const key = 'visitor_session_id'
  let id = sessionStorage.getItem(key)
  if (!id) {
    id = crypto.randomUUID()
    sessionStorage.setItem(key, id)
  }
  return id
}

async function sendHeartbeat() {
  try {
    await fetch(`${API_URL}/api/v1/visitors/heartbeat`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ sessionId: getVisitorSessionId() }),
      keepalive: true
    })
  } catch { /* silent */ }
}

let heartbeatTimer: ReturnType<typeof setInterval> | null = null

onMounted(() => {
  sendHeartbeat()
  heartbeatTimer = setInterval(sendHeartbeat, 60_000)
})

onUnmounted(() => {
  if (heartbeatTimer) clearInterval(heartbeatTimer)
})
</script>

<style>
/* Global styles with theme variables */
:root {
  /* Default values - will be overridden by theme */
  --color-primary: #CC0000;
  --color-secondary: #9A9A9A;
  --color-accent: #FF3333;
  --color-bg: #0A0A0A;
  --color-text-primary: #FFFFFF;
  --font-body: 'Inter', system-ui, sans-serif;
  --font-heading: 'Rajdhani', 'Inter', system-ui, sans-serif;
  --font-size-base: 16px;
  --line-height-body: 1.6;
  --card-radius: 6px;
  --button-radius: 3px;
  --transition-duration: 250ms;
}

* {
  margin: 0;
  padding: 0;
  box-sizing: border-box;
}

html {
  font-size: var(--font-size-base, 16px);
  scroll-behavior: var(--scroll-behavior, smooth);
  overflow-x: hidden;
}

body {
  font-family: var(--font-body, 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif);
  font-weight: var(--font-weight-body, 400);
  line-height: var(--line-height-body, 1.6);
  color: var(--color-text-primary, #333);
  background-color: var(--color-bg, #f8f9fa);
  margin: 0;
  padding: 0;
  overflow-x: hidden;
}

/* Apply heading styles */
h1, h2, h3, h4, h5, h6 {
  font-family: var(--font-heading, inherit);
  font-weight: var(--font-weight-heading, 700);
  line-height: var(--line-height-heading, 1.2);
  text-shadow: var(--heading-text-shadow, none);
  letter-spacing: var(--heading-letter-spacing, 0);
  color: var(--color-text-primary);
}

h1 {
  font-size: var(--font-size-h1, 2.5rem);
}

h2 {
  font-size: var(--font-size-h2, 2rem);
}

h3 {
  font-size: var(--font-size-h3, 1.5rem);
}

#app {
  min-height: 100vh;
  display: flex;
  flex-direction: column;
  margin: 0;
  padding: 0;
}

.main-content {
  flex: 1;
  display: block;
  margin: 0;
  padding: 0;
  background: transparent;
}

/* Full viewport for pages without header/footer */
.main-content.full-viewport {
  padding: 0;
  min-height: 100vh;
}

/* Container utility */
.container {
  max-width: var(--container-max-width, 1200px);
  margin: 0 auto;
  padding: 0 var(--container-padding, 1rem);
}

/* Grid utilities */
.grid {
  display: grid;
  gap: var(--element-gap, 2rem);
}

.grid-1 { grid-template-columns: 1fr; }
.grid-2 { grid-template-columns: repeat(2, 1fr); }
.grid-3 { grid-template-columns: repeat(3, 1fr); }
.grid-4 { grid-template-columns: repeat(4, 1fr); }

@media (max-width: 1024px) {
  .grid-4 { grid-template-columns: repeat(3, 1fr); }
}

@media (max-width: 768px) {
  .grid-3,
  .grid-4 { grid-template-columns: repeat(2, 1fr); }
}

@media (max-width: 480px) {
  .grid-2,
  .grid-3,
  .grid-4 { grid-template-columns: 1fr; }
}

/* Typography utilities */
.text-center { text-align: center; }
.text-left { text-align: left; }
.text-right { text-align: right; }

.font-bold { font-weight: bold; }
.font-semibold { font-weight: 600; }
.font-medium { font-weight: 500; }

/* Spacing utilities */
.mb-1 { margin-bottom: 0.5rem; }
.mb-2 { margin-bottom: 1rem; }
.mb-3 { margin-bottom: 1.5rem; }
.mb-4 { margin-bottom: 2rem; }

.mt-1 { margin-top: 0.5rem; }
.mt-2 { margin-top: 1rem; }
.mt-3 { margin-top: 1.5rem; }
.mt-4 { margin-top: 2rem; }

.p-1 { padding: 0.5rem; }
.p-2 { padding: 1rem; }
.p-3 { padding: 1.5rem; }
.p-4 { padding: 2rem; }

/* Button styles */
.btn {
  display: inline-block;
  padding: var(--button-padding-y, 0.75rem) var(--button-padding-x, 1.5rem);
  border: 2px solid transparent;
  border-radius: var(--button-radius, 8px);
  font-weight: var(--button-font-weight, 600);
  text-transform: var(--button-text-transform, none);
  text-decoration: none;
  text-align: center;
  cursor: pointer;
  transition: all var(--transition-duration, 0.3s) var(--transition-timing, ease);
  font-size: 1rem;
}

.btn-primary {
  background-color: var(--color-primary, #ff6b35);
  color: white;
  border-color: var(--color-primary, #ff6b35);
}

.btn-primary:hover {
  filter: brightness(0.9);
  transform: translateY(calc(var(--hover-lift-amount, 4px) * -0.25));
  box-shadow: var(--hover-shadow, 0 4px 12px rgba(0, 0, 0, 0.15));
}

.btn-secondary {
  background-color: var(--color-secondary, #6c757d);
  color: white;
  border-color: var(--color-secondary, #6c757d);
}

.btn-secondary:hover {
  filter: brightness(0.9);
  transform: translateY(calc(var(--hover-lift-amount, 4px) * -0.25));
}

.btn-outline {
  background-color: transparent;
  border: 2px solid var(--color-primary, #ff6b35);
  color: var(--color-primary, #ff6b35);
}

.btn-outline:hover {
  background-color: var(--color-primary, #ff6b35);
  color: white;
}

/* Form styles */
.form-group {
  margin-bottom: 1.5rem;
}

.form-label {
  display: block;
  margin-bottom: 0.5rem;
  font-weight: 600;
  color: var(--color-text-primary, #333);
}

.form-control {
  width: 100%;
  padding: 0.75rem;
  border: var(--input-border-width, 2px) solid var(--color-border, #ddd);
  border-radius: var(--input-radius, 8px);
  font-size: 1rem;
  font-family: var(--font-body);
  background-color: var(--color-bg, white);
  color: var(--color-text-primary, #333);
  transition: all var(--transition-duration, 0.3s) var(--transition-timing, ease);
}

.form-control:focus {
  outline: none;
  border-color: var(--input-focus-color, #ff6b35);
  box-shadow: 0 0 0 3px rgba(99, 102, 241, 0.1);
}

.form-control.error {
  border-color: var(--color-danger, #dc3545);
}

/* Loading and error states */
.loading {
  text-align: center;
  padding: 2rem;
  color: var(--color-text-secondary, #666);
}

.error {
  background-color: #f8d7da;
  color: var(--color-danger, #721c24);
  padding: 1rem;
  border-radius: var(--card-radius, 8px);
  margin-bottom: 1rem;
  border: 1px solid #f5c6cb;
}

.success {
  background-color: #d4edda;
  color: var(--color-success, #155724);
  padding: 1rem;
  border-radius: var(--card-radius, 8px);
  margin-bottom: 1rem;
  border: 1px solid #c3e6cb;
}

/* Section styles */
.section {
  padding: var(--section-padding-top, 3rem) 0 var(--section-padding-bottom, 3rem) 0;
}

.section-title {
  font-size: var(--font-size-h1, 2.5rem);
  font-weight: var(--font-weight-heading, bold);
  font-family: var(--font-heading);
  text-align: center;
  margin-bottom: 1rem;
  color: var(--color-text-primary, #333);
  text-shadow: var(--heading-text-shadow, none);
  letter-spacing: var(--heading-letter-spacing, 0);
}

.section-subtitle {
  font-size: 1.25rem;
  text-align: center;
  color: var(--color-text-secondary, #666);
  margin-bottom: 3rem;
  max-width: 600px;
  margin-left: auto;
  margin-right: auto;
}

/* Page transition animations */
.fade-enter-active,
.fade-leave-active {
  transition: opacity var(--transition-duration, 0.3s) var(--transition-timing, ease);
}

.fade-enter-from,
.fade-leave-to {
  opacity: 0;
}

/* Card styles with theme support */
.card {
  background-color: var(--color-bg-secondary, white);
  border-radius: var(--card-radius, 12px);
  padding: var(--card-padding, 1.5rem);
  box-shadow: var(--card-shadow, 0 1px 3px 0 rgb(0 0 0 / 0.1));
  transition: all var(--transition-duration, 0.3s) var(--transition-timing, ease);
}

.card:hover {
  transform: translateY(calc(var(--hover-lift-amount, 4px) * -1));
  box-shadow: var(--hover-shadow, 0 8px 20px 0 rgb(0 0 0 / 0.15));
}

/* Image hover effects */
img.theme-image {
  transition: all var(--transition-duration, 0.3s) var(--transition-timing, ease);
}

/* Zoom effect */
[data-image-effect="zoom"] img.theme-image:hover {
  transform: scale(1.1);
}

/* Zoom out effect */
[data-image-effect="zoom-out"] img.theme-image:hover {
  transform: scale(0.95);
}

/* Brightness effect */
[data-image-effect="brightness"] img.theme-image:hover {
  filter: brightness(1.2);
}

/* Grayscale to color effect */
[data-image-effect="grayscale"] img.theme-image {
  filter: grayscale(100%);
}

[data-image-effect="grayscale"] img.theme-image:hover {
  filter: grayscale(0%);
}

/* Blur effect */
[data-image-effect="blur"] img.theme-image {
  filter: blur(2px);
}

[data-image-effect="blur"] img.theme-image:hover {
  filter: blur(0);
}

/* Glass morphism support */
.glass-effect {
  background: rgba(255, 255, 255, 0.1);
  backdrop-filter: blur(var(--backdrop-blur, 10px));
  border: 1px solid rgba(255, 255, 255, 0.2);
}

/* Gradient overlay support */
.gradient-overlay::before {
  content: '';
  position: absolute;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background: var(--gradient);
  opacity: var(--gradient-opacity, 0.7);
  pointer-events: none;
}

/* Scroll animations */
[data-animate] {
  opacity: 0;
  transform: translateY(30px);
  transition: opacity 0.8s ease, transform 0.8s ease;
}

[data-animate].animate-in {
  opacity: 1;
  transform: translateY(0);
}

[data-animate="fade"] {
  transform: none;
}

[data-animate="fade"].animate-in {
  opacity: 1;
}

[data-animate="slide-left"] {
  transform: translateX(-30px);
}

[data-animate="slide-left"].animate-in {
  transform: translateX(0);
}

[data-animate="slide-right"] {
  transform: translateX(30px);
}

[data-animate="slide-right"].animate-in {
  transform: translateX(0);
}

[data-animate="zoom"] {
  transform: scale(0.9);
}

[data-animate="zoom"].animate-in {
  transform: scale(1);
}

/* Responsive container adjustments */
@media (max-width: 1200px) {
  .container {
    max-width: 1000px;
  }
}

@media (max-width: 768px) {
  .section {
    padding: 2rem 0;
  }

  .section-title {
    font-size: 2rem;
  }

  .section-subtitle {
    font-size: 1.1rem;
  }
}

@media (max-width: 480px) {
  .container {
    padding: 0 0.75rem;
  }

  .section-title {
    font-size: 1.75rem;
  }
}

/* Toast notifications */
.toast-container {
  position: fixed;
  bottom: 1.5rem;
  right: 1.5rem;
  z-index: 9999;
  display: flex;
  flex-direction: column;
  gap: 0.625rem;
  pointer-events: none;
}

.toast {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  padding: 0.75rem 1.125rem;
  border-radius: 8px;
  font-size: 0.9rem;
  font-weight: 500;
  color: #fff;
  box-shadow: 0 4px 14px rgba(0, 0, 0, 0.25);
  pointer-events: auto;
  max-width: 360px;
}

.toast--info    { background: #3b82f6; }
.toast--success { background: #22c55e; }
.toast--warning { background: #f59e0b; }
.toast--error   { background: #ef4444; }

.toast-icon { font-size: 1rem; flex-shrink: 0; }

.toast-enter-active,
.toast-leave-active { transition: all 0.3s ease; }
.toast-enter-from   { opacity: 0; transform: translateX(60px); }
.toast-leave-to     { opacity: 0; transform: translateX(60px); }
</style>