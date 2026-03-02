import { createRouter, createWebHistory } from 'vue-router'
import type { RouteRecordRaw } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import { logDebug, logWarn } from '@/services/logger'

// Extend route meta properties
declare module 'vue-router' {
  interface RouteMeta {
    title?: string
    requiresAuth?: boolean
    allowDuringMaintenance?: boolean
    guest?: boolean
  }
}

// Eager-load only the Home view for fast initial load
// All other views are lazy-loaded on demand for better performance
import Home from '@/views/Home.vue'

const routes: Array<RouteRecordRaw> = [
  {
    path: '/',
    name: 'Home',
    component: Home,
    meta: {
      title: 'Home - Powersports Gear & Vehicles'
    }
  },
  {
    path: '/about',
    name: 'About',
    component: () => import('@/views/About.vue'),
    meta: {
      title: 'About Us - Powersports Gear & Vehicles'
    }
  },
  {
    path: '/contact',
    name: 'Contact',
    component: () => import('@/views/Contact.vue'),
    meta: {
      title: 'Contact Us - Powersports Gear & Vehicles'
    }
  },
  {
    path: '/products',
    name: 'Products',
    component: () => import('@/views/Products.vue'),
    meta: {
      title: 'Products - Powersports Gear & Vehicles'
    }
  },
  {
    path: '/login',
    name: 'Login',
    component: () => import('@/views/Login.vue'),
    meta: {
      title: 'Login - Powersports Gear & Vehicles',
      guest: true,
      allowDuringMaintenance: true
    }
  },
  {
    path: '/signup',
    name: 'SignUp',
    component: () => import('@/views/SignUp.vue'),
    meta: {
      title: 'Sign Up - Powersports Gear & Vehicles',
      guest: true,
      allowDuringMaintenance: true
    }
  },
  {
    path: '/verify-email',
    name: 'VerifyEmail',
    component: () => import('@/views/VerifyEmail.vue'),
    meta: {
      title: 'Verify Email - Powersports Gear & Vehicles',
      allowDuringMaintenance: true
    }
  },
  {
    path: '/faq',
    name: 'FAQ',
    component: () => import('@/views/FAQ.vue'),
    meta: {
      title: 'FAQ - Powersports Gear & Vehicles'
    }
  },
  {
    path: '/shipping-returns',
    name: 'ShippingReturns',
    component: () => import('@/views/ShippingReturns.vue'),
    meta: {
      title: 'Shipping & Returns - Powersports Gear & Vehicles'
    }
  },
  {
    path: '/privacy-policy',
    name: 'PrivacyPolicy',
    component: () => import('@/views/PrivacyPolicy.vue'),
    meta: {
      title: 'Privacy Policy - Powersports Gear & Vehicles'
    }
  },
  {
    path: '/terms-of-service',
    name: 'TermsOfService',
    component: () => import('@/views/TermsOfService.vue'),
    meta: {
      title: 'Terms of Service - Powersports Gear & Vehicles'
    }
  },
  {
    path: '/maintenance',
    name: 'Maintenance',
    component: () => import('@/views/Maintenance.vue'),
    meta: {
      title: 'Site Under Maintenance',
      allowDuringMaintenance: true
    }
  },
  // Catch-all route for 404
  {
    path: '/:pathMatch(.*)*',
    name: 'NotFound',
    redirect: '/'
  }
]

const router = createRouter({
  history: createWebHistory(),
  routes,
  scrollBehavior(_to, _from, savedPosition) {
    if (savedPosition) {
      return savedPosition;
    } else {
      return { top: 0 };
    }
  }
});

// Global navigation guard for maintenance mode and route protection
router.beforeEach(async (to, from, next) => {
  // Set page title
  if (to.meta.title) {
    document.title = to.meta.title as string;
  }
  
  const authStore = useAuthStore()
  
  // Check if route requires authentication
  if (to.meta.requiresAuth && !authStore.isAuthenticated) {
    logDebug('Route requires authentication, redirecting to login', { route: to.path });
    next({ name: 'Login', query: { redirect: to.fullPath } })
    return
  }
  
  // Check maintenance mode
  try {
    const API_URL = import.meta.env.VITE_API_URL || 'http://localhost:5226'
    const response = await fetch(`${API_URL}/api/v1/settings`, {
      // Add a short timeout to prevent hanging
      signal: AbortSignal.timeout(3000)
    })
    
    if (response.ok) {
      const settings = await response.json()
      const maintenanceSetting = settings.find((s: any) => s.key === 'enable_maintenance_mode')
      const isMaintenanceMode = maintenanceSetting?.value === 'true'
      
      if (isMaintenanceMode) {
        logDebug('Maintenance mode is active');
        
        // Allow access to maintenance page, login, and signup
        if (to.meta.allowDuringMaintenance) {
          next()
          return
        }
        
        // Check if user is admin (has admin access)
        if (authStore.hasAdminAccess) {
          logDebug('Admin user, bypassing maintenance mode');
          next()
          return
        }
        
        // Redirect non-admins to maintenance page
        logDebug('Non-admin user, redirecting to maintenance');
        if (to.path !== '/maintenance') {
          next('/maintenance')
          return
        }
      } else if (from.path === '/maintenance' && to.path === '/maintenance') {
        // If maintenance mode is off and user is on maintenance page, redirect to home
        logDebug('Maintenance mode disabled, redirecting to home');
        next('/')
        return
      }
    }
  } catch (err) {
    // If settings check fails (API down, timeout, etc.), allow navigation
    // This prevents the site from being completely unusable if backend is down
    logWarn('Could not check maintenance mode', err);
  }
  
  next();
});

export default router;