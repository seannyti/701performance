<script setup lang="ts">
import { ref, computed } from 'vue'
import { RouterLink, useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth.store'
import { useSettings } from '@/composables/useSettings'

const mobileOpen = ref(false)
const userMenuOpen = ref(false)
const auth = useAuthStore()
const router = useRouter()
const { settings } = useSettings()

const defaultNavLinks = [
  { name: 'Home',      to: '/' },
  { name: 'Inventory', to: '/inventory' },
  { name: 'About',     to: '/about' },
  { name: 'Contact',   to: '/contact' },
]

const navLinks = computed(() => {
  const links = settings.value?.general?.navLinks?.filter((l: { name: string; to: string }) => l.name?.trim() && l.to?.trim())
  return links && links.length > 0 ? links : defaultNavLinks
})

const displayName = computed(() => {
  if (auth.firstName) return auth.firstName
  if (!auth.email) return ''
  return auth.email.split('@')[0]
})

function logout() {
  userMenuOpen.value = false
  mobileOpen.value = false
  auth.logout()
  router.push('/')
}

function closeUserMenu() {
  userMenuOpen.value = false
}
</script>

<template>
  <header class="header">
    <div class="header__inner container">
      <RouterLink to="/" class="header__logo">
        {{ settings?.general?.businessName || 'M Performance Power' }}
      </RouterLink>

      <nav class="header__nav" :class="{ 'header__nav--open': mobileOpen }">
        <RouterLink
          v-for="link in navLinks"
          :key="link.to"
          :to="link.to"
          class="nav-link"
          @click="mobileOpen = false"
        >
          {{ link.name }}
        </RouterLink>

        <!-- Logged in: user dropdown -->
        <div v-if="auth.isAuthenticated" class="user-menu">
          <button class="user-menu__trigger" @click="userMenuOpen = !userMenuOpen">
            Hi, {{ displayName }}
            <svg class="user-menu__caret" :class="{ 'user-menu__caret--open': userMenuOpen }" width="12" height="12" viewBox="0 0 24 24" fill="currentColor">
              <path d="M7 10l5 5 5-5z"/>
            </svg>
          </button>
          <div v-if="userMenuOpen" class="user-menu__backdrop" @click="closeUserMenu" />
          <div v-if="userMenuOpen" class="user-menu__dropdown">
            <RouterLink to="/profile" class="dropdown-item" @click="closeUserMenu">
              <svg width="15" height="15" viewBox="0 0 24 24" fill="currentColor"><path d="M12 12c2.7 0 4.8-2.1 4.8-4.8S14.7 2.4 12 2.4 7.2 4.5 7.2 7.2 9.3 12 12 12zm0 2.4c-3.2 0-9.6 1.6-9.6 4.8v2.4h19.2v-2.4c0-3.2-6.4-4.8-9.6-4.8z"/></svg>
              My Account
            </RouterLink>
            <a v-if="auth.isAdmin" href="/admin/" target="_blank" rel="noopener" class="dropdown-item" @click="closeUserMenu">
              <svg width="15" height="15" viewBox="0 0 24 24" fill="currentColor"><path d="M3 13h8V3H3v10zm0 8h8v-6H3v6zm10 0h8V11h-8v10zm0-18v6h8V3h-8z"/></svg>
              Admin Dashboard
            </a>
            <div class="dropdown-divider" />
            <button class="dropdown-item dropdown-item--danger" @click="logout">
              <svg width="15" height="15" viewBox="0 0 24 24" fill="currentColor"><path d="M17 7l-1.41 1.41L18.17 11H8v2h10.17l-2.58 2.58L17 17l5-5-5-5zM4 5h8V3H4c-1.1 0-2 .9-2 2v14c0 1.1.9 2 2 2h8v-2H4V5z"/></svg>
              Logout
            </button>
          </div>
        </div>

        <!-- Logged out: login button -->
        <RouterLink v-else to="/login" class="nav-btn" @click="mobileOpen = false">
          Login
        </RouterLink>
      </nav>

      <button
        class="header__hamburger"
        :aria-expanded="mobileOpen"
        aria-label="Toggle menu"
        @click="mobileOpen = !mobileOpen"
      >
        <span />
        <span />
        <span />
      </button>
    </div>
  </header>
</template>

<style lang="scss" scoped>
@use '@/styles/variables' as *;
@use '@/styles/mixins' as *;

.header {
  position: sticky;
  top: 0;
  z-index: 100;
  background: rgba($color-dark, 0.97);
  backdrop-filter: blur(8px);
  border-bottom: 1px solid $color-border;
  height: $header-height;

  &__inner {
    display: flex;
    align-items: center;
    justify-content: space-between;
    height: 100%;
  }

  &__logo {
    font-size: 1.1rem;
    font-weight: 800;
    color: $color-text;
    letter-spacing: -0.02em;

    strong { color: var(--color-primary); }
  }

  &__nav {
    display: none;
    align-items: center;
    gap: $space-lg;

    @include respond-to(md) { display: flex; }
  }

  &__hamburger {
    display: flex;
    flex-direction: column;
    gap: 5px;
    padding: $space-sm;

    @include respond-to(md) { display: none; }

    span {
      display: block;
      width: 24px;
      height: 2px;
      background: $color-text;
      border-radius: 2px;
    }
  }
}

.nav-link {
  font-size: 0.9rem;
  color: $color-muted;
  transition: color 0.15s;

  &:hover, &.router-link-active { color: var(--color-primary); }
}

.nav-btn {
  font-size: 0.875rem;
  font-weight: 600;
  padding: $space-sm $space-md;
  background: var(--color-primary);
  color: #fff;
  border-radius: $border-radius;
  border: none;
  cursor: pointer;
  transition: background 0.2s;

  &:hover { background: var(--color-primary-dark); }
}

// ── User dropdown ─────────────────────────────────────────────
.user-menu {
  position: relative;
}

.user-menu__backdrop {
  position: fixed;
  inset: 0;
  z-index: 199;
}

.user-menu__trigger {
  display: flex;
  align-items: center;
  gap: 4px;
  font-size: 0.875rem;
  font-weight: 600;
  color: $color-text;
  background: none;
  border: 1px solid $color-border;
  border-radius: $border-radius;
  padding: $space-sm $space-md;
  cursor: pointer;
  transition: border-color 0.15s, color 0.15s;

  &:hover { border-color: var(--color-primary); color: var(--color-primary); }
}

.user-menu__caret {
  transition: transform 0.2s;
  color: $color-muted;

  &--open { transform: rotate(180deg); }
}

.user-menu__dropdown {
  position: absolute;
  top: calc(100% + 8px);
  right: 0;
  min-width: 180px;
  background: $color-surface;
  border: 1px solid $color-border;
  border-radius: 10px;
  padding: 6px;
  box-shadow: 0 8px 24px rgba(0, 0, 0, 0.4);
  z-index: 200;
}

.dropdown-item {
  display: flex;
  align-items: center;
  gap: 8px;
  width: 100%;
  padding: 9px 12px;
  font-size: 0.875rem;
  color: $color-text;
  border-radius: 6px;
  border: none;
  background: none;
  cursor: pointer;
  text-align: left;
  transition: background 0.15s;

  svg { color: $color-muted; flex-shrink: 0; }

  &:hover { background: $color-surface-2; }

  &--danger {
    color: var(--color-primary);
    svg { color: var(--color-primary); }
    &:hover { background: rgba(var(--color-primary-rgb), 0.08); }
  }
}

.dropdown-divider {
  height: 1px;
  background: $color-border;
  margin: 4px 6px;
}
</style>
