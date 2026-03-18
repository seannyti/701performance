<template>
  <div class="admin-layout">
    <!-- Sidebar -->
    <aside class="admin-sidebar">
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
            <router-link to="/live-chat" class="nav-link" active-class="active">
              💬 Live Chat
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
import { useAuthStore } from '@/stores/auth'

const authStore = useAuthStore()

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
</style>