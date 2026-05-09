<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { RouterLink, useRoute, useRouter } from 'vue-router'
import { usePortalAuthStore } from '../stores/auth'
import { useSettingsStore } from '../stores/settings'
import Button from 'primevue/button'
import Avatar from 'primevue/avatar'
import Menu from 'primevue/menu'

const auth = usePortalAuthStore()
const settings = useSettingsStore()

onMounted(() => settings.fetchSettings())

const route = useRoute()
const router = useRouter()
const sidebarCollapsed = ref(false)
const mobileOpen = ref(false)
const userMenuRef = ref()

function closeMobile() { mobileOpen.value = false }

const logoUrl = computed(() => settings.get('theme_logo_url'))
const logoNameParts = computed(() => {
  const name = settings.get('site_name', 'PerformancePower')
  const accent = settings.get('site_name_accent', '')
  if (!accent || !name.includes(accent)) return { before: name, accent: '', after: '' }
  const idx = name.indexOf(accent)
  return { before: name.substring(0, idx), accent, after: name.substring(idx + accent.length) }
})
const logoInitials = computed(() => {
  const name = settings.get('site_name', 'PerformancePower')
  const words = name.trim().split(/\s+/)
  if (words.length >= 2) return (words[0][0] + words[1][0]).toUpperCase()
  return name.substring(0, 2).toUpperCase()
})

const navItems = [
  { label: 'Inventory', icon: 'pi pi-car', to: '/inventory' },
  { label: 'Site Settings', icon: 'pi pi-cog', to: '/settings' },
]

function isActive(to: string) {
  if (route.path === to) return true
  if (to !== '/' && route.path.startsWith(to + '/')) return true
  return false
}

const userMenuItems = [
  {
    label: auth.fullName,
    items: [
      { label: 'Two-Factor Authentication', icon: 'pi pi-shield', command: () => router.push('/security/mfa') },
      { separator: true },
      { label: 'Logout', icon: 'pi pi-sign-out', command: handleLogout }
    ]
  }
]

async function handleLogout() {
  await auth.logout()
  router.push('/login')
}

function toggleUserMenu(event: Event) {
  userMenuRef.value.toggle(event)
}
</script>

<template>
  <div class="portal-shell" :class="{ 'sidebar-collapsed': sidebarCollapsed }">
    <div v-if="mobileOpen" class="mobile-overlay" @click="closeMobile" />

    <aside class="sidebar" :class="{ 'mobile-open': mobileOpen }">
      <div class="sidebar__header">
        <RouterLink to="/inventory" class="sidebar__logo">
          <template v-if="!sidebarCollapsed">
            <img v-if="logoUrl" :src="logoUrl" alt="Logo" class="sidebar__logo-img" />
            <span v-else>{{ logoNameParts.before }}<span class="accent">{{ logoNameParts.accent }}</span>{{ logoNameParts.after }}</span>
          </template>
          <span v-else class="accent">{{ logoInitials }}</span>
        </RouterLink>
        <Button
          :icon="sidebarCollapsed ? 'pi pi-angle-right' : 'pi pi-angle-left'"
          text rounded class="sidebar__toggle"
          @click="sidebarCollapsed = !sidebarCollapsed"
        />
      </div>

      <nav class="sidebar__nav">
        <RouterLink
          v-for="item in navItems"
          :key="item.to"
          :to="item.to"
          class="nav-item"
          :class="{ active: isActive(item.to) }"
          @click="closeMobile"
        >
          <i :class="item.icon" class="nav-item__icon" />
          <span v-if="!sidebarCollapsed" class="nav-item__label">{{ item.label }}</span>
        </RouterLink>
      </nav>

      <div class="sidebar__user" @click="toggleUserMenu">
        <Avatar :label="auth.initials" shape="circle" class="user-avatar" />
        <div v-if="!sidebarCollapsed" class="user-info">
          <span class="user-name">{{ auth.fullName }}</span>
          <span class="user-role">{{ auth.user?.role }}</span>
        </div>
      </div>
      <Menu ref="userMenuRef" :model="userMenuItems" popup />
    </aside>

    <div class="portal-main">
      <header class="topbar">
        <div class="topbar__title">
          <Button icon="pi pi-bars" text rounded class="mobile-menu-btn" @click="mobileOpen = !mobileOpen" />
          <h1>{{ route.meta.title as string || '' }}</h1>
        </div>
        <div class="topbar__actions">
          <a href="/" target="_blank" class="view-site-btn">
            <i class="pi pi-external-link" />
            <span>View Site</span>
          </a>
        </div>
      </header>

      <main class="portal-content">
        <RouterView />
      </main>
    </div>
  </div>
</template>

<style scoped>
.portal-shell { display: flex; min-height: 100vh; background: #0a0a0a; }

.sidebar {
  width: 260px; min-height: 100vh; background: #111111;
  border-right: 1px solid #2a2a2a; display: flex; flex-direction: column;
  transition: width 0.25s ease; position: fixed; top: 0; left: 0; bottom: 0;
  z-index: 50; overflow: hidden;
}
.sidebar-collapsed .sidebar { width: 72px; }

.sidebar__header {
  display: flex; align-items: center; justify-content: space-between;
  padding: 1.25rem 1rem; border-bottom: 1px solid #2a2a2a; min-height: 64px;
}
.sidebar__logo { font-size: 1.125rem; font-weight: 900; color: white; white-space: nowrap; text-decoration: none; }
.accent { color: #e53935; }
.sidebar__logo-img { max-height: 32px; max-width: 160px; object-fit: contain; }

.sidebar__nav { flex: 1; overflow-y: auto; overflow-x: hidden; padding: 0.5rem 0; }

.nav-item {
  display: flex; align-items: center; gap: 0.875rem; padding: 0.625rem 1rem;
  color: #9e9e9e; text-decoration: none; font-size: 0.875rem; font-weight: 500;
  border-radius: 8px; margin: 2px 8px; transition: background 0.15s, color 0.15s;
  white-space: nowrap;
  &:hover { background: #1e1e1e; color: white; }
  &.active { background: rgba(229, 57, 53, 0.15); color: #e53935; }
}
.nav-item__icon { font-size: 1rem; width: 20px; text-align: center; flex-shrink: 0; }

.sidebar__user {
  padding: 1rem; border-top: 1px solid #2a2a2a; display: flex; align-items: center;
  gap: 0.75rem; cursor: pointer; transition: background 0.15s; border-radius: 8px; margin: 8px;
  &:hover { background: #1e1e1e; }
}
.user-avatar { background: #e53935 !important; color: white !important; font-weight: 700 !important; flex-shrink: 0; }
.user-info { display: flex; flex-direction: column; overflow: hidden; }
.user-name { font-size: 0.875rem; font-weight: 600; color: white; white-space: nowrap; overflow: hidden; text-overflow: ellipsis; }
.user-role { font-size: 0.7rem; color: #9e9e9e; text-transform: capitalize; }

.portal-main { flex: 1; margin-left: 260px; transition: margin-left 0.25s ease; display: flex; flex-direction: column; min-width: 0; }
.sidebar-collapsed .portal-main { margin-left: 72px; }

.topbar {
  height: 64px; background: #111111; border-bottom: 1px solid #2a2a2a;
  display: flex; align-items: center; justify-content: space-between;
  padding: 0 1.5rem; position: sticky; top: 0; z-index: 40;
  h1 { font-size: 1.125rem; font-weight: 600; color: white; }
}
.topbar__title { display: flex; align-items: center; gap: 0.75rem; }
.topbar__actions { display: flex; align-items: center; gap: 0.5rem; }

.view-site-btn {
  display: flex; align-items: center; gap: 0.5rem; padding: 0.5rem 1rem;
  background: rgba(229, 57, 53, 0.1); border: 1px solid rgba(229, 57, 53, 0.3);
  border-radius: 8px; color: #e53935; font-size: 0.8rem; font-weight: 600;
  transition: background 0.15s, border-color 0.15s; white-space: nowrap;
  i { font-size: 0.75rem; }
  &:hover { background: rgba(229, 57, 53, 0.18); border-color: rgba(229, 57, 53, 0.6); }
  span { display: none; }
  @media (min-width: 640px) { span { display: inline; } }
}

.portal-content { flex: 1; padding: 1.5rem; overflow-x: hidden; overflow-y: auto; min-width: 0; }

.mobile-menu-btn { display: none; }
.mobile-overlay { display: none; }

@media (max-width: 768px) {
  .mobile-menu-btn { display: flex; }
  .sidebar { transform: translateX(-100%); transition: transform 0.25s ease; }
  .sidebar.mobile-open { transform: translateX(0); }
  .sidebar-collapsed .sidebar { width: 260px; transform: translateX(-100%); }
  .sidebar-collapsed .sidebar.mobile-open { transform: translateX(0); }
  .portal-main { margin-left: 0 !important; }
  .mobile-overlay { display: block; position: fixed; inset: 0; background: rgba(0,0,0,0.6); z-index: 49; }
}
</style>
