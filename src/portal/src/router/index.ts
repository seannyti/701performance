import { createRouter, createWebHistory } from 'vue-router'
import { usePortalAuthStore } from '../stores/auth'

declare module 'vue-router' {
  interface RouteMeta {
    requiresAuth?: boolean
    title?: string
    layout?: string
  }
}

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  scrollBehavior: () => ({ top: 0 }),
  routes: [
    {
      path: '/login',
      name: 'login',
      component: () => import('../views/auth/LoginView.vue'),
      meta: { layout: 'auth' }
    },
    {
      path: '/',
      component: () => import('../layouts/PortalLayout.vue'),
      meta: { requiresAuth: true },
      children: [
        // Redirect root to inventory
        { path: '', redirect: { name: 'inventory' } },

        // Inventory
        { path: 'inventory', name: 'inventory', component: () => import('../views/inventory/InventoryListView.vue'), meta: { title: 'Inventory' } },
        { path: 'inventory/new', name: 'inventory-new', component: () => import('../views/inventory/InventoryFormView.vue'), meta: { title: 'Add Vehicle' } },
        { path: 'inventory/:id/edit', name: 'inventory-edit', component: () => import('../views/inventory/InventoryFormView.vue'), meta: { title: 'Edit Vehicle' } },

        // Settings
        { path: 'settings', name: 'settings', component: () => import('../views/settings/SiteSettingsView.vue'), meta: { title: 'Site Settings' } },
        { path: 'settings/hero', name: 'settings-hero', component: () => import('../views/settings/HeroSettingsView.vue'), meta: { title: 'Hero Section' } },
        { path: 'settings/finance', name: 'settings-finance', component: () => import('../views/settings/FinanceSettingsView.vue'), meta: { title: 'Financing' } },
        { path: 'settings/contact', name: 'settings-contact', component: () => import('../views/settings/ContactSettingsView.vue'), meta: { title: 'Contact & Hours' } },
        { path: 'settings/seo', name: 'settings-seo', component: () => import('../views/settings/SeoSettingsView.vue'), meta: { title: 'SEO' } },
        { path: 'settings/theme', name: 'settings-theme', component: () => import('../views/settings/ThemeSettingsView.vue'), meta: { title: 'Theme & Branding' } },
        { path: 'settings/notifications', name: 'settings-notifications', component: () => import('../views/settings/NotificationSettingsView.vue'), meta: { title: 'Email & Notifications' } },
        { path: 'settings/inventory-categories', name: 'settings-inventory-categories', component: () => import('../views/settings/InventoryCategoriesView.vue'), meta: { title: 'Catalog Categories' } },
        { path: 'settings/tos', name: 'settings-tos', component: () => import('../views/settings/TosSettingsView.vue'), meta: { title: 'Terms of Service' } },
        { path: 'settings/privacy', name: 'settings-privacy', component: () => import('../views/settings/PrivacyPolicySettingsView.vue'), meta: { title: 'Privacy Policy' } },

        // Security
        { path: 'security/mfa', name: 'mfa-setup', component: () => import('../views/auth/MfaSetupView.vue') },
      ]
    },
    { path: '/:pathMatch(.*)*', redirect: { name: 'inventory' } }
  ]
})

router.beforeEach(async (to) => {
  const auth = usePortalAuthStore()

  if (to.meta.requiresAuth) {
    if (!auth.isLoggedIn) {
      return { name: 'login', query: { redirect: to.fullPath } }
    }
  }

  if (to.name === 'login' && auth.isLoggedIn) {
    return { name: 'inventory' }
  }
})

export default router
