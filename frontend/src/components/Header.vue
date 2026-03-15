<template>
  <header class="header">
    <nav class="navbar">
      <div class="nav-container">
        <!-- Logo and Brand -->
        <div class="nav-brand">
          <router-link to="/" class="brand-link">
            <img 
              v-if="getSetting('logo_url')" 
              :src="getMediaUrl(getSetting('logo_url'))" 
              alt="Logo" 
              class="brand-logo"
              :style="{ height: logoHeaderHeight }"
            />
            <div v-else class="logo-placeholder">🏍️</div>
            <span class="brand-text">{{ getSetting('site_name', 'Powersports Gear & Vehicles') }}</span>
          </router-link>
        </div>

        <!-- Mobile Menu Toggle -->
        <button 
          class="mobile-toggle" 
          @click="toggleMobileMenu"
          :class="{ active: isMobileMenuOpen }"
          aria-label="Toggle navigation menu"
        >
          <span></span>
          <span></span>
          <span></span>
        </button>

        <!-- Navigation Links -->
        <div 
          class="nav-menu" 
          :class="{ active: isMobileMenuOpen }"
          :style="{ '--mobile-menu-top': `${headerHeight}px` }"
        >
          <router-link 
            to="/" 
            class="nav-link" 
            @click="closeMobileMenu"
            exact-active-class="active"
          >
            Home
          </router-link>
          <router-link 
            to="/products" 
            class="nav-link" 
            @click="closeMobileMenu"
            active-class="active"
          >
            Products
          </router-link>
          <router-link 
            to="/about" 
            class="nav-link" 
            @click="closeMobileMenu"
            active-class="active"
          >
            About
          </router-link>
          <router-link 
            to="/contact" 
            class="nav-link" 
            @click="closeMobileMenu"
            active-class="active"
          >
            Contact
          </router-link>
          
          <!-- Authentication Links -->
          <div class="auth-links">
            <!-- Dark Mode Toggle -->
            <button 
              v-if="darkModeEnabled"
              @click="themeStore.toggleDarkMode()" 
              class="dark-mode-toggle"
              :title="themeStore.isDarkMode ? 'Switch to light mode' : 'Switch to dark mode'"
              aria-label="Toggle dark mode"
            >
              <span class="icon">{{ themeStore.isDarkMode ? '☀️' : '🌙' }}</span>
            </button>

            <template v-if="!authStore.isAuthenticated">
              <router-link 
                to="/login" 
                class="auth-link login-link" 
                @click="closeMobileMenu"
                active-class="active"
              >
                Login
              </router-link>
            </template>
            <template v-else>
              <div class="user-welcome">
                Welcome, {{ authStore.user?.firstName }}!
              </div>
              <a 
                v-if="authStore.hasAdminAccess"
                :href="`http://localhost:5174?token=${authStore.token}`"
                target="_blank"
                class="auth-link admin-link"
                @click="closeMobileMenu"
              >
                Admin Dashboard
              </a>
              <button 
                @click="handleLogout"
                class="auth-link logout-btn"
              >
                Logout
              </button>
            </template>
          </div>
        </div>
      </div>
    </nav>
  </header>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, onUnmounted, watch } from 'vue';
import { useRouter } from 'vue-router';
import { useAuthStore } from '@/stores/auth';
import { useThemeStore } from '@/stores/theme';
import { useSettings } from '@/composables/useSettings';
import { getMediaUrl } from '@/utils/api-config';

// Router and stores
const router = useRouter();
const authStore = useAuthStore();
const themeStore = useThemeStore();
const { getSetting } = useSettings();

const registrationEnabled = computed(() => getSetting('allow_user_registration', 'true') !== 'false');
const darkModeEnabled = computed(() => getSetting('theme_dark_mode_enabled', 'false') === 'true');

// Logo size from settings (default: 40px)
const logoHeaderHeight = computed(() => {
  const height = getSetting('logo_header_height', '40');
  return `${height}px`;
});

// Header height tracking for mobile menu positioning
const headerHeight = ref(72);

const updateHeaderHeight = () => {
  const headerEl = document.querySelector('.header') as HTMLElement;
  if (headerEl) {
    headerHeight.value = headerEl.offsetHeight;
  }
};

// Watch for logo size changes and update header height
watch(logoHeaderHeight, () => {
  // Use setTimeout to allow DOM to update first
  setTimeout(updateHeaderHeight, 0);
});

onMounted(() => {
  updateHeaderHeight();
  window.addEventListener('resize', updateHeaderHeight);
});

onUnmounted(() => {
  window.removeEventListener('resize', updateHeaderHeight);
});

// Mobile menu state
const isMobileMenuOpen = ref(false);

const toggleMobileMenu = () => {
  isMobileMenuOpen.value = !isMobileMenuOpen.value;
};

const closeMobileMenu = () => {
  isMobileMenuOpen.value = false;
};

const handleLogout = async () => {
  const redirectPath = await authStore.logout();
  closeMobileMenu();
  
  // If maintenance mode is active, redirect to maintenance page
  // Otherwise, redirect to home
  router.push(redirectPath || '/');
};
</script>

<style scoped>
.header {
  background-color: var(--header-bg, #1a1a1a);
  box-shadow: var(--header-shadow, 0 2px 4px rgba(0, 0, 0, 0.1));
  position: sticky;
  top: 0;
  z-index: 1000;
  margin: 0;
  padding: 1rem 0;
  display: flex;
  align-items: center;
}

.navbar {
  width: 100%;
}

.nav-container {
  max-width: 1200px;
  margin: 0 auto;
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 0 1rem;
  gap: 2rem;
}

.nav-brand {
  flex-shrink: 0;
}

.brand-link {
  display: flex;
  align-items: center;
  text-decoration: none;
  color: var(--header-text, white);
  font-weight: bold;
}

.brand-logo {
  width: auto;
  margin-right: 0.5rem;
  object-fit: contain;
}

.logo-placeholder {
  font-size: 2rem;
  margin-right: 0.5rem;
  color: var(--header-text, white);
}

.brand-text {
  font-size: 1.5rem;
  color: var(--header-text, white);
  font-weight: 700;
}

.mobile-toggle {
  display: none;
  flex-direction: column;
  background: none;
  border: none;
  cursor: pointer;
  padding: 0.5rem;
}

.mobile-toggle span {
  width: 25px;
  height: 3px;
  background-color: var(--header-text, white);
  margin: 3px 0;
  transition: var(--transition-duration, 0.3s) var(--transition-timing, ease);
}

.mobile-toggle.active span:nth-child(1) {
  transform: rotate(-45deg) translate(-5px, 6px);
}

.mobile-toggle.active span:nth-child(2) {
  opacity: 0;
}

.mobile-toggle.active span:nth-child(3) {
  transform: rotate(45deg) translate(-5px, -6px);
}

.nav-menu {
  display: flex;
  gap: 2rem;
  align-items: center;
}

.nav-link {
  color: var(--header-text, white);
  text-decoration: none;
  font-weight: 500;
  padding: 0.5rem 1rem;
  border-radius: var(--button-radius, 4px);
  transition: all var(--transition-duration, 0.3s) var(--transition-timing, ease);
  position: relative;
}

.nav-link:hover {
  color: var(--header-text, white);
  background-color: var(--color-bg-muted, rgba(255, 107, 53, 0.1));
  transform: translateY(calc(var(--hover-lift-amount, 4px) * -0.1));
  opacity: 0.9;
}

.nav-link.active {
  color: var(--header-text, white);
  background-color: var(--color-bg-muted, rgba(255, 255, 255, 0.2));
  font-weight: 600;
  position: relative;
}

.nav-link.active::after {
  content: '';
  position: absolute;
  bottom: -2px;
  left: 1rem;
  right: 1rem;
  height: 2px;
  background-color: var(--header-text, white);
  border-radius: 1px;
}

/* Authentication Links */
.auth-links {
  display: flex;
  gap: 1rem;
  align-items: center;
  margin-left: 1rem;
  padding-left: 1rem;
  border-left: 1px solid var(--color-border, rgba(255, 255, 255, 0.2));
}

/* Dark Mode Toggle */
.dark-mode-toggle {
  display: flex;
  align-items: center;
  justify-content: center;
  background: var(--color-bg-muted, rgba(255, 255, 255, 0.1));
  border: 1px solid var(--color-border, rgba(255, 255, 255, 0.2));
  border-radius: 50%;
  width: 40px;
  height: 40px;
  cursor: pointer;
  transition: all var(--transition-duration, 0.3s) var(--transition-timing, ease);
  padding: 0;
}

.dark-mode-toggle:hover {
  background: var(--color-bg-secondary, rgba(255, 255, 255, 0.2));
  transform: rotate(20deg) scale(1.1);
  box-shadow: var(--hover-shadow, 0 4px 12px rgba(0, 0, 0, 0.1));
}

.dark-mode-toggle .icon {
  font-size: 1.25rem;
  line-height: 1;
}

.auth-link {
  color: var(--header-text, white);
  text-decoration: none;
  font-weight: var(--button-font-weight, 500);
  padding: var(--button-padding-y, 0.5rem) var(--button-padding-x, 1rem);
  border-radius: var(--button-radius, 6px);
  transition: all var(--transition-duration, 0.3s) var(--transition-timing, ease);
  border: none;
  cursor: pointer;
  background: none;
  font-size: inherit;
}

.login-link {
  background: var(--color-bg-muted, rgba(255, 255, 255, 0.1));
}

.login-link:hover {
  background: var(--color-bg-secondary, rgba(255, 255, 255, 0.2));
  transform: translateY(calc(var(--hover-lift-amount, 4px) * -0.25));
}

.signup-link {
  background: var(--color-primary, #ff6b35);
  color: var(--header-text, white);
}

.signup-link:hover {
  background: var(--gradient);
  transform: translateY(-1px);
  opacity: 0.9;
}

.logout-btn {
  background: var(--color-bg-muted, rgba(255, 107, 53, 0.1));
  color: var(--header-text, white);
}

.logout-btn:hover {
  background: var(--color-bg-secondary, rgba(255, 107, 53, 0.2));
}

.user-welcome {
  color: var(--header-text, white);
  font-size: 0.9rem;
  font-weight: 500;
  opacity: 0.9;
}

.admin-link {
  background: var(--color-secondary, #4f46e5);
  color: var(--header-text, white);
  font-size: 0.9rem;
}

.admin-link:hover {
  background: var(--gradient);
  transform: translateY(-1px);
}

/* Mobile styles */
@media (max-width: 768px) {
  .mobile-toggle {
    display: flex;
  }

  .header {
    padding: 0.75rem 0;
  }

  .nav-menu {
    position: fixed;
    left: -100%;
    top: var(--mobile-menu-top, 70px);
    flex-direction: column;
    background-color: var(--header-bg, #1a1a1a);
    width: 100%;
    text-align: center;
    transition: 0.3s;
    box-shadow: 0 10px 27px rgba(0, 0, 0, 0.05);
    padding: 2rem 0;
  }

  .nav-menu.active {
    left: 0;
  }

  .nav-link {
    padding: 1rem;
    display: block;
    width: 100%;
  }
  
  .auth-links {
    flex-direction: column;
    gap: 1rem;
    margin: 1rem 0 0 0;
    padding: 1rem 0 0 0;
    border-left: none;
    border-top: 1px solid rgba(255, 255, 255, 0.2);
    width: 100%;
  }
  
  .auth-link {
    width: 80%;
    margin: 0 auto;
    text-align: center;
  }
  
  .user-welcome {
    margin-bottom: 0.5rem;
  }

  .brand-text {
    font-size: 1.2rem;
  }
}

/* Tablet styles */
@media (max-width: 1024px) and (min-width: 769px) {
  .nav-container {
    padding: 0 2rem;
  }
  
  .nav-menu {
    gap: 1.5rem;
  }
}
</style>