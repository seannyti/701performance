import { createRouter, createWebHistory, type RouteLocationNormalized } from 'vue-router'
import { useAuthStore } from '@/stores/auth'

const router = createRouter({
  history: createWebHistory(),
  routes: [
    {
      path: '/',
      name: 'dashboard',
      component: () => import('@/views/Dashboard.vue'),
      meta: { requiresAuth: true }
    },
    {
      path: '/catalog',
      name: 'catalog',
      component: () => import('@/views/Catalog.vue'),
      meta: { requiresAuth: true }
    },
    // Redirect old routes to new combined catalog
    {
      path: '/products',
      redirect: '/catalog'
    },
    {
      path: '/categories',
      redirect: '/catalog'
    },
    {
      path: '/users',
      name: 'users',
      component: () => import('@/views/Users.vue'),
      meta: { requiresAuth: true, requiresSuperAdmin: true }
    },
    {
      path: '/settings',
      name: 'settings',
      component: () => import('@/views/Settings.vue'),
      meta: { requiresAuth: true }
    },
    {
      path: '/login',
      name: 'login',
      component: () => import('@/views/Login.vue'),
      meta: { hideForAuth: true }
    },
    {
      path: '/unauthorized',
      name: 'unauthorized',
      component: () => import('@/views/Unauthorized.vue')
    }
  ]
})

// Navigation guards
router.beforeEach(async (to: RouteLocationNormalized, _from: RouteLocationNormalized, next: Function) => {
  const authStore = useAuthStore()
  
  // Initialize auth state if not done yet
  if (!authStore.isAuthenticated && !to.query.token) {
    await authStore.initializeAuth()
  }

  // Handle routes that require authentication
  if (to.meta.requiresAuth) {
    if (!authStore.isAuthenticated) {
      next('/login')
      return
    }

    // Check for super admin requirement
    if (to.meta.requiresSuperAdmin && !authStore.isSuperAdmin) {
      next('/unauthorized')
      return
    }

    // Check for general admin access
    if (!authStore.hasAdminAccess) {
      next('/unauthorized')
      return
    }
  }

  // Hide certain pages if already authenticated
  if (to.meta.hideForAuth && authStore.isAuthenticated) {
    next('/')
    return
  }

  next()
})

export default router