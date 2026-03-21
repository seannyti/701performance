import { createRouter, createWebHistory } from 'vue-router'
import type { RouteRecordRaw } from 'vue-router'

const routes: RouteRecordRaw[] = [
  {
    path: '/',
    name: 'home',
    component: () => import('@/pages/HomeView.vue'),
  },
  {
    path: '/inventory',
    name: 'inventory',
    component: () => import('@/pages/InventoryView.vue'),
  },
  {
    path: '/inventory/:id',
    name: 'vehicle',
    component: () => import('@/pages/VehicleDetailView.vue'),
  },
  {
    path: '/about',
    name: 'about',
    component: () => import('@/pages/AboutView.vue'),
  },
  {
    path: '/contact',
    name: 'contact',
    component: () => import('@/pages/ContactView.vue'),
  },
  {
    path: '/login',
    name: 'login',
    component: () => import('@/pages/LoginView.vue'),
  },
  {
    path: '/signup',
    name: 'signup',
    component: () => import('@/pages/SignUpView.vue'),
  },
  {
    path: '/verify-email',
    name: 'verify-email',
    component: () => import('@/pages/VerifyEmailView.vue'),
  },
  {
    path: '/profile',
    name: 'profile',
    component: () => import('@/pages/ProfileView.vue'),
  },
  {
    path: '/financing',
    name: 'financing',
    component: () => import('@/pages/FinanceView.vue'),
  },
  {
    path: '/faq',
    name: 'faq',
    component: () => import('@/pages/FaqView.vue'),
  },
  {
    path: '/shipping-returns',
    name: 'shipping-returns',
    component: () => import('@/pages/ShippingReturnsView.vue'),
  },
  {
    path: '/privacy-policy',
    name: 'privacy-policy',
    component: () => import('@/pages/PrivacyPolicyView.vue'),
  },
  {
    path: '/terms-of-service',
    name: 'terms-of-service',
    component: () => import('@/pages/TermsView.vue'),
  },
  {
    path: '/:pathMatch(.*)*',
    name: 'not-found',
    component: () => import('@/pages/NotFoundView.vue'),
  },
]

export default createRouter({
  history: createWebHistory(),
  routes,
  scrollBehavior: () => ({ top: 0 }),
})
