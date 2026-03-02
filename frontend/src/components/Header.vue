<template>
  <header class="header">
    <nav class="navbar">
      <div class="nav-container">
        <!-- Logo and Brand -->
        <div class="nav-brand">
          <router-link to="/" class="brand-link">
            <div class="logo-placeholder">🏍️</div>
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
        <div class="nav-menu" :class="{ active: isMobileMenuOpen }">
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
            <template v-if="!authStore.isAuthenticated">
              <router-link 
                to="/login" 
                class="auth-link login-link" 
                @click="closeMobileMenu"
                active-class="active"
              >
                Login
              </router-link>
              <router-link 
                v-if="registrationEnabled"
                to="/signup" 
                class="auth-link signup-link" 
                @click="closeMobileMenu"
                active-class="active"
              >
                Sign Up
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
import { ref, computed } from 'vue';
import { useRouter } from 'vue-router';
import { useAuthStore } from '@/stores/auth';
import { useSettings } from '@/composables/useSettings';

// Router and auth store
const router = useRouter();
const authStore = useAuthStore();
const { getSetting } = useSettings();

const registrationEnabled = computed(() => getSetting('allow_user_registration', 'true') !== 'false');

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
  background-color: #1a1a1a;
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
  position: sticky;
  top: 0;
  z-index: 1000;
}

.navbar {
  padding: 0.5rem 0;
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
  color: white;
  font-weight: bold;
}

.logo-placeholder {
  font-size: 2rem;
  margin-right: 0.5rem;
}

.brand-text {
  font-size: 1.5rem;
  color: #ff6b35;
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
  background-color: white;
  margin: 3px 0;
  transition: 0.3s;
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
  color: white;
  text-decoration: none;
  font-weight: 500;
  padding: 0.5rem 1rem;
  border-radius: 4px;
  transition: all 0.3s ease;
  position: relative;
}

.nav-link:hover {
  color: #ff6b35;
  background-color: rgba(255, 107, 53, 0.1);
}

.nav-link.active {
  color: #ff6b35;
  background-color: rgba(255, 107, 53, 0.1);
}

/* Authentication Links */
.auth-links {
  display: flex;
  gap: 1rem;
  align-items: center;
  margin-left: 1rem;
  padding-left: 1rem;
  border-left: 1px solid rgba(255, 255, 255, 0.2);
}

.auth-link {
  color: white;
  text-decoration: none;
  font-weight: 500;
  padding: 0.5rem 1rem;
  border-radius: 6px;
  transition: all 0.3s ease;
  border: none;
  cursor: pointer;
  background: none;
  font-size: inherit;
}

.login-link {
  background: rgba(255, 255, 255, 0.1);
}

.login-link:hover {
  background: rgba(255, 255, 255, 0.2);
}

.signup-link {
  background: linear-gradient(135deg, #ff6b35, #f7931e);
  color: white;
}

.signup-link:hover {
  background: linear-gradient(135deg, #e55a2b, #e6830e);
  transform: translateY(-1px);
}

.logout-btn {
  background: rgba(255, 107, 53, 0.1);
  color: #ff6b35;
}

.logout-btn:hover {
  background: rgba(255, 107, 53, 0.2);
}

.user-welcome {
  color: #ff6b35;
  font-size: 0.9rem;
  font-weight: 500;
}

.admin-link {
  background: linear-gradient(135deg, #4f46e5, #7c3aed);
  color: white;
  font-size: 0.9rem;
}

.admin-link:hover {
  background: linear-gradient(135deg, #4338ca, #6d28d9);
  transform: translateY(-1px);
}

/* Mobile styles */
@media (max-width: 768px) {
  .mobile-toggle {
    display: flex;
  }

  .nav-menu {
    position: fixed;
    left: -100%;
    top: 70px;
    flex-direction: column;
    background-color: #1a1a1a;
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