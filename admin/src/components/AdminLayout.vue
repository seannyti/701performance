<template>
  <div class="admin-layout">
    <!-- Mobile top bar -->
    <div class="mobile-topbar">
      <button class="mobile-menu-btn" @click="sidebarOpen = !sidebarOpen" aria-label="Toggle menu">
        <span class="hamburger-line"></span>
        <span class="hamburger-line"></span>
        <span class="hamburger-line"></span>
      </button>
      <span class="mobile-title">🏍️ Admin Dashboard</span>
    </div>

    <!-- Sidebar overlay (mobile) -->
    <div v-if="sidebarOpen" class="sidebar-overlay" @click="sidebarOpen = false"></div>

    <!-- Sidebar -->
    <aside class="admin-sidebar" :class="{ 'sidebar-open': sidebarOpen }">
      <div class="sidebar-header mb-6">
        <h2 style="color: #4f46e5; font-size: 1.5rem; font-weight: bold;">
          🏍️ Admin Dashboard
        </h2>
        <p class="text-muted" style="font-size: 0.875rem; margin-top: 0.5rem;">
          Welcome, {{ authStore.user?.firstName }}!
        </p>
      </div>

      <nav>
        <ul class="nav-list">
          <li class="nav-item">
            <router-link to="/" class="nav-link" active-class="active">
              📊 Dashboard
            </router-link>
          </li>
          <li class="nav-item">
            <router-link to="/catalog" class="nav-link" active-class="active">
              📦 Catalog
            </router-link>
          </li>
          <li class="nav-item">
            <router-link to="/inquiries" class="nav-link" active-class="active">
              📧 Inquiries
            </router-link>
          </li>
          <li class="nav-item">
            <router-link to="/orders" class="nav-link" active-class="active">
              🛒 Orders
            </router-link>
          </li>
          <li class="nav-item">
            <router-link to="/calendar" class="nav-link" active-class="active">
              📅 Calendar
            </router-link>
          </li>
          <li class="nav-item">
            <router-link to="/live-chat" class="nav-link nav-link--badged" active-class="active">
              <span>💬 Live Chat</span>
              <span v-if="chatBadge > 0" class="nav-badge">{{ chatBadge > 99 ? '99+' : chatBadge }}</span>
            </router-link>
          </li>
          <li class="nav-item">
            <router-link to="/media" class="nav-link" active-class="active">
              📁 Media Library
            </router-link>
          </li>
          <li class="nav-item">
            <router-link to="/music" class="nav-link nav-sub" active-class="active">
              🎵 Music Player
            </router-link>
          </li>
          <li class="nav-item" v-if="authStore.isSuperAdmin">
            <router-link to="/users" class="nav-link" active-class="active">
              👥 Users
            </router-link>
          </li>
          <li class="nav-item" v-if="authStore.isSuperAdmin">
            <router-link to="/backup" class="nav-link" active-class="active">
              💾 Backup & Restore
            </router-link>
          </li>
          <li class="nav-item">
            <router-link to="/settings" class="nav-link" active-class="active">
              ⚙️ Site Settings
            </router-link>
          </li>
        </ul>

        <div style="margin-top: 2rem; padding-top: 2rem; border-top: 1px solid #334155;">
          <button @click="handleLogout" class="nav-link" style="width: 100%; text-align: left; border: none; background: none; color: inherit;">
            🚪 Logout
          </button>
        </div>
      </nav>
    </aside>

    <!-- Main Content -->
    <main class="admin-main">
      <slot />
    </main>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, onUnmounted } from 'vue'
import { useAuthStore } from '@/stores/auth'
import { useRouter } from 'vue-router'
import axios from 'axios'

const authStore = useAuthStore()
const router = useRouter()

const sidebarOpen = ref(false)

// Close sidebar on route change (mobile nav tap)
router.afterEach(() => {
  sidebarOpen.value = false
})

const API_URL = import.meta.env.VITE_API_URL || 'http://localhost:5226'
const chatBadge = ref(0)
let chatPollTimer: ReturnType<typeof setInterval> | null = null

async function loadChatBadge() {
  try {
    const res = await axios.get(`${API_URL}/api/v1/chat/sessions`, {
      headers: { Authorization: `Bearer ${authStore.token}` }
    })
    chatBadge.value = (res.data as { status: string }[]).filter(
      s => s.status === 'Waiting' || s.status === 'Active'
    ).length
  } catch { chatBadge.value = 0 }
}

onMounted(() => {
  loadChatBadge()
  chatPollTimer = setInterval(loadChatBadge, 30_000)
})

onUnmounted(() => {
  if (chatPollTimer) clearInterval(chatPollTimer)
})

const handleLogout = () => {
  authStore.logout()
  window.close()
  // Fallback: if the browser blocked window.close() (tab was navigated directly),
  // redirect to login after a short delay
  setTimeout(() => {
    window.location.href = '/login'
  }, 300)
}
</script>

<style scoped>
/* Styles are already in the main CSS file */
.nav-sub {
  padding-left: 2rem;
}

/* Mobile top bar — hidden on desktop */
.mobile-topbar {
  display: none;
}

/* Sidebar overlay (darkens content when sidebar is open on mobile) */
.sidebar-overlay {
  display: none;
}

@media (max-width: 768px) {
  .mobile-topbar {
    display: flex;
    align-items: center;
    gap: 1rem;
    background: #1e293b;
    padding: 0.75rem 1rem;
    position: sticky;
    top: 0;
    z-index: 200;
    grid-column: 1 / -1;
  }

  .mobile-title {
    color: white;
    font-weight: 700;
    font-size: 1.1rem;
  }

  .mobile-menu-btn {
    background: none;
    border: none;
    cursor: pointer;
    display: flex;
    flex-direction: column;
    gap: 5px;
    padding: 4px;
  }

  .hamburger-line {
    display: block;
    width: 24px;
    height: 2px;
    background: white;
    border-radius: 2px;
  }

  /* Sidebar hidden by default on mobile, slides in when open */
  :deep(.admin-sidebar) {
    position: fixed;
    top: 0;
    left: 0;
    bottom: 0;
    width: 260px;
    z-index: 300;
    transform: translateX(-100%);
    transition: transform 0.25s ease;
    overflow-y: auto;
  }

  :deep(.admin-sidebar.sidebar-open) {
    transform: translateX(0);
  }

  .sidebar-overlay {
    display: block;
    position: fixed;
    inset: 0;
    background: rgba(0, 0, 0, 0.5);
    z-index: 299;
  }
}
</style>