<script setup lang="ts">
import { ref, computed, onMounted, onUnmounted } from 'vue'
import { RouterLink, useRoute } from 'vue-router'
import { useSettingsStore } from '../../stores/settings'

const settings = useSettingsStore()
const route = useRoute()
const scrolled = ref(false)
const mobileOpen = ref(false)

const logoNameParts = computed(() => {
  const name = settings.get('site_name', 'PerformancePower')
  const accent = settings.get('site_name_accent', '')
  if (!accent || !name.includes(accent)) return { before: name, accent: '', after: '' }
  const idx = name.indexOf(accent)
  return { before: name.substring(0, idx), accent, after: name.substring(idx + accent.length) }
})

const navLinks = [
  { name: 'Home', to: '/' },
  { name: 'Inventory', to: '/inventory' },
  { name: 'Finance', to: '/finance' },
  { name: 'About', to: '/about' },
  { name: 'Contact', to: '/contact' },
]

function handleScroll() {
  scrolled.value = window.scrollY > 20
}

onMounted(() => window.addEventListener('scroll', handleScroll))
onUnmounted(() => window.removeEventListener('scroll', handleScroll))
</script>

<template>
  <nav class="navbar" :class="{ scrolled, 'mobile-open': mobileOpen }">
    <div class="container navbar__inner">
      <RouterLink to="/" class="navbar__logo">
        <img
          v-if="settings.get('theme_logo_url')"
          :src="settings.get('theme_logo_url')"
          alt="Logo"
          class="logo-img"
        />
        <span v-else class="logo-text">{{ logoNameParts.before }}<span class="logo-accent">{{ logoNameParts.accent }}</span>{{ logoNameParts.after }}</span>
      </RouterLink>

      <ul class="navbar__links">
        <li v-for="link in navLinks" :key="link.to">
          <RouterLink :to="link.to" class="navbar__link" :class="{ active: route.path === link.to }">
            {{ link.name }}
          </RouterLink>
        </li>
      </ul>

      <!-- Mobile hamburger -->
      <button class="navbar__hamburger" @click="mobileOpen = !mobileOpen" aria-label="Menu">
        <span /><span /><span />
      </button>
    </div>

    <!-- Mobile drawer -->
    <div class="navbar__mobile" :class="{ open: mobileOpen }">
      <ul class="mobile-nav-links">
        <li v-for="link in navLinks" :key="link.to">
          <RouterLink :to="link.to" :class="{ active: route.path === link.to }" @click="mobileOpen = false">{{ link.name }}</RouterLink>
        </li>
      </ul>
    </div>
  </nav>
</template>

<style lang="scss" scoped>
@use '../../assets/styles/variables' as *;
@use '../../assets/styles/mixins' as *;

.navbar {
  height: $navbar-height;
  transition: background $transition-base, backdrop-filter $transition-base, border-color $transition-base;
  border-bottom: 1px solid transparent;

  &.scrolled {
    background: rgba(10, 10, 10, 0.95);
    backdrop-filter: blur(12px);
    border-color: $border;
  }

  &__inner {
    @include flex-between;
    height: 100%;
  }

  &__logo {
    .logo-text {
      font-size: $font-size-xl;
      font-weight: 800;
      letter-spacing: -0.5px;
    }
    .logo-accent { color: $primary; }
    .logo-img { height: 40px; width: auto; object-fit: contain; }
  }

  &__links {
    display: none;
    list-style: none;
    gap: $spacing-xl;

    @include respond-to(lg) {
      display: flex;
    }
  }

  &__link {
    font-size: $font-size-sm;
    font-weight: 500;
    color: $text-muted;
    text-transform: uppercase;
    letter-spacing: 0.5px;
    transition: color $transition-fast;

    &:hover, &.active {
      color: $text;
    }
    &.active { color: $primary; }
  }

  &__hamburger {
    display: flex;
    flex-direction: column;
    gap: 5px;
    background: none;
    border: none;
    cursor: pointer;
    padding: $spacing-sm;

    @include respond-to(lg) { display: none; }

    span {
      display: block;
      width: 24px;
      height: 2px;
      background: $text;
      border-radius: 2px;
      transition: $transition-fast;
    }
  }

  &__mobile {
    display: none;
    background: $bg-card;
    border-top: 1px solid $border;
    padding: $spacing-lg $spacing-lg $spacing-xl;

    @media (max-width: #{$bp-lg - 1px}) {
      display: none;
      &.open { display: block; }
    }
  }
}

.mobile-nav-links {
  list-style: none;
  display: flex;
  flex-direction: column;
  gap: 0;

  a {
    display: block;
    padding: 0.875rem 0;
    font-size: $font-size-lg;
    font-weight: 600;
    color: $text-secondary;
    border-bottom: 1px solid rgba(255,255,255,0.05);
    transition: color $transition-fast;

    &:hover, &.active { color: $primary; }
  }
}
</style>
