<template>
  <AdminLayout>
    <div class="dashboard">

      <!-- ── Header ──────────────────────────────────────── -->
      <div class="dashboard-header">
        <div>
          <h1 class="dashboard-title">Dashboard</h1>
          <p class="dashboard-subtitle">
            Welcome back, <strong>{{ authStore.user?.firstName ?? 'Admin' }}</strong> —
            <span class="text-muted">{{ formattedDate }}</span>
          </p>
        </div>
        <div class="header-actions">
          <!-- Weather widget -->
          <div v-if="weather" class="weather-chip">
            <span class="weather-chip__icon">{{ weatherIcon }}</span>
            <div class="weather-chip__info">
              <span class="weather-chip__temp">{{ weather.tempF }}°F</span>
              <span class="weather-chip__desc">{{ weather.desc }}</span>
            </div>
            <div class="weather-chip__detail">
              <span>💧 {{ weather.humidity }}%</span>
              <span>💨 {{ weather.windMph }} mph</span>
            </div>
          </div>

          <div class="status-pills" v-if="systemStatus.length">
            <span
              v-for="s in systemStatus"
              :key="s.label"
              class="status-pill"
              :class="s.ok ? 'status-pill--ok' : 'status-pill--error'"
              :title="s.ok ? `${s.label}: OK` : `${s.label}: Down`"
            >
              <span class="status-dot"></span>{{ s.label }}
            </span>
          </div>
          <button class="btn btn-secondary btn-sm" :disabled="isLoading" @click="refresh">
            <span v-if="isLoading">⟳ Refreshing…</span>
            <span v-else>⟳ Refresh</span>
          </button>
        </div>
      </div>

      <!-- ── Error banner ────────────────────────────────── -->
      <div v-if="error" class="error-banner">
        <span>⚠ {{ error }}</span>
        <button class="btn btn-sm" style="margin-left:auto;" @click="refresh">Retry</button>
      </div>

      <!-- ── Loading skeleton ────────────────────────────── -->
      <div v-if="isLoading && !stats" class="stats-grid">
        <div v-for="i in 8" :key="i" class="stat-card skeleton" />
      </div>

      <template v-if="stats">
        <!-- ── Stat cards ────────────────────────────────── -->
        <div class="stats-grid">
          <div class="stat-card stat-card--purple">
            <div class="stat-icon" style="background:#ede9fe;color:#7c3aed;">📦</div>
            <div class="stat-info">
              <span class="stat-value">{{ stats.totalProducts }}</span>
              <span class="stat-label">Active Products</span>
              <span class="stat-sub">{{ stats.featuredProducts }} featured · {{ stats.inactiveProducts }} inactive</span>
            </div>
          </div>

          <div class="stat-card stat-card--green">
            <div class="stat-icon" style="background:#d1fae5;color:#059669;">👤</div>
            <div class="stat-info">
              <span class="stat-value">{{ stats.totalUsers }}</span>
              <span class="stat-label">Registered Users</span>
              <span class="stat-sub">{{ stats.recentRegistrations }} joined last 7 days</span>
            </div>
          </div>

          <div class="stat-card stat-card--blue">
            <div class="stat-icon" style="background:#dbeafe;color:#2563eb;">🗂</div>
            <div class="stat-info">
              <span class="stat-value">{{ stats.totalCategories }}</span>
              <span class="stat-label">Active Categories</span>
              <span class="stat-sub">Product groupings</span>
            </div>
          </div>

          <div class="stat-card stat-card--amber">
            <div class="stat-icon" style="background:#fef3c7;color:#d97706;">🛒</div>
            <div class="stat-info">
              <span class="stat-value">{{ stats.totalOrders }}</span>
              <span class="stat-label">Total Orders</span>
              <span class="stat-sub">{{ stats.pendingOrders }} pending · ${{ stats.totalRevenue.toLocaleString('en-US', { minimumFractionDigits: 2 }) }} revenue</span>
            </div>
          </div>

          <div class="stat-card stat-card--teal">
            <div class="stat-icon" style="background:#f0fdf4;color:#16a34a;">📧</div>
            <div class="stat-info">
              <span class="stat-value">{{ stats.newInquiries }}</span>
              <span class="stat-label">New Inquiries</span>
              <span class="stat-sub">Unread contact submissions</span>
            </div>
          </div>

          <div class="stat-card stat-card--indigo">
            <div class="stat-icon" style="background:#e0e7ff;color:#4338ca;">🖼️</div>
            <div class="stat-info">
              <span class="stat-value">{{ mediaCount ?? '—' }}</span>
              <span class="stat-label">Media Files</span>
              <span class="stat-sub">Images &amp; videos in library</span>
            </div>
          </div>

          <div class="stat-card stat-card--rose">
            <div class="stat-icon" style="background:#ffe4e6;color:#e11d48;">💬</div>
            <div class="stat-info">
              <span class="stat-value">{{ activeChatCount ?? '—' }}</span>
              <span class="stat-label">Active Live Chats</span>
              <span class="stat-sub">Open customer sessions</span>
            </div>
          </div>

          <div class="stat-card stat-card--red" v-if="authStore.isSuperAdmin">
            <div class="stat-icon" style="background:#fee2e2;color:#dc2626;">🛡</div>
            <div class="stat-info">
              <span class="stat-value">{{ stats.totalAdmins }}</span>
              <span class="stat-label">Admin Accounts</span>
              <span class="stat-sub">Admins &amp; super admins</span>
            </div>
          </div>

          <div class="stat-card stat-card--cyan">
            <div class="stat-icon" style="background:#cffafe;color:#0891b2;">🌐</div>
            <div class="stat-info">
              <span class="stat-value">{{ visitorCount ?? '—' }}</span>
              <span class="stat-label">Live Visitors</span>
              <span class="stat-sub">Active in last 2 minutes</span>
            </div>
          </div>
        </div>

        <!-- ── Linode server graphs ──────────────────────── -->
        <div v-if="linodeStats">
          <template v-if="linodeStats.configured && linodeStats.nodes.length">
            <div v-for="node in linodeStats.nodes" :key="node.id" class="linode-server-card">

              <!-- Server header -->
              <div class="linode-server-header">
                <span class="linode-icon">🖥</span>
                <div>
                  <span class="linode-label">{{ node.label }}</span>
                  <span class="linode-sublabel">Linode #{{ node.id }} · {{ node.vcpus }} vCPU · {{ node.ram ? (node.ram / 1024) + ' GB RAM' : '' }}</span>
                </div>
                <span v-if="node.error" class="linode-badge linode-badge--error">Unreachable</span>
                <span class="linode-refresh-note">refreshes every 30s</span>
              </div>

              <div v-if="node.error" class="linode-error-body">Could not reach Linode API</div>

              <div v-else-if="node.series" class="linode-charts-grid">

                <!-- CPU -->
                <div class="linode-chart-panel">
                  <div class="linode-chart-title">CPU (%)</div>
                  <svg class="linode-svg" :viewBox="`0 0 ${svgW} ${svgH}`" preserveAspectRatio="none">
                    <defs>
                      <linearGradient :id="`cpu-grad-${node.id}`" x1="0" y1="0" x2="0" y2="1">
                        <stop offset="0%" stop-color="#4e9eff" stop-opacity="0.5"/>
                        <stop offset="100%" stop-color="#4e9eff" stop-opacity="0.02"/>
                      </linearGradient>
                    </defs>
                    <path :d="areaPath(node.series.cpu, 100)" :fill="`url(#cpu-grad-${node.id})`"/>
                    <path :d="linePath(node.series.cpu, 100)" fill="none" stroke="#4e9eff" stroke-width="1.5"/>
                  </svg>
                  <div class="linode-chart-stats">
                    <span>Max <strong>{{ fmtPct(node.cpuMax) }}</strong></span>
                    <span>Avg <strong>{{ fmtPct(node.cpuAvg) }}</strong></span>
                    <span>Last <strong>{{ fmtPct(node.cpuCurrent) }}</strong></span>
                  </div>
                </div>

                <!-- Disk I/O -->
                <div class="linode-chart-panel">
                  <div class="linode-chart-title">Disk I/O (blocks/s)</div>
                  <svg class="linode-svg" :viewBox="`0 0 ${svgW} ${svgH}`" preserveAspectRatio="none">
                    <defs>
                      <linearGradient :id="`io-grad-${node.id}`" x1="0" y1="0" x2="0" y2="1">
                        <stop offset="0%" stop-color="#f59e0b" stop-opacity="0.5"/>
                        <stop offset="100%" stop-color="#f59e0b" stop-opacity="0.02"/>
                      </linearGradient>
                    </defs>
                    <path :d="areaPath(node.series.io.read, seriesMax(node.series.io.read))" :fill="`url(#io-grad-${node.id})`"/>
                    <path :d="linePath(node.series.io.read, seriesMax(node.series.io.read))" fill="none" stroke="#f59e0b" stroke-width="1.5"/>
                    <path :d="linePath(node.series.io.swap, seriesMax(node.series.io.read))" fill="none" stroke="#ef4444" stroke-width="1" stroke-dasharray="3,2"/>
                  </svg>
                  <div class="linode-chart-stats">
                    <span><span class="dot" style="background:#f59e0b"></span> I/O Rate &nbsp;Max <strong>{{ fmtNum(Math.max(...node.series.io.read)) }}</strong> Avg <strong>{{ fmtNum(avg(node.series.io.read)) }}</strong> Last <strong>{{ fmtNum(last(node.series.io.read)) }}</strong></span>
                    <span><span class="dot" style="background:#ef4444"></span> Swap &nbsp;Max <strong>{{ fmtNum(Math.max(...node.series.io.swap)) }}</strong></span>
                  </div>
                </div>

                <!-- Network IPv4 -->
                <div class="linode-chart-panel">
                  <div class="linode-chart-title">Network — IPv4 (Kb/s)</div>
                  <svg class="linode-svg" :viewBox="`0 0 ${svgW} ${svgH}`" preserveAspectRatio="none">
                    <defs>
                      <linearGradient :id="`netv4-grad-${node.id}`" x1="0" y1="0" x2="0" y2="1">
                        <stop offset="0%" stop-color="#22c55e" stop-opacity="0.4"/>
                        <stop offset="100%" stop-color="#22c55e" stop-opacity="0.02"/>
                      </linearGradient>
                    </defs>
                    <path :d="areaPath(node.series.netv4.publicIn, seriesMax([...node.series.netv4.publicIn, ...node.series.netv4.publicOut]))" :fill="`url(#netv4-grad-${node.id})`"/>
                    <path :d="linePath(node.series.netv4.publicIn,  seriesMax([...node.series.netv4.publicIn, ...node.series.netv4.publicOut]))" fill="none" stroke="#22c55e" stroke-width="1.5"/>
                    <path :d="linePath(node.series.netv4.publicOut, seriesMax([...node.series.netv4.publicIn, ...node.series.netv4.publicOut]))" fill="none" stroke="#a3e635" stroke-width="1.5"/>
                  </svg>
                  <div class="linode-chart-stats">
                    <span><span class="dot" style="background:#22c55e"></span> Public In &nbsp;Max <strong>{{ fmtKb(Math.max(...node.series.netv4.publicIn)) }}</strong> Avg <strong>{{ fmtKb(avg(node.series.netv4.publicIn)) }}</strong> Last <strong>{{ fmtKb(last(node.series.netv4.publicIn)) }}</strong></span>
                    <span><span class="dot" style="background:#a3e635"></span> Public Out &nbsp;Max <strong>{{ fmtKb(Math.max(...node.series.netv4.publicOut)) }}</strong></span>
                  </div>
                </div>

                <!-- Network IPv6 -->
                <div class="linode-chart-panel">
                  <div class="linode-chart-title">Network — IPv6 (Mb/s)</div>
                  <svg class="linode-svg" :viewBox="`0 0 ${svgW} ${svgH}`" preserveAspectRatio="none">
                    <defs>
                      <linearGradient :id="`netv6-grad-${node.id}`" x1="0" y1="0" x2="0" y2="1">
                        <stop offset="0%" stop-color="#a78bfa" stop-opacity="0.4"/>
                        <stop offset="100%" stop-color="#a78bfa" stop-opacity="0.02"/>
                      </linearGradient>
                    </defs>
                    <path :d="areaPath(node.series.netv6.publicIn, seriesMax([...node.series.netv6.publicIn, ...node.series.netv6.publicOut]))" :fill="`url(#netv6-grad-${node.id})`"/>
                    <path :d="linePath(node.series.netv6.publicIn,  seriesMax([...node.series.netv6.publicIn, ...node.series.netv6.publicOut]))" fill="none" stroke="#a78bfa" stroke-width="1.5"/>
                    <path :d="linePath(node.series.netv6.publicOut, seriesMax([...node.series.netv6.publicIn, ...node.series.netv6.publicOut]))" fill="none" stroke="#f472b6" stroke-width="1.5"/>
                  </svg>
                  <div class="linode-chart-stats">
                    <span><span class="dot" style="background:#a78bfa"></span> Public In &nbsp;Max <strong>{{ fmtMb(Math.max(...node.series.netv6.publicIn)) }}</strong> Avg <strong>{{ fmtMb(avg(node.series.netv6.publicIn)) }}</strong> Last <strong>{{ fmtMb(last(node.series.netv6.publicIn)) }}</strong></span>
                    <span><span class="dot" style="background:#f472b6"></span> Public Out &nbsp;Max <strong>{{ fmtMb(Math.max(...node.series.netv6.publicOut)) }}</strong></span>
                  </div>
                </div>

              </div>
            </div>
          </template>
          <div v-else class="linode-unconfigured">
            <span>🖥</span>
            <div>
              <strong>Linode monitoring not configured</strong>
              <p>Add your Linode API token and node IDs to <code>appsettings.Production.json</code> under <code>Linode</code> to enable live server graphs.</p>
            </div>
          </div>
        </div>

        <!-- ── Lower section ────────────────────────────────── -->
        <div class="dashboard-lower">

          <!-- Col 1: Recent Registrations -->
          <div class="card">
            <div class="card-header d-flex align-items-center justify-content-between">
              <h2 class="card-title">Recent Registrations</h2>
              <router-link v-if="authStore.isSuperAdmin" to="/users" class="btn btn-sm btn-secondary">View all</router-link>
            </div>
            <div class="card-body" style="padding:0;">
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
                    <td>
                      <div class="user-avatar-row">
                        <span class="user-avatar">{{ u.firstName[0] }}{{ u.lastName[0] }}</span>
                        {{ u.firstName }} {{ u.lastName }}
                      </div>
                    </td>
                    <td style="color:#64748b;font-size:0.875rem;">{{ u.email }}</td>
                    <td><span class="role-badge" :class="roleBadgeClass(u.roleName)">{{ u.roleName }}</span></td>
                    <td style="color:#64748b;font-size:0.875rem;">{{ timeAgo(u.createdAt) }}</td>
                  </tr>
                </tbody>
              </table>
              <div v-else style="padding:2rem;text-align:center;color:#94a3b8;">No users registered yet.</div>
            </div>
          </div>

          <!-- Col 2: Recent Activity -->
          <div class="card">
            <div class="card-header"><h2 class="card-title">Recent Activity</h2></div>
            <div class="card-body activity-feed" v-if="recentActivity.length">
              <div v-for="(ev, i) in recentActivity" :key="i" class="activity-item">
                <span class="activity-dot" :class="`activity-dot--${ev.type}`"></span>
                <div class="activity-body">
                  <span class="activity-icon">{{ ev.icon }}</span>
                  <span class="activity-text">{{ ev.title }}</span>
                </div>
                <span class="activity-time">{{ timeAgo(ev.time) }}</span>
              </div>
            </div>
            <div v-else class="card-body" style="color:#94a3b8;font-size:0.875rem;padding:1.25rem;">
              No recent activity.
            </div>
          </div>

          <!-- Col 3: Quick Actions -->
          <div class="card">
            <div class="card-header"><h2 class="card-title">Quick Actions</h2></div>
            <div class="card-body" style="display:flex;flex-direction:column;gap:0.5rem;">
              <router-link to="/catalog" class="action-tile">
                <span class="action-icon" style="background:#ede9fe;">📦</span>
                <div><div class="action-title">Catalog</div><div class="action-desc">Products &amp; categories</div></div>
              </router-link>
              <router-link to="/inquiries" class="action-tile">
                <span class="action-icon" style="background:#f0fdf4;">📧</span>
                <div>
                  <div class="action-title">Inquiries <span v-if="stats.newInquiries > 0" class="badge-count">{{ stats.newInquiries }}</span></div>
                  <div class="action-desc">Contact form submissions</div>
                </div>
              </router-link>
              <router-link to="/orders" class="action-tile">
                <span class="action-icon" style="background:#fef3c7;">🛒</span>
                <div><div class="action-title">Orders</div><div class="action-desc">Customer orders</div></div>
              </router-link>
              <router-link to="/calendar" class="action-tile">
                <span class="action-icon" style="background:#ecfdf5;">📅</span>
                <div><div class="action-title">Calendar</div><div class="action-desc">Appointments &amp; scheduling</div></div>
              </router-link>
              <router-link to="/live-chat" class="action-tile">
                <span class="action-icon" style="background:#ffe4e6;">💬</span>
                <div>
                  <div class="action-title">Live Chat <span v-if="activeChatCount" class="badge-count badge-count--active">{{ activeChatCount }}</span></div>
                  <div class="action-desc">Customer chat sessions</div>
                </div>
              </router-link>
              <router-link to="/media" class="action-tile">
                <span class="action-icon" style="background:#fce7f3;">📁</span>
                <div><div class="action-title">Media Library</div><div class="action-desc">Images &amp; videos</div></div>
              </router-link>
              <router-link to="/music" class="action-tile">
                <span class="action-icon" style="background:#f0fdf4;">🎵</span>
                <div><div class="action-title">Music Player</div><div class="action-desc">Background music settings</div></div>
              </router-link>
              <router-link v-if="authStore.isSuperAdmin" to="/users" class="action-tile">
                <span class="action-icon" style="background:#fef3c7;">👥</span>
                <div><div class="action-title">Users</div><div class="action-desc">Accounts &amp; roles</div></div>
              </router-link>
              <router-link v-if="authStore.isSuperAdmin" to="/backup" class="action-tile">
                <span class="action-icon" style="background:#eff6ff;">💾</span>
                <div><div class="action-title">Backup &amp; Restore</div><div class="action-desc">Database backups</div></div>
              </router-link>
              <router-link to="/settings" class="action-tile">
                <span class="action-icon" style="background:#e0f2fe;">⚙️</span>
                <div><div class="action-title">Site Settings</div><div class="action-desc">Content, theme &amp; config</div></div>
              </router-link>
            </div>
          </div>

        </div>

        <!-- ── API Health Check ──────────────────────────── -->
        <div class="card">
          <div class="card-header d-flex align-items-center justify-content-between">
            <h2 class="card-title">
              API Health Check
              <span v-if="healthStatus" class="health-summary-badge" :class="healthStatus.unhealthy === 0 ? 'health-summary-badge--ok' : 'health-summary-badge--warn'">
                {{ healthStatus.unhealthy === 0 ? `✓ All ${healthStatus.healthy} healthy` : `⚠ ${healthStatus.unhealthy} failing` }}
              </span>
            </h2>
            <button class="btn btn-sm btn-secondary" @click="runHealthCheck" :disabled="isCheckingHealth">
              <span v-if="isCheckingHealth">⟳ Checking…</span>
              <span v-else>⟳ {{ healthStatus ? 'Recheck' : 'Run Check' }}</span>
            </button>
          </div>
          <div class="card-body" style="padding:0;">
            <div v-if="isCheckingHealth && !healthStatus" class="health-checking">
              <div class="spinner-sm"></div> Running health checks across all endpoints…
            </div>
            <template v-else-if="healthStatus">
              <div class="health-grid">
                <div
                  v-for="(r, i) in healthStatus.endpoints"
                  :key="i"
                  class="health-row"
                  :class="r.healthy ? 'health-row--ok' : 'health-row--error'"
                >
                  <span class="health-dot" :class="r.healthy ? 'health-dot--ok' : 'health-dot--error'"></span>
                  <span class="health-name">{{ r.endpoint }}</span>
                  <span class="health-path">{{ r.path }}</span>
                  <span class="health-time">{{ r.responseTime }}ms</span>
                  <span class="health-msg">{{ r.message || r.error || '' }}</span>
                </div>
              </div>
              <div class="health-footer">
                <span>{{ healthStatus.healthy }}/{{ healthStatus.total }} healthy</span>
                <span>·</span>
                <span>Checked {{ new Date(healthStatus.checkedAt).toLocaleTimeString() }}</span>
              </div>
            </template>
            <div v-else class="health-empty">
              Click <strong>Run Check</strong> to test all API endpoints.
            </div>
          </div>
        </div>

        <p class="dashboard-refreshed">Stats refreshed {{ timeAgo(stats.generatedAt) }}</p>
      </template>
    </div>
  </AdminLayout>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, onUnmounted } from 'vue'
import AdminLayout from '@/components/AdminLayout.vue'
import { useAuthStore } from '@/stores/auth'
import type { DashboardStats } from '@/types'
import { logError, logDebug } from '@/services/logger'
import { DASHBOARD_STATS_TIMEOUT_MS } from '@/constants'
import { apiGet } from '@/utils/apiClient'

const authStore = useAuthStore()

// ── Types ──────────────────────────────────────────────────
interface HealthEndpoint {
  endpoint: string
  path: string
  healthy: boolean
  responseTime: number
  message?: string
  error?: string
}
interface HealthReport {
  endpoints: HealthEndpoint[]
  total: number
  healthy: number
  unhealthy: number
  checkedAt: string
}
interface SystemStatusItem { label: string; ok: boolean }

interface ActivityEvent {
  type: 'user' | 'order' | 'inquiry'
  icon: string
  title: string
  time: string
}

interface LinodeSeries {
  cpu: number[]
  io: { read: number[]; swap: number[] }
  netv4: { publicIn: number[]; publicOut: number[]; privateIn: number[]; privateOut: number[] }
  netv6: { publicIn: number[]; publicOut: number[]; privateIn: number[]; privateOut: number[] }
}
interface LinodeNode {
  id: string
  label: string
  vcpus: number
  ram: number
  cpuCurrent: number
  cpuMax: number
  cpuAvg: number
  series?: LinodeSeries
  error: boolean
}
interface LinodeStats {
  configured: boolean
  nodes: LinodeNode[]
}

// ── State ──────────────────────────────────────────────────
const stats = ref<DashboardStats | null>(null)
const isLoading = ref(false)
const error = ref<string | null>(null)
const mediaCount = ref<number | null>(null)
const activeChatCount = ref<number | null>(null)
const healthStatus = ref<HealthReport | null>(null)
const isCheckingHealth = ref(false)
const systemStatus = ref<SystemStatusItem[]>([])
const recentActivity = ref<ActivityEvent[]>([])
const linodeStats = ref<LinodeStats | null>(null)
const visitorCount = ref<number | null>(null)

interface WeatherData {
  tempF: number
  feelsLikeF: number
  desc: string
  humidity: number
  windMph: number
  location: string
}
const weather = ref<WeatherData | null>(null)

let linodePollTimer: ReturnType<typeof setInterval> | null = null

// ── Computed ───────────────────────────────────────────────
const formattedDate = computed(() =>
  new Intl.DateTimeFormat('en-US', { weekday: 'long', month: 'long', day: 'numeric' }).format(new Date())
)

// ── Loaders ────────────────────────────────────────────────
const loadDashboardStats = async () => {
  isLoading.value = true
  error.value = null
  try {
    logDebug('Loading dashboard stats')
    const body = await apiGet<{ data: DashboardStats }>('/admin/dashboard/stats')
    stats.value = body.data
  } catch (err) {
    logError('Failed to load dashboard stats', err)
    error.value = 'Could not load dashboard statistics. Is the API running?'
  } finally {
    isLoading.value = false
  }
}

const loadExtras = async () => {
  const baseUrl = import.meta.env.VITE_API_URL || ''
  const headers: Record<string, string> = {}
  if (authStore.token) headers['Authorization'] = `Bearer ${authStore.token}`

  try {
    const r = await fetch(`${baseUrl}/api/v1/admin/media?pageSize=1`, { headers })
    if (r.ok) { const d = await r.json(); mediaCount.value = d.totalCount ?? null }
  } catch { /* silent */ }

  try {
    const r = await fetch(`${baseUrl}/api/v1/chat/sessions?status=active`, { headers })
    if (r.ok) { const d = await r.json(); activeChatCount.value = Array.isArray(d) ? d.length : (d.count ?? null) }
  } catch { /* silent */ }
}

const loadRecentActivity = async () => {
  const baseUrl = import.meta.env.VITE_API_URL || ''
  const headers: Record<string, string> = {}
  if (authStore.token) headers['Authorization'] = `Bearer ${authStore.token}`
  const events: ActivityEvent[] = []

  try {
    const r = await fetch(`${baseUrl}/api/v1/admin/orders?pageSize=5`, { headers })
    if (r.ok) {
      const d = await r.json()
      for (const o of (d.orders ?? [])) {
        events.push({
          type: 'order',
          icon: '🛒',
          title: `Order from ${o.customerName} — $${Number(o.totalAmount).toFixed(2)}`,
          time: o.createdAt
        })
      }
    }
  } catch { /* silent */ }

  try {
    const r = await fetch(`${baseUrl}/api/v1/admin/contact-submissions?pageSize=5`, { headers })
    if (r.ok) {
      const d = await r.json()
      const arr = Array.isArray(d) ? d : (d.submissions ?? [])
      for (const s of arr) {
        events.push({
          type: 'inquiry',
          icon: '📧',
          title: `Inquiry from ${s.name}${s.subject ? ` — ${s.subject}` : ''}`,
          time: s.createdAt
        })
      }
    }
  } catch { /* silent */ }

  if (stats.value) {
    for (const u of stats.value.recentUsers) {
      events.push({ type: 'user', icon: '👤', title: `${u.firstName} ${u.lastName} registered`, time: u.createdAt })
    }
  }

  events.sort((a, b) => new Date(b.time).getTime() - new Date(a.time).getTime())
  recentActivity.value = events.slice(0, 10)
}

const loadLinodeStats = async () => {
  const baseUrl = import.meta.env.VITE_API_URL || ''
  const headers: Record<string, string> = {}
  if (authStore.token) headers['Authorization'] = `Bearer ${authStore.token}`
  try {
    const r = await fetch(`${baseUrl}/api/v1/admin/dashboard/linode-stats`, { headers })
    if (r.ok) linodeStats.value = await r.json()
  } catch { /* silent */ }
}

const runSystemStatusCheck = async () => {
  const baseUrl = import.meta.env.VITE_API_URL || ''
  const checks = [
    { label: 'API', path: '/health', auth: false },
    { label: 'Auth', path: '/api/v1/auth/me', auth: true },
    { label: 'DB', path: '/api/v1/admin/dashboard/stats', auth: true },
  ]
  const token = authStore.token
  const results = await Promise.all(checks.map(async (c) => {
    try {
      const headers: Record<string, string> = {}
      if (c.auth && token) headers['Authorization'] = `Bearer ${token}`
      const r = await fetch(`${baseUrl}${c.path}`, { headers, signal: AbortSignal.timeout(4000) })
      return { label: c.label, ok: r.ok }
    } catch {
      return { label: c.label, ok: false }
    }
  }))
  systemStatus.value = results
}

const runHealthCheck = async () => {
  isCheckingHealth.value = true
  const endpoints = [
    { path: '/health',                             name: 'API Health',            requiresAuth: false },
    { path: '/api/v1/products',                    name: 'Products',              requiresAuth: false },
    { path: '/api/v1/products/featured',           name: 'Featured Products',     requiresAuth: false },
    { path: '/api/v1/categories',                  name: 'Categories',            requiresAuth: false },
    { path: '/api/v1/settings',                    name: 'Public Settings',       requiresAuth: false },
    { path: '/api/v1/theme',                       name: 'Theme',                 requiresAuth: false },
    { path: '/api/v1/auth/me',                     name: 'Auth — Current User',   requiresAuth: true  },
    { path: '/api/v1/admin/dashboard/stats',       name: 'Dashboard Stats',       requiresAuth: true  },
    { path: '/api/v1/admin/users',                 name: 'Users',                 requiresAuth: true  },
    { path: '/api/v1/admin/settings',              name: 'Settings',              requiresAuth: true  },
    { path: '/api/v1/admin/media',                 name: 'Media Library',         requiresAuth: true  },
    { path: '/api/v1/admin/orders',                name: 'Orders',                requiresAuth: true  },
    { path: '/api/v1/admin/contact-submissions',   name: 'Contact Submissions',   requiresAuth: true  },
    { path: '/api/v1/admin/backup/list',           name: 'Backup',                requiresAuth: true  },
    { path: '/api/v1/appointments',                name: 'Appointments',          requiresAuth: true  },
    { path: '/api/v1/chat/sessions',               name: 'Chat Sessions',         requiresAuth: true  },
  ]
  const baseUrl = import.meta.env.VITE_API_URL || ''
  const results = await Promise.all(endpoints.map(async (ep): Promise<HealthEndpoint> => {
    const t0 = performance.now()
    try {
      const headers: Record<string, string> = {}
      if (ep.requiresAuth && authStore.token) headers['Authorization'] = `Bearer ${authStore.token}`
      const r = await fetch(`${baseUrl}${ep.path}`, { headers, signal: AbortSignal.timeout(DASHBOARD_STATS_TIMEOUT_MS) })
      const ms = Math.round(performance.now() - t0)
      return r.ok
        ? { endpoint: ep.name, path: ep.path, healthy: true,  responseTime: ms, message: `HTTP ${r.status}` }
        : { endpoint: ep.name, path: ep.path, healthy: false, responseTime: ms, error: `HTTP ${r.status} ${r.statusText}` }
    } catch (e: any) {
      return { endpoint: ep.name, path: ep.path, healthy: false, responseTime: Math.round(performance.now() - t0),
        error: e.name === 'TimeoutError' ? 'Timeout' : (e.message || 'Network error') }
    }
  }))
  const healthy = results.filter(r => r.healthy).length
  healthStatus.value = { endpoints: results, total: results.length, healthy, unhealthy: results.length - healthy, checkedAt: new Date().toISOString() }
  isCheckingHealth.value = false
}

// ── Helpers ────────────────────────────────────────────────
const timeAgo = (dateStr: string): string => {
  const diff = Date.now() - new Date(dateStr).getTime()
  const minutes = Math.floor(diff / 60_000)
  if (minutes < 1) return 'just now'
  if (minutes < 60) return `${minutes}m ago`
  const hours = Math.floor(minutes / 60)
  if (hours < 24) return `${hours}h ago`
  return `${Math.floor(hours / 24)}d ago`
}

const roleBadgeClass = (roleName: string) => {
  if (roleName === 'SuperAdmin') return 'role-badge--superadmin'
  if (roleName === 'Admin') return 'role-badge--admin'
  return 'role-badge--user'
}

// ── SVG chart helpers ──────────────────────────────────────
const svgW = 400
const svgH = 80

const seriesMax = (data: number[]): number => {
  const m = Math.max(...data)
  return m > 0 ? m : 1
}

const avg = (data: number[]): number =>
  data.length ? data.reduce((a, b) => a + b, 0) / data.length : 0

const last = (data: number[]): number =>
  data.length ? data[data.length - 1] : 0

const linePath = (data: number[], maxVal: number): string => {
  if (!data.length) return ''
  const mx = maxVal > 0 ? maxVal : 1
  const pts = data.map((v, i) => {
    const x = (i / (data.length - 1)) * svgW
    const y = svgH - (v / mx) * svgH
    return `${i === 0 ? 'M' : 'L'}${x.toFixed(1)},${y.toFixed(1)}`
  })
  return pts.join(' ')
}

const areaPath = (data: number[], maxVal: number): string => {
  if (!data.length) return ''
  const line = linePath(data, maxVal)
  return `${line} L${svgW},${svgH} L0,${svgH} Z`
}

const fmtPct = (v: number): string => `${v.toFixed(2)} %`
const fmtNum = (v: number): string => v.toFixed(2)
const fmtKb  = (v: number): string => v >= 1000 ? `${(v / 1000).toFixed(2)} Mb/s` : `${v.toFixed(0)} Kb/s`
const fmtMb  = (v: number): string => v >= 1000 ? `${(v / 1000).toFixed(2)} Gb/s` : `${v.toFixed(2)} Mb/s`

const loadVisitors = async () => {
  try {
    const baseUrl = import.meta.env.VITE_API_URL || ''
    const headers: Record<string, string> = {}
    if (authStore.token) headers['Authorization'] = `Bearer ${authStore.token}`
    const r = await fetch(`${baseUrl}/api/v1/admin/visitors`, { headers })
    if (r.ok) { const d = await r.json(); visitorCount.value = d.active ?? null }
  } catch { /* silent */ }
}

const loadWeather = async () => {
  try {
    const baseUrl = import.meta.env.VITE_API_URL || ''
    const headers: Record<string, string> = {}
    if (authStore.token) headers['Authorization'] = `Bearer ${authStore.token}`
    const r = await fetch(`${baseUrl}/api/v1/admin/dashboard/weather`, { headers })
    if (!r.ok) return
    const d = await r.json()
    if (!d.ok) return
    weather.value = {
      tempF: d.tempF,
      feelsLikeF: d.feelsLikeF,
      desc: d.desc,
      humidity: d.humidity,
      windMph: d.windMph,
      location: d.location
    }
  } catch { /* silent */ }
}

const weatherIcon = computed(() => {
  const d = weather.value?.desc.toLowerCase() ?? ''
  if (d.includes('sun') || d.includes('clear')) return '☀️'
  if (d.includes('thunder') || d.includes('storm')) return '⛈️'
  if (d.includes('snow') || d.includes('blizzard')) return '❄️'
  if (d.includes('rain') || d.includes('shower') || d.includes('drizzle')) return '🌧️'
  if (d.includes('cloud') || d.includes('overcast')) return '☁️'
  if (d.includes('fog') || d.includes('mist') || d.includes('haze')) return '🌫️'
  if (d.includes('wind')) return '💨'
  return '🌡️'
})

// ── Actions ────────────────────────────────────────────────
const refresh = async () => {
  await Promise.all([loadDashboardStats(), loadExtras()])
  loadRecentActivity()
  loadVisitors()
}

onMounted(async () => {
  await Promise.all([loadDashboardStats(), loadExtras(), runSystemStatusCheck(), loadLinodeStats(), loadWeather(), loadVisitors()])
  loadRecentActivity()
  linodePollTimer = setInterval(loadLinodeStats, 30_000)
})

onUnmounted(() => {
  if (linodePollTimer) clearInterval(linodePollTimer)
})
</script>

<style scoped>
.dashboard {
  display: flex;
  flex-direction: column;
  gap: 1.5rem;
}

/* ── Header ──────────────────────────────────────────── */
.dashboard-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 1rem;
  flex-wrap: wrap;
}
.header-actions {
  display: flex;
  align-items: center;
  gap: 0.75rem;
  flex-wrap: wrap;
}

/* ── Weather chip ─────────────────────────────────────── */
.weather-chip {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  background: #f8fafc;
  border: 1px solid #e2e8f0;
  border-radius: 10px;
  padding: 0.4rem 0.75rem;
  font-size: 0.8rem;
}
.weather-chip__icon { font-size: 1.4rem; line-height: 1; }
.weather-chip__info { display: flex; flex-direction: column; line-height: 1.3; }
.weather-chip__temp { font-weight: 700; font-size: 0.95rem; color: #111827; }
.weather-chip__desc { font-size: 0.72rem; color: #6b7280; }
.weather-chip__detail {
  display: flex;
  flex-direction: column;
  font-size: 0.7rem;
  color: #6b7280;
  line-height: 1.4;
  padding-left: 0.5rem;
  border-left: 1px solid #e2e8f0;
}
.dashboard-title {
  font-size: 1.75rem;
  font-weight: 800;
  color: #111827;
  margin-bottom: 0.2rem;
}
.dashboard-subtitle { color: #64748b; font-size: 0.9rem; }

/* ── System status pills ─────────────────────────────── */
.status-pills { display: flex; gap: 0.4rem; }
.status-pill {
  display: inline-flex;
  align-items: center;
  gap: 0.35rem;
  padding: 0.25rem 0.6rem;
  border-radius: 999px;
  font-size: 0.72rem;
  font-weight: 600;
  letter-spacing: 0.03em;
}
.status-pill--ok    { background: #d1fae5; color: #065f46; }
.status-pill--error { background: #fee2e2; color: #991b1b; }
.status-dot {
  width: 6px; height: 6px;
  border-radius: 50%;
  background: currentColor;
  display: inline-block;
}

/* ── Stats grid ──────────────────────────────────────── */
.stats-grid {
  display: grid;
  grid-template-columns: repeat(4, 1fr);
  gap: 1rem;
}
@media (max-width: 1100px) { .stats-grid { grid-template-columns: repeat(2, 1fr); } }
@media (max-width: 600px)  { .stats-grid { grid-template-columns: 1fr; } }

.stat-card {
  background: white;
  border-radius: 0.875rem;
  box-shadow: 0 1px 4px rgba(0,0,0,0.07);
  padding: 1.25rem 1.375rem;
  display: flex;
  align-items: center;
  gap: 1rem;
  border-left: 4px solid transparent;
  transition: box-shadow 0.2s, transform 0.15s;
}
.stat-card:hover { box-shadow: 0 6px 18px rgba(0,0,0,0.1); transform: translateY(-1px); }
.stat-card--purple { border-left-color: #7c3aed; }
.stat-card--green  { border-left-color: #059669; }
.stat-card--blue   { border-left-color: #2563eb; }
.stat-card--amber  { border-left-color: #d97706; }
.stat-card--teal   { border-left-color: #16a34a; }
.stat-card--indigo { border-left-color: #4338ca; }
.stat-card--rose   { border-left-color: #e11d48; }
.stat-card--red    { border-left-color: #dc2626; }
.stat-card--cyan   { border-left-color: #0891b2; }

.stat-card.skeleton {
  min-height: 90px;
  background: linear-gradient(90deg, #f1f5f9 25%, #e2e8f0 50%, #f1f5f9 75%);
  background-size: 200% 100%;
  animation: shimmer 1.4s infinite;
  border-left-color: #e2e8f0;
}
@keyframes shimmer { 0% { background-position: 200% 0; } 100% { background-position: -200% 0; } }

.stat-icon {
  width: 3rem; height: 3rem;
  border-radius: 0.75rem;
  display: flex; align-items: center; justify-content: center;
  font-size: 1.4rem;
  flex-shrink: 0;
}
.stat-info  { display: flex; flex-direction: column; gap: 0.1rem; min-width: 0; }
.stat-value { font-size: 1.875rem; font-weight: 800; color: #111827; line-height: 1; }
.stat-label { font-size: 0.875rem; font-weight: 600; color: #374151; }
.stat-sub   { font-size: 0.72rem; color: #94a3b8; white-space: nowrap; overflow: hidden; text-overflow: ellipsis; }

/* ── Linode server graphs ─────────────────────────────── */
.linode-server-card {
  background: #1a1f2e;
  border-radius: 0.875rem;
  overflow: hidden;
  border: 1px solid #2d3548;
}

.linode-server-header {
  display: flex;
  align-items: center;
  gap: 0.75rem;
  padding: 0.875rem 1.25rem;
  border-bottom: 1px solid #2d3548;
}
.linode-icon { font-size: 1.25rem; }
.linode-label { display: block; font-weight: 700; font-size: 0.95rem; color: #f1f5f9; }
.linode-sublabel { display: block; font-size: 0.72rem; color: #64748b; margin-top: 0.15rem; }
.linode-badge {
  padding: 0.2rem 0.6rem;
  border-radius: 999px;
  font-size: 0.68rem;
  font-weight: 600;
}
.linode-badge--error { background: #450a0a; color: #fca5a5; }
.linode-refresh-note { margin-left: auto; font-size: 0.68rem; color: #475569; }

.linode-error-body {
  padding: 2rem;
  color: #94a3b8;
  font-size: 0.875rem;
}

.linode-charts-grid {
  display: grid;
  grid-template-columns: repeat(4, 1fr);
  gap: 1px;
  background: #2d3548;
}
@media (max-width: 1100px) { .linode-charts-grid { grid-template-columns: repeat(2, 1fr); } }
@media (max-width: 600px)  { .linode-charts-grid { grid-template-columns: 1fr; } }

.linode-chart-panel {
  background: #1a1f2e;
  padding: 0.875rem 1rem 0.625rem;
}

.linode-chart-title {
  font-size: 0.78rem;
  font-weight: 600;
  color: #94a3b8;
  margin-bottom: 0.5rem;
  text-transform: uppercase;
  letter-spacing: 0.05em;
}

.linode-svg {
  display: block;
  width: 100%;
  height: 65px;
  overflow: visible;
}

.linode-chart-stats {
  display: flex;
  flex-direction: column;
  gap: 0.2rem;
  margin-top: 0.5rem;
  font-size: 0.72rem;
  color: #64748b;
  border-top: 1px solid #2d3548;
  padding-top: 0.5rem;
}
.linode-chart-stats span { display: flex; align-items: center; gap: 0.35rem; }
.linode-chart-stats strong { color: #cbd5e1; }

.dot {
  display: inline-block;
  width: 8px; height: 8px;
  border-radius: 50%;
  flex-shrink: 0;
}

.linode-unconfigured {
  display: flex;
  align-items: flex-start;
  gap: 1rem;
  padding: 1.25rem 1.5rem;
  background: white;
  border-radius: 0.875rem;
  border: 1px dashed #e2e8f0;
  font-size: 0.875rem;
  color: #64748b;
}
.linode-unconfigured span { font-size: 1.5rem; }
.linode-unconfigured strong { display: block; color: #374151; margin-bottom: 0.25rem; }
.linode-unconfigured p { margin: 0; font-size: 0.8rem; }
.linode-unconfigured code { background: #f1f5f9; padding: 0.1rem 0.35rem; border-radius: 4px; font-size: 0.78rem; }

/* ── Lower layout ────────────────────────────────────── */
.dashboard-lower {
  display: grid;
  grid-template-columns: 1fr 1fr 300px;
  gap: 1.5rem;
  align-items: start;
}
@media (max-width: 1100px) { .dashboard-lower { grid-template-columns: 1fr 1fr; } }
@media (max-width: 768px)  { .dashboard-lower { grid-template-columns: 1fr; } }

.dashboard-right-col {
  display: flex;
  flex-direction: column;
  gap: 1.25rem;
}

/* ── User avatar row ─────────────────────────────────── */
.user-avatar-row { display: flex; align-items: center; gap: 0.625rem; font-size: 0.9rem; }
.user-avatar {
  width: 1.75rem; height: 1.75rem;
  border-radius: 50%;
  background: #e0e7ff;
  color: #4338ca;
  font-size: 0.65rem;
  font-weight: 700;
  display: inline-flex; align-items: center; justify-content: center;
  flex-shrink: 0;
  text-transform: uppercase;
}

/* ── Role badges ─────────────────────────────────────── */
.role-badge {
  display: inline-block;
  padding: 0.2rem 0.6rem;
  border-radius: 999px;
  font-size: 0.7rem;
  font-weight: 600;
  text-transform: uppercase;
  letter-spacing: 0.04em;
}
.role-badge--user       { background: #eff6ff; color: #2563eb; }
.role-badge--admin      { background: #fef3c7; color: #d97706; }
.role-badge--superadmin { background: #fee2e2; color: #dc2626; }

/* ── Recent Activity ─────────────────────────────────── */
.activity-feed {
  display: flex;
  flex-direction: column;
  gap: 0;
  padding: 0 !important;
}
.activity-item {
  display: flex;
  align-items: center;
  gap: 0.75rem;
  padding: 0.75rem 1.25rem;
  border-bottom: 1px solid #f1f5f9;
  transition: background 0.1s;
}
.activity-item:last-child { border-bottom: none; }
.activity-item:hover { background: #f8fafc; }

.activity-dot {
  width: 8px; height: 8px;
  border-radius: 50%;
  flex-shrink: 0;
}
.activity-dot--user    { background: #6366f1; }
.activity-dot--order   { background: #f59e0b; }
.activity-dot--inquiry { background: #22c55e; }

.activity-body {
  display: flex;
  align-items: center;
  gap: 0.4rem;
  flex: 1;
  min-width: 0;
}
.activity-icon { font-size: 0.9rem; flex-shrink: 0; }
.activity-text {
  font-size: 0.8rem;
  color: #374151;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}
.activity-time {
  font-size: 0.72rem;
  color: #94a3b8;
  flex-shrink: 0;
}

/* ── Action tiles ────────────────────────────────────── */
.action-tile {
  display: flex;
  align-items: center;
  gap: 0.75rem;
  padding: 0.625rem 0.75rem;
  border-radius: 0.5rem;
  background: #f8fafc;
  border: 1px solid #e2e8f0;
  text-decoration: none;
  color: inherit;
  transition: all 0.15s;
  cursor: pointer;
}
.action-tile:hover { background: #f1f5f9; border-color: #c7d2fe; transform: translateX(3px); }

.action-icon {
  width: 2rem; height: 2rem;
  border-radius: 0.375rem;
  display: flex; align-items: center; justify-content: center;
  font-size: 1rem;
  flex-shrink: 0;
}
.action-title { font-size: 0.825rem; font-weight: 600; color: #1e293b; display: flex; align-items: center; gap: 0.4rem; }
.action-desc  { font-size: 0.72rem; color: #94a3b8; }

.badge-count {
  display: inline-flex; align-items: center; justify-content: center;
  min-width: 1.1rem; height: 1.1rem;
  padding: 0 0.25rem;
  border-radius: 999px;
  background: #fee2e2; color: #dc2626;
  font-size: 0.6rem; font-weight: 700;
}
.badge-count--active { background: #d1fae5; color: #059669; }

/* ── Health check ────────────────────────────────────── */
.health-summary-badge {
  display: inline-block;
  margin-left: 0.75rem;
  padding: 0.15rem 0.6rem;
  border-radius: 999px;
  font-size: 0.75rem;
  font-weight: 600;
}
.health-summary-badge--ok   { background: #d1fae5; color: #065f46; }
.health-summary-badge--warn { background: #fef3c7; color: #92400e; }

.health-grid { display: flex; flex-direction: column; }
.health-row {
  display: grid;
  grid-template-columns: 1rem 180px 1fr 70px 120px;
  align-items: center;
  gap: 0.75rem;
  padding: 0.625rem 1.25rem;
  border-bottom: 1px solid #f1f5f9;
  font-size: 0.82rem;
  transition: background 0.1s;
}
.health-row:last-child  { border-bottom: none; }
.health-row:hover       { background: #f8fafc; }
.health-row--error      { background: #fff5f5; }
.health-row--error:hover { background: #fef2f2; }

.health-dot { width: 8px; height: 8px; border-radius: 50%; flex-shrink: 0; }
.health-dot--ok    { background: #22c55e; box-shadow: 0 0 0 2px #bbf7d0; }
.health-dot--error { background: #ef4444; box-shadow: 0 0 0 2px #fecaca; }
.health-name { font-weight: 600; color: #1e293b; }
.health-path { font-family: monospace; color: #64748b; font-size: 0.78rem; overflow: hidden; text-overflow: ellipsis; white-space: nowrap; }
.health-time { color: #94a3b8; text-align: right; }
.health-msg  { color: #64748b; overflow: hidden; text-overflow: ellipsis; white-space: nowrap; }

.health-footer {
  display: flex;
  gap: 0.5rem;
  padding: 0.875rem 1.25rem;
  background: #f8fafc;
  border-top: 1px solid #e2e8f0;
  font-size: 0.78rem;
  color: #94a3b8;
}
.health-checking {
  display: flex; align-items: center; gap: 0.75rem;
  padding: 2rem 1.5rem;
  color: #64748b; font-size: 0.875rem;
}
.health-empty {
  padding: 2rem 1.5rem;
  color: #94a3b8; font-size: 0.875rem;
  text-align: center;
}

/* ── Error banner ────────────────────────────────────── */
.error-banner {
  display: flex; align-items: center; gap: 1rem;
  padding: 0.875rem 1.25rem;
  background: #fef2f2;
  border: 1px solid #fecaca;
  border-radius: 0.625rem;
  color: #dc2626;
  font-size: 0.9rem;
}

.spinner-sm {
  width: 1rem; height: 1rem;
  border: 2px solid #e2e8f0; border-top-color: #6366f1;
  border-radius: 50%;
  animation: spin 0.7s linear infinite;
  flex-shrink: 0;
}
@keyframes spin { to { transform: rotate(360deg); } }

.dashboard-refreshed {
  font-size: 0.75rem;
  color: #cbd5e1;
  text-align: right;
}

@media (max-width: 640px) {
  .health-row { grid-template-columns: 1rem 1fr 60px; }
  .health-path, .health-msg { display: none; }
}
</style>
