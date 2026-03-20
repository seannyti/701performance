import { createRouter, createWebHistory } from 'vue-router'
import type { RouteRecordRaw } from 'vue-router'

const routes: RouteRecordRaw[] = [
  {
    path: '/login',
    name: 'login',
    component: () => import('@/pages/LoginView.vue'),
  },
  {
    path: '/',
    redirect: '/dashboard',
  },
  {
    path: '/dashboard',
    name: 'dashboard',
    component: () => import('@/pages/DashboardView.vue'),
    meta: { requiresAuth: true },
  },
  {
    path: '/catalog/categories',
    name: 'categories',
    component: () => import('@/pages/CategoriesView.vue'),
    meta: { requiresAuth: true },
  },
  {
    path: '/catalog/vehicles',
    name: 'vehicles',
    component: () => import('@/pages/VehiclesView.vue'),
    meta: { requiresAuth: true },
  },
  {
    path: '/catalog/vehicles/new',
    name: 'vehicle-new',
    component: () => import('@/pages/VehicleEditView.vue'),
    meta: { requiresAuth: true },
  },
  {
    path: '/catalog/vehicles/:id/edit',
    name: 'vehicle-edit',
    component: () => import('@/pages/VehicleEditView.vue'),
    meta: { requiresAuth: true },
  },
  {
    path: '/inquiries',
    name: 'inquiries',
    component: () => import('@/pages/InquiriesView.vue'),
    meta: { requiresAuth: true },
  },
  {
    path: '/sales/orders',
    name: 'orders',
    component: () => import('@/pages/OrdersView.vue'),
    meta: { requiresAuth: true },
  },
  {
    path: '/sales/orders/new',
    name: 'order-new',
    component: () => import('@/pages/OrderEditView.vue'),
    meta: { requiresAuth: true },
  },
  {
    path: '/sales/orders/:id/edit',
    name: 'order-edit',
    component: () => import('@/pages/OrderEditView.vue'),
    meta: { requiresAuth: true },
  },
  {
    path: '/sales/finance',
    name: 'finance',
    component: () => import('@/pages/FinanceView.vue'),
    meta: { requiresAuth: true },
  },
  {
    path: '/calendar',
    name: 'calendar',
    component: () => import('@/pages/CalendarView.vue'),
    meta: { requiresAuth: true },
  },
  {
    path: '/chat',
    name: 'chat',
    component: () => import('@/pages/ChatView.vue'),
    meta: { requiresAuth: true },
  },
  {
    path: '/users',
    name: 'users',
    component: () => import('@/pages/UsersView.vue'),
    meta: { requiresAuth: true },
  },
  {
    path: '/hero',
    name: 'hero',
    component: () => import('@/pages/HeroView.vue'),
    meta: { requiresAuth: true },
  },
  {
    path: '/settings',
    name: 'settings',
    component: () => import('@/pages/SettingsView.vue'),
    meta: { requiresAuth: true },
  },
]

const router = createRouter({
  history: createWebHistory('/admin/'),
  routes,
})

router.beforeEach((to) => {
  const token = localStorage.getItem('mpp_token')
  if (to.meta.requiresAuth && !token) {
    return { name: 'login' }
  }
  if (to.name === 'login' && token) {
    return { name: 'dashboard' }
  }
})

export default router
