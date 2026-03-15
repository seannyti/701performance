<template>
  <AdminLayout>
    <div class="dashboard">
      <!-- Header -->
      <div class="dashboard-header">
        <div>
          <h1 class="dashboard-title">Dashboard</h1>
          <p class="dashboard-subtitle">
            Welcome back, {{ authStore.user?.firstName ?? 'Admin' }} —
            <span class="text-muted" style="font-size: 0.85rem;">
              {{ formattedDate }}
            </span>
          </p>
        </div>
        <button class="btn btn-secondary btn-sm" :disabled="isLoading" @click="loadDashboardStats">
          <span v-if="isLoading">⟳ Refreshing…</span>
          <span v-else>⟳ Refresh</span>
        </button>
      </div>

      <!-- Loading skeleton -->
      <div v-if="isLoading && !stats" class="stats-grid">
        <div v-for="i in 4" :key="i" class="stat-card skeleton" />
      </div>

      <!-- Error banner -->
      <div v-if="error" class="error-banner">
        <span>⚠ {{ error }}</span>
        <button class="btn btn-sm" style="margin-left: auto;" @click="loadDashboardStats">Retry</button>
      </div>

      <template v-if="stats">
        <!-- Primary stats -->
        <div class="stats-grid">
          <div class="stat-card">
            <div class="stat-icon" style="background: #ede9fe; color: #7c3aed;">📦</div>
            <div class="stat-info">
              <span class="stat-value">{{ stats.totalProducts }}</span>
              <span class="stat-label">Active Products</span>
              <span class="stat-sub">{{ stats.featuredProducts }} featured · {{ stats.inactiveProducts }} inactive</span>
            </div>
          </div>

          <div class="stat-card">
            <div class="stat-icon" style="background: #d1fae5; color: #059669;">👤</div>
            <div class="stat-info">
              <span class="stat-value">{{ stats.totalUsers }}</span>
              <span class="stat-label">Registered Users</span>
              <span class="stat-sub">{{ stats.recentRegistrations }} joined in last 7 days</span>
            </div>
          </div>

          <div class="stat-card" v-if="authStore.isSuperAdmin">
            <div class="stat-icon" style="background: #fee2e2; color: #dc2626;">🛡</div>
            <div class="stat-info">
              <span class="stat-value">{{ stats.totalAdmins }}</span>
              <span class="stat-label">Admin Accounts</span>
              <span class="stat-sub">Admins &amp; super admins</span>
            </div>
          </div>

          <div class="stat-card">
            <div class="stat-icon" style="background: #dbeafe; color: #2563eb;">🗂</div>
            <div class="stat-info">
              <span class="stat-value">{{ stats.totalCategories }}</span>
              <span class="stat-label">Active Categories</span>
              <span class="stat-sub">Product groupings</span>
            </div>
          </div>
        </div>

        <!-- Two-column lower section -->
        <div class="dashboard-lower">
          <!-- Recent registrations -->
          <div class="card">
            <div class="card-header d-flex align-items-center justify-content-between">
              <h2 class="card-title">Recent Registrations</h2>
              <router-link v-if="authStore.isSuperAdmin" to="/users" class="btn btn-sm btn-secondary">
                View all
              </router-link>
            </div>
            <div class="card-body" style="padding: 0;">
              <table class="table" v-if="stats.recentUsers.length > 0">
                <thead>
                  <tr>
                    <th>Name</th>
                    <th>Email</th>
                    <th>Role</th>
                    <th>Joined</th>
                  </tr>
                </thead>
                <tbody>
                  <tr v-for="u in stats.recentUsers" :key="u.id">
                    <td>{{ u.firstName }} {{ u.lastName }}</td>
                    <td style="color: #64748b; font-size: 0.875rem;">{{ u.email }}</td>
                    <td>
                      <span class="role-badge" :class="roleBadgeClass(u.roleName)">{{ u.roleName }}</span>
                    </td>
                    <td style="color: #64748b; font-size: 0.875rem;">{{ timeAgo(u.createdAt) }}</td>
                  </tr>
                </tbody>
              </table>
              <div v-else style="padding: 2rem; text-align: center; color: #94a3b8;">
                No users registered yet.
              </div>
            </div>
          </div>

          <!-- Quick actions -->
          <div class="card">
            <div class="card-header">
              <h2 class="card-title">Quick Actions</h2>
            </div>
            <div class="card-body" style="display: flex; flex-direction: column; gap: 0.75rem;">
              <router-link to="/catalog" class="action-tile">
                <span class="action-icon" style="background: #ede9fe;">📦</span>
                <div>
                  <div class="action-title">Manage Catalog</div>
                  <div class="action-desc">Add, edit or remove products and categories</div>
                </div>
              </router-link>

              <router-link to="/media" class="action-tile">
                <span class="action-icon" style="background: #fce7f3;">🖼️</span>
                <div>
                  <div class="action-title">Media Library</div>
                  <div class="action-desc">Upload and manage product images</div>
                </div>
              </router-link>

              <router-link to="/settings" class="action-tile">
                <span class="action-icon" style="background: #f0fdf4;">⚙️</span>
                <div>
                  <div class="action-title">Site Settings</div>
                  <div class="action-desc">Update site content and configuration</div>
                </div>
              </router-link>

              <router-link v-if="authStore.isSuperAdmin" to="/users" class="action-tile">
                <span class="action-icon" style="background: #fef3c7;">👥</span>
                <div>
                  <div class="action-title">Manage Users</div>
                  <div class="action-desc">View accounts, update roles, and moderate users</div>
                </div>
              </router-link>

              <button @click="runHealthCheck" class="action-tile" :disabled="isCheckingHealth" style="cursor: pointer;">
                <span class="action-icon" style="background: #dbeafe;">🏥</span>
                <div>
                  <div class="action-title">API Health Check</div>
                  <div class="action-desc">
                    <span v-if="isCheckingHealth">Checking endpoints...</span>
                    <span v-else-if="healthStatus">{{ healthSummary }}</span>
                    <span v-else>Test all API endpoints</span>
                  </div>
                </div>
              </button>
            </div>
          </div>
        </div>

        <!-- Health Check Results -->
        <div v-if="healthStatus" class="card">
          <div class="card-header d-flex align-items-center justify-content-between">
            <h2 class="card-title">API Health Check Results</h2>
            <button class="btn btn-sm btn-secondary" @click="runHealthCheck" :disabled="isCheckingHealth">
              <span v-if="isCheckingHealth">⟳ Checking...</span>
              <span v-else>⟳ Recheck</span>
            </button>
          </div>
          <div class="card-body" style="padding: 0;">
            <table class="table">
              <thead>
                <tr>
                  <th>Endpoint</th>
                  <th>Status</th>
                  <th>Response Time</th>
                  <th>Details</th>
                </tr>
              </thead>
              <tbody>
                <tr v-for="(result, index) in healthStatus.endpoints" :key="index" :class="{ 'health-error': !result.healthy }">
                  <td style="font-family: monospace; font-size: 0.85rem;">{{ result.endpoint }}</td>
                  <td>
                    <span class="health-badge" :class="result.healthy ? 'health-badge--ok' : 'health-badge--error'">
                      {{ result.healthy ? '✓ OK' : '✗ Error' }}
                    </span>
                  </td>
                  <td style="color: #64748b; font-size: 0.875rem;">
                    <span v-if="result.responseTime">{{ result.responseTime }}ms</span>
                    <span v-else>—</span>
                  </td>
                  <td style="color: #64748b; font-size: 0.875rem;">
                    {{ result.message || result.error || '—' }}
                  </td>
                </tr>
              </tbody>
            </table>
            <div class="health-summary">
              <div class="health-summary-item">
                <span class="health-label">Total Endpoints:</span>
                <span class="health-value">{{ healthStatus.total }}</span>
              </div>
              <div class="health-summary-item">
                <span class="health-label">Healthy:</span>
                <span class="health-value" style="color: #059669;">{{ healthStatus.healthy }}</span>
              </div>
              <div class="health-summary-item">
                <span class="health-label">Failed:</span>
                <span class="health-value" style="color: #dc2626;">{{ healthStatus.unhealthy }}</span>
              </div>
              <div class="health-summary-item">
                <span class="health-label">Checked:</span>
                <span class="health-value">{{ new Date(healthStatus.checkedAt).toLocaleString() }}</span>
              </div>
            </div>
          </div>
        </div>

        <!-- Footer note -->
        <p class="dashboard-refreshed">
          Stats refreshed {{ timeAgo(stats.generatedAt) }}
        </p>
      </template>
    </div>
  </AdminLayout>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import AdminLayout from '@/components/AdminLayout.vue'
import { useAuthStore } from '@/stores/auth'
import type { DashboardStats } from '@/types'
import { logError, logDebug } from '@/services/logger'
import { DASHBOARD_STATS_TIMEOUT_MS } from '@/constants'

const API_URL = `${import.meta.env.VITE_API_URL ?? 'http://localhost:5226'}/api/v1`

const authStore = useAuthStore()

const stats = ref<DashboardStats | null>(null)
const isLoading = ref(false)
const error = ref<string | null>(null)

// Health check state
const healthStatus = ref<any | null>(null)
const isCheckingHealth = ref(false)

const healthSummary = computed(() => {
  if (!healthStatus.value) return ''
  const { healthy, unhealthy } = healthStatus.value
  if (unhealthy === 0) return `✓ All ${healthy} endpoints healthy`
  return `⚠ ${unhealthy} endpoint${unhealthy > 1 ? 's' : ''} failing`
})

const formattedDate = computed(() => {
  return new Intl.DateTimeFormat('en-US', {
    weekday: 'long',
    month: 'long',
    day: 'numeric'
  }).format(new Date())
})

const loadDashboardStats = async () => {
  isLoading.value = true
  error.value = null

  try {
    logDebug('Loading dashboard stats', { url: `${API_URL}/admin/dashboard/stats` })

    const response = await fetch(`${API_URL}/admin/dashboard/stats`, {
      headers: { Authorization: `Bearer ${authStore.token}` }
    })

    if (!response.ok) {
      throw new Error(`Server responded with ${response.status}`)
    }

    const body = await response.json()
    stats.value = body.data
    logDebug('Dashboard stats loaded', { stats: stats.value })
  } catch (err) {
    logError('Failed to load dashboard stats', err)
    error.value = 'Could not load dashboard statistics. Is the API running?'
  } finally {
    isLoading.value = false
  }
}

const timeAgo = (dateStr: string): string => {
  const diff = Date.now() - new Date(dateStr).getTime()
  const minutes = Math.floor(diff / 60_000)
  if (minutes < 1) return 'just now'
  if (minutes < 60) return `${minutes}m ago`
  const hours = Math.floor(minutes / 60)
  if (hours < 24) return `${hours}h ago`
  const days = Math.floor(hours / 24)
  return `${days}d ago`
}

const roleBadgeClass = (roleName: string) => {
  if (roleName === 'SuperAdmin') return 'role-badge--superadmin'
  if (roleName === 'Admin') return 'role-badge--admin'
  return 'role-badge--user'
}

const runHealthCheck = async () => {
  isCheckingHealth.value = true
  
  const endpoints = [
    { path: '/health', name: 'API Health', requiresAuth: false },
    { path: '/api/v1/products', name: 'Products List', requiresAuth: false },
    { path: '/api/v1/products/featured', name: 'Featured Products', requiresAuth: false },
    { path: '/api/v1/categories', name: 'Categories', requiresAuth: false },
    { path: '/api/v1/settings', name: 'Public Settings', requiresAuth: false },
    { path: '/api/v1/auth/me', name: 'Auth - Current User', requiresAuth: true },
    { path: '/api/v1/admin/dashboard/stats', name: 'Dashboard Stats', requiresAuth: true },
    { path: '/api/v1/admin/users', name: 'Admin - Users', requiresAuth: true },
    { path: '/api/v1/admin/settings', name: 'Admin - Settings', requiresAuth: true },
    { path: '/api/v1/admin/media', name: 'Media Library', requiresAuth: true },
  ]
  
  const results = []
  const baseUrl = import.meta.env.VITE_API_URL ?? 'http://localhost:5226'
  
  for (const endpoint of endpoints) {
    const startTime = performance.now()
    try {
      const headers: Record<string, string> = {}
      if (endpoint.requiresAuth && authStore.token) {
        headers['Authorization'] = `Bearer ${authStore.token}`
      }
      
      const response = await fetch(`${baseUrl}${endpoint.path}`, {
        method: 'GET',
        headers,
        signal: AbortSignal.timeout(DASHBOARD_STATS_TIMEOUT_MS)
      })
      
      const endTime = performance.now()
      const responseTime = Math.round(endTime - startTime)
      
      if (response.ok) {
        results.push({
          endpoint: endpoint.name,
          path: endpoint.path,
          healthy: true,
          responseTime,
          message: `HTTP ${response.status}`
        })
      } else {
        results.push({
          endpoint: endpoint.name,
          path: endpoint.path,
          healthy: false,
          responseTime,
          error: `HTTP ${response.status} ${response.statusText}`
        })
      }
    } catch (err: any) {
      const endTime = performance.now()
      const responseTime = Math.round(endTime - startTime)
      
      results.push({
        endpoint: endpoint.name,
        path: endpoint.path,
        healthy: false,
        responseTime,
        error: err.name === 'TimeoutError' ? 'Request timeout' : err.message || 'Network error'
      })
    }
  }
  
  const healthy = results.filter(r => r.healthy).length
  const unhealthy = results.length - healthy
  
  healthStatus.value = {
    endpoints: results,
    total: results.length,
    healthy,
    unhealthy,
    checkedAt: new Date().toISOString()
  }
  
  isCheckingHealth.value = false
}

onMounted(() => {
  loadDashboardStats()
})
</script>

<style scoped>
.dashboard {
  display: flex;
  flex-direction: column;
  gap: 1.5rem;
}

.dashboard-header {
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  gap: 1rem;
}

.dashboard-title {
  font-size: 1.75rem;
  font-weight: 800;
  color: #111827;
  margin-bottom: 0.25rem;
}

.dashboard-subtitle {
  color: #64748b;
  font-size: 0.95rem;
}

/* Stats grid */
.stats-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(220px, 1fr));
  gap: 1rem;
}

.stat-card {
  background: white;
  border-radius: 0.75rem;
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.08);
  padding: 1.25rem 1.5rem;
  display: flex;
  align-items: center;
  gap: 1rem;
  transition: box-shadow 0.2s;
}

.stat-card:hover {
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1);
}

.stat-card.skeleton {
  min-height: 90px;
  background: linear-gradient(90deg, #f1f5f9 25%, #e2e8f0 50%, #f1f5f9 75%);
  background-size: 200% 100%;
  animation: shimmer 1.4s infinite;
}

@keyframes shimmer {
  0% { background-position: 200% 0; }
  100% { background-position: -200% 0; }
}

.stat-icon {
  width: 3rem;
  height: 3rem;
  border-radius: 0.75rem;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 1.4rem;
  flex-shrink: 0;
}

.stat-info {
  display: flex;
  flex-direction: column;
  gap: 0.1rem;
}

.stat-value {
  font-size: 1.75rem;
  font-weight: 800;
  color: #111827;
  line-height: 1;
}

.stat-label {
  font-size: 0.875rem;
  font-weight: 600;
  color: #374151;
}

.stat-sub {
  font-size: 0.75rem;
  color: #94a3b8;
}

/* Lower section */
.dashboard-lower {
  display: grid;
  grid-template-columns: 1fr 380px;
  gap: 1.5rem;
  align-items: start;
}

@media (max-width: 900px) {
  .dashboard-lower {
    grid-template-columns: 1fr;
  }
}

/* Role badges */
.role-badge {
  display: inline-block;
  padding: 0.2rem 0.6rem;
  border-radius: 999px;
  font-size: 0.7rem;
  font-weight: 600;
  text-transform: uppercase;
  letter-spacing: 0.04em;
}

.role-badge--user {
  background: #eff6ff;
  color: #2563eb;
}

.role-badge--admin {
  background: #fef3c7;
  color: #d97706;
}

.role-badge--superadmin {
  background: #fee2e2;
  color: #dc2626;
}

/* Action tiles */
.action-tile {
  display: flex;
  align-items: center;
  gap: 1rem;
  padding: 0.875rem 1rem;
  border-radius: 0.625rem;
  background: #f8fafc;
  border: 1px solid #e2e8f0;
  text-decoration: none;
  color: inherit;
  transition: all 0.15s;
  cursor: pointer;
}

.action-tile:hover:not(.action-tile--muted) {
  background: #f1f5f9;
  border-color: #c7d2fe;
  transform: translateX(2px);
}

.action-tile--muted {
  opacity: 0.5;
  cursor: default;
}

.action-icon {
  width: 2.5rem;
  height: 2.5rem;
  border-radius: 0.5rem;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 1.2rem;
  flex-shrink: 0;
}

.action-title {
  font-size: 0.9rem;
  font-weight: 600;
  color: #1e293b;
}

.action-desc {
  font-size: 0.78rem;
  color: #94a3b8;
}

/* Error banner */
.error-banner {
  display: flex;
  align-items: center;
  gap: 1rem;
  padding: 0.875rem 1.25rem;
  background: #fef2f2;
  border: 1px solid #fecaca;
  border-radius: 0.625rem;
  color: #dc2626;
  font-size: 0.9rem;
}

.dashboard-refreshed {
  font-size: 0.78rem;
  color: #cbd5e1;
  text-align: right;
}

/* Health check styles */
.health-badge {
  display: inline-block;
  padding: 0.2rem 0.6rem;
  border-radius: 999px;
  font-size: 0.7rem;
  font-weight: 600;
  text-transform: uppercase;
  letter-spacing: 0.04em;
}

.health-badge--ok {
  background: #d1fae5;
  color: #059669;
}

.health-badge--error {
  background: #fee2e2;
  color: #dc2626;
}

.health-error {
  background: #fef2f2;
}

.health-summary {
  display: flex;
  gap: 2rem;
  padding: 1.25rem 1.5rem;
  background: #f8fafc;
  border-top: 1px solid #e2e8f0;
}

.health-summary-item {
  display: flex;
  flex-direction: column;
  gap: 0.25rem;
}

.health-label {
  font-size: 0.75rem;
  color: #94a3b8;
  text-transform: uppercase;
  letter-spacing: 0.05em;
  font-weight: 600;
}

.health-value {
  font-size: 1.125rem;
  color: #1e293b;
  font-weight: 700;
}

@media (max-width: 768px) {
  .health-summary {
    flex-wrap: wrap;
    gap: 1rem;
  }
  
  .health-summary-item {
    flex: 1 1 40%;
  }
}
</style>
