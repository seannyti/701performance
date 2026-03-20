<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useVehicleStore } from '@/stores/vehicle.store'
import { useInquiryStore } from '@/stores/inquiry.store'
import { useOrderStore } from '@/stores/order.store'
import { useAuthStore } from '@/stores/auth.store'
import AdminShell from '@/components/layout/AdminShell.vue'
import Button from 'primevue/button'
import Tag from 'primevue/tag'
import { RouterLink } from 'vue-router'
import * as appointmentService from '@/services/appointment.service'
import * as usersService from '@/services/users.service'
import type { Appointment } from '@/types/appointment.types'

const vehicleStore = useVehicleStore()
const inquiryStore = useInquiryStore()
const orderStore = useOrderStore()
const auth = useAuthStore()

const upcomingAppointments = ref<Appointment[]>([])
const totalUsers = ref(0)
const loading = ref(true)

// ── Greeting ─────────────────────────────────────────────────────────────
const greeting = computed(() => {
  const h = new Date().getHours()
  if (h < 12) return 'Good morning'
  if (h < 17) return 'Good afternoon'
  return 'Good evening'
})

const adminName = computed(() => {
  const email = auth.email ?? ''
  return email.split('@')[0] || 'Admin'
})

const todayLabel = computed(() =>
  new Date().toLocaleDateString('en-US', { weekday: 'long', year: 'numeric', month: 'long', day: 'numeric' })
)

// ── Derived stats ─────────────────────────────────────────────────────────
const featuredCount = computed(() => vehicleStore.vehicles.filter(v => v.featured).length)
const pendingOrders = computed(() => orderStore.orders.filter(o => o.status === 'Pending').length)
const recentInquiries = computed(() => inquiryStore.inquiries.slice(0, 6))
const fs = computed(() => orderStore.financeStats)

// ── Formatters ────────────────────────────────────────────────────────────
function currency(val: number) {
  if (val >= 1_000_000) return `$${(val / 1_000_000).toFixed(1)}M`
  if (val >= 1_000) return `$${(val / 1_000).toFixed(0)}K`
  return `$${val.toFixed(0)}`
}

function timeAgo(dateStr: string): string {
  const diff = Date.now() - new Date(dateStr).getTime()
  const mins = Math.floor(diff / 60000)
  if (mins < 60) return `${mins}m ago`
  const hrs = Math.floor(mins / 60)
  if (hrs < 24) return `${hrs}h ago`
  return `${Math.floor(hrs / 24)}d ago`
}

function apptDate(dateStr: string): string {
  const d = new Date(dateStr)
  const today = new Date()
  const tomorrow = new Date(today)
  tomorrow.setDate(today.getDate() + 1)
  if (d.toDateString() === today.toDateString())
    return `Today ${d.toLocaleTimeString('en-US', { hour: 'numeric', minute: '2-digit' })}`
  if (d.toDateString() === tomorrow.toDateString())
    return `Tomorrow ${d.toLocaleTimeString('en-US', { hour: 'numeric', minute: '2-digit' })}`
  return d.toLocaleDateString('en-US', { weekday: 'short', month: 'short', day: 'numeric' }) +
    ' ' + d.toLocaleTimeString('en-US', { hour: 'numeric', minute: '2-digit' })
}

const inquirySeverity: Record<string, 'danger' | 'warn' | 'success'> = {
  New: 'danger', InProgress: 'warn', Resolved: 'success'
}

// ── Load ──────────────────────────────────────────────────────────────────
onMounted(async () => {
  loading.value = true
  const now = new Date()
  const weekOut = new Date(now)
  weekOut.setDate(now.getDate() + 7)

  await Promise.all([
    vehicleStore.fetchVehicles(1, 200),
    inquiryStore.fetchInquiries(1, 6),
    inquiryStore.fetchStats(),
    orderStore.fetchOrders(1, 200),
    orderStore.fetchFinanceStats(),
    appointmentService.getAppointments(now.toISOString(), weekOut.toISOString())
      .then(r => { upcomingAppointments.value = r.filter(a => a.status === 'Scheduled') }),
    usersService.getUsers(1, 1).then(r => { totalUsers.value = r.totalCount }),
  ])
  loading.value = false
})
</script>

<template>
  <AdminShell>
    <div class="dashboard">

      <!-- ── Header ───────────────────────────────────────────────────── -->
      <div class="dash-header">
        <div>
          <h1>{{ greeting }}, <span class="name-accent">{{ adminName }}</span></h1>
          <p class="date-label">{{ todayLabel }}</p>
        </div>
        <RouterLink to="/catalog/vehicles/new">
          <Button label="Add Vehicle" icon="pi pi-plus" severity="warn" size="small" />
        </RouterLink>
      </div>

      <!-- ── KPI Cards ─────────────────────────────────────────────────── -->
      <div class="kpi-grid">
        <!-- Inventory -->
        <div class="kpi-card kpi-card--blue">
          <div class="kpi-card__top">
            <span class="kpi-label">Inventory</span>
            <i class="pi pi-car kpi-icon" />
          </div>
          <div class="kpi-value">{{ vehicleStore.totalCount }}</div>
          <div class="kpi-sub">{{ featuredCount }} featured</div>
        </div>

        <!-- Revenue -->
        <div class="kpi-card kpi-card--green">
          <div class="kpi-card__top">
            <span class="kpi-label">Total Revenue</span>
            <i class="pi pi-dollar kpi-icon" />
          </div>
          <div class="kpi-value">{{ fs ? currency(fs.totalRevenue) : '—' }}</div>
          <div class="kpi-sub">{{ fs ? currency(fs.thisMonthRevenue) : '—' }} this month</div>
        </div>

        <!-- Inquiries -->
        <div class="kpi-card" :class="inquiryStore.stats.new > 0 ? 'kpi-card--red' : 'kpi-card--muted'">
          <div class="kpi-card__top">
            <span class="kpi-label">New Inquiries</span>
            <i class="pi pi-envelope kpi-icon" />
          </div>
          <div class="kpi-value">{{ inquiryStore.stats.new }}</div>
          <div class="kpi-sub">{{ inquiryStore.stats.total }} total · {{ inquiryStore.stats.resolved }} resolved</div>
        </div>

        <!-- Orders -->
        <div class="kpi-card" :class="pendingOrders > 0 ? 'kpi-card--amber' : 'kpi-card--muted'">
          <div class="kpi-card__top">
            <span class="kpi-label">Pending Orders</span>
            <i class="pi pi-clipboard kpi-icon" />
          </div>
          <div class="kpi-value">{{ pendingOrders }}</div>
          <div class="kpi-sub">{{ orderStore.totalCount }} total · {{ fs?.totalUnitsSold ?? 0 }} sold</div>
        </div>
      </div>

      <!-- ── Secondary stats ────────────────────────────────────────────── -->
      <div class="secondary-grid">
        <div class="sec-card">
          <i class="pi pi-chart-line sec-icon" style="color:#4ade80" />
          <div>
            <div class="sec-value">{{ fs?.thisMonthUnits ?? 0 }}</div>
            <div class="sec-label">Units this month</div>
          </div>
        </div>
        <div class="sec-card">
          <i class="pi pi-tag sec-icon" style="color:#60a5fa" />
          <div>
            <div class="sec-value">{{ fs ? currency(fs.avgSalePrice) : '—' }}</div>
            <div class="sec-label">Avg sale price</div>
          </div>
        </div>
        <div class="sec-card">
          <i class="pi pi-calendar sec-icon" style="color:#f4a261" />
          <div>
            <div class="sec-value">{{ upcomingAppointments.length }}</div>
            <div class="sec-label">Appointments this week</div>
          </div>
        </div>
        <div class="sec-card">
          <i class="pi pi-users sec-icon" style="color:#c084fc" />
          <div>
            <div class="sec-value">{{ totalUsers }}</div>
            <div class="sec-label">Registered users</div>
          </div>
        </div>
      </div>

      <!-- ── Content panels ─────────────────────────────────────────────── -->
      <div class="panels">

        <!-- Recent Inquiries -->
        <div class="panel">
          <div class="panel__head">
            <span class="panel__title">Recent Inquiries</span>
            <RouterLink to="/inquiries">
              <Button label="View all" icon="pi pi-arrow-right" icon-pos="right" text size="small" severity="secondary" />
            </RouterLink>
          </div>
          <div v-if="loading" class="panel__empty">Loading…</div>
          <div v-else-if="recentInquiries.length === 0" class="panel__empty">No inquiries yet.</div>
          <div v-else class="inq-list">
            <div v-for="inq in recentInquiries" :key="inq.id" class="inq-row">
              <div class="inq-avatar">{{ inq.name.charAt(0).toUpperCase() }}</div>
              <div class="inq-body">
                <div class="inq-name">{{ inq.name }}</div>
                <div class="inq-sub">{{ inq.vehicleName ?? 'General inquiry' }} · {{ timeAgo(inq.createdAt) }}</div>
              </div>
              <Tag :value="inq.status" :severity="inquirySeverity[inq.status]" style="font-size:0.7rem" />
            </div>
          </div>
        </div>

        <!-- Upcoming Appointments -->
        <div class="panel">
          <div class="panel__head">
            <span class="panel__title">Upcoming Appointments</span>
            <RouterLink to="/calendar">
              <Button label="Calendar" icon="pi pi-arrow-right" icon-pos="right" text size="small" severity="secondary" />
            </RouterLink>
          </div>
          <div v-if="loading" class="panel__empty">Loading…</div>
          <div v-else-if="upcomingAppointments.length === 0" class="panel__empty">No appointments this week.</div>
          <div v-else class="appt-list">
            <div v-for="appt in upcomingAppointments" :key="appt.id" class="appt-row">
              <div class="appt-time">
                <i class="pi pi-clock" style="font-size:0.75rem; color:#9a9a9a" />
                <span>{{ apptDate(appt.startTime) }}</span>
              </div>
              <div class="appt-title">{{ appt.title }}</div>
              <div class="appt-customer">{{ appt.customerName }}</div>
              <div v-if="appt.vehicleName" class="appt-vehicle">{{ appt.vehicleName }}</div>
            </div>
          </div>
        </div>

      </div>

      <!-- ── Quick actions ──────────────────────────────────────────────── -->
      <div class="quick-actions">
        <RouterLink to="/inquiries">
          <Button label="Inquiries" icon="pi pi-envelope" severity="secondary" outlined size="small" />
        </RouterLink>
        <RouterLink to="/orders">
          <Button label="Orders" icon="pi pi-clipboard" severity="secondary" outlined size="small" />
        </RouterLink>
        <RouterLink to="/calendar">
          <Button label="Calendar" icon="pi pi-calendar" severity="secondary" outlined size="small" />
        </RouterLink>
        <RouterLink to="/users">
          <Button label="Users" icon="pi pi-users" severity="secondary" outlined size="small" />
        </RouterLink>
        <RouterLink to="/finance">
          <Button label="Finance" icon="pi pi-chart-bar" severity="secondary" outlined size="small" />
        </RouterLink>
      </div>

    </div>
  </AdminShell>
</template>

<style scoped>
.dashboard { display: flex; flex-direction: column; gap: 22px; }

/* Header */
.dash-header {
  display: flex; align-items: center; justify-content: space-between; flex-wrap: wrap; gap: 12px;
}
.dash-header h1 { font-size: 1.6rem; font-weight: 700; }
.name-accent { color: #e8a020; }
.date-label { font-size: 0.8rem; color: #555; margin-top: 3px; }

/* KPI cards */
.kpi-grid {
  display: grid;
  grid-template-columns: repeat(4, 1fr);
  gap: 14px;
}

@media (max-width: 900px) {
  .kpi-grid { grid-template-columns: repeat(2, 1fr); }
}
@media (max-width: 500px) {
  .kpi-grid { grid-template-columns: 1fr; }
}

.kpi-card {
  background: #111;
  border: 1px solid #222;
  border-left: 3px solid #333;
  border-radius: 10px;
  padding: 18px 20px;
  display: flex;
  flex-direction: column;
  gap: 6px;
  transition: border-color 0.2s;
}
.kpi-card--blue  { border-left-color: #60a5fa; }
.kpi-card--green { border-left-color: #4ade80; }
.kpi-card--red   { border-left-color: #f87171; }
.kpi-card--amber { border-left-color: #fbbf24; }
.kpi-card--muted { border-left-color: #333; }

.kpi-card__top { display: flex; align-items: center; justify-content: space-between; }
.kpi-label { font-size: 0.7rem; font-weight: 600; color: #666; text-transform: uppercase; letter-spacing: 0.07em; }
.kpi-icon { font-size: 1rem; color: #444; }
.kpi-value { font-size: 2rem; font-weight: 700; line-height: 1.1; margin-top: 4px; }
.kpi-sub { font-size: 0.75rem; color: #666; }

/* Secondary stats */
.secondary-grid {
  display: grid;
  grid-template-columns: repeat(4, 1fr);
  gap: 14px;
}

@media (max-width: 900px) {
  .secondary-grid { grid-template-columns: repeat(2, 1fr); }
}

.sec-card {
  background: #0d0d0d;
  border: 1px solid #1a1a1a;
  border-radius: 10px;
  padding: 14px 16px;
  display: flex;
  align-items: center;
  gap: 14px;
}
.sec-icon { font-size: 1.25rem; flex-shrink: 0; }
.sec-value { font-size: 1.3rem; font-weight: 700; }
.sec-label { font-size: 0.7rem; color: #555; margin-top: 2px; text-transform: uppercase; letter-spacing: 0.05em; }

/* Content panels */
.panels {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 14px;
}

@media (max-width: 800px) {
  .panels { grid-template-columns: 1fr; }
}

.panel {
  background: #111;
  border: 1px solid #222;
  border-radius: 10px;
  overflow: hidden;
}

.panel__head {
  display: flex; align-items: center; justify-content: space-between;
  padding: 14px 18px 12px;
  border-bottom: 1px solid #1a1a1a;
}
.panel__title { font-size: 0.8rem; font-weight: 600; color: #888; text-transform: uppercase; letter-spacing: 0.06em; }
.panel__empty { padding: 28px 18px; font-size: 0.85rem; color: #444; text-align: center; }

/* Inquiry list */
.inq-list { display: flex; flex-direction: column; }
.inq-row {
  display: flex; align-items: center; gap: 12px;
  padding: 11px 18px;
  border-bottom: 1px solid #161616;
  transition: background 0.15s;
}
.inq-row:last-child { border-bottom: none; }
.inq-row:hover { background: #161616; }

.inq-avatar {
  width: 32px; height: 32px; border-radius: 50%;
  background: #1e1e1e; border: 1px solid #2a2a2a;
  display: flex; align-items: center; justify-content: center;
  font-size: 0.85rem; font-weight: 700; color: #888;
  flex-shrink: 0;
}
.inq-body { flex: 1; min-width: 0; }
.inq-name { font-size: 0.875rem; font-weight: 500; white-space: nowrap; overflow: hidden; text-overflow: ellipsis; }
.inq-sub { font-size: 0.75rem; color: #555; margin-top: 1px; white-space: nowrap; overflow: hidden; text-overflow: ellipsis; }

/* Appointment list */
.appt-list { display: flex; flex-direction: column; }
.appt-row {
  padding: 11px 18px;
  border-bottom: 1px solid #161616;
  transition: background 0.15s;
}
.appt-row:last-child { border-bottom: none; }
.appt-row:hover { background: #161616; }

.appt-time {
  display: flex; align-items: center; gap: 5px;
  font-size: 0.7rem; color: #666; margin-bottom: 3px;
  text-transform: uppercase; letter-spacing: 0.04em;
}
.appt-title { font-size: 0.875rem; font-weight: 600; }
.appt-customer { font-size: 0.75rem; color: #666; margin-top: 1px; }
.appt-vehicle { font-size: 0.7rem; color: #444; margin-top: 1px; }

/* Quick actions */
.quick-actions {
  display: flex; gap: 10px; flex-wrap: wrap;
}
.quick-actions a { text-decoration: none; }
</style>
