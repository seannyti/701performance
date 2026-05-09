import { createRouter, createWebHistory } from 'vue-router'
import { useSettingsStore } from '../stores/settings'

const router = createRouter({
  history: createWebHistory(),
  scrollBehavior: () => ({ top: 0 }),
  routes: [
    // Maintenance — standalone, no layout
    {
      path: '/maintenance',
      name: 'maintenance',
      component: () => import('../views/MaintenanceView.vue'),
    },
    // Public site
    {
      path: '/',
      component: () => import('../layouts/DefaultLayout.vue'),
      children: [
        { path: '', name: 'home', component: () => import('../views/HomeView.vue') },
        { path: 'inventory', name: 'inventory', component: () => import('../views/InventoryView.vue'), meta: { title: 'Inventory' } },
        { path: 'inventory/:id', name: 'vehicle-detail', component: () => import('../views/VehicleDetailView.vue') },
        { path: 'finance', name: 'finance', component: () => import('../views/FinanceView.vue'), meta: { title: 'Financing' } },
        { path: 'about', name: 'about', component: () => import('../views/AboutView.vue'), meta: { title: 'About Us' } },
        { path: 'contact', name: 'contact', component: () => import('../views/ContactView.vue'), meta: { title: 'Contact Us' } },
        { path: 'tos', name: 'tos', component: () => import('../views/TosView.vue'), meta: { title: 'Terms of Service' } },
        { path: 'privacy', name: 'privacy', component: () => import('../views/PrivacyPolicyView.vue'), meta: { title: 'Privacy Policy' } },
      ]
    }
  ]
})

router.beforeEach(async (to) => {
  const settings = useSettingsStore()
  if (!settings.loaded) {
    await settings.fetchSettings()
  }

  const maintenanceOn = settings.get('maintenance_mode') === 'true'

  if (to.name === 'maintenance' && !maintenanceOn) {
    return { name: 'home' }
  }

  if (maintenanceOn && to.name !== 'maintenance') {
    return { name: 'maintenance' }
  }
})

export default router
