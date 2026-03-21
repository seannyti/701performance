<script setup lang="ts">
import { ref, onMounted, watch } from 'vue'
import { useRouter } from 'vue-router'
import { useInquiryStore } from '@/stores/inquiry.store'
import AdminShell from '@/components/layout/AdminShell.vue'
import Button from 'primevue/button'
import InputText from 'primevue/inputtext'
import Select from 'primevue/select'
import Tag from 'primevue/tag'
import IconField from 'primevue/iconfield'
import InputIcon from 'primevue/inputicon'
import type { Inquiry } from '@/types/inquiry.types'

const store = useInquiryStore()
const router = useRouter()
const selected = ref<Inquiry | null>(null)

onMounted(async () => {
  await Promise.all([store.fetchInquiries(), store.fetchStats()])
})

watch(() => ({ ...store.filters }), () => {
  store.fetchInquiries()
}, { deep: true })

async function selectInquiry(inq: Inquiry) {
  selected.value = inq
}

async function setStatus(id: number, status: string) {
  const updated = await store.updateStatus(id, status)
  if (selected.value?.id === id) selected.value = updated
}

async function deleteSelected() {
  if (!selected.value) return
  await store.deleteInquiry(selected.value.id)
  selected.value = null
}

function formatDate(d: string) {
  return new Date(d).toLocaleString('en-US', {
    month: 'short', day: 'numeric', year: 'numeric',
    hour: 'numeric', minute: '2-digit'
  })
}

function formatAvg(mins: number | null) {
  if (mins === null) return '-'
  if (mins < 60) return `${Math.round(mins)}m`
  const h = Math.floor(mins / 60)
  const m = Math.round(mins % 60)
  return m > 0 ? `${h}h ${m}m` : `${h}h`
}

const statusSeverity: Record<string, string> = {
  New: 'info',
  InProgress: 'warn',
  Resolved: 'success',
}

const statusOptions = [
  { label: 'New', value: 'New' },
  { label: 'In Progress', value: 'InProgress' },
  { label: 'Resolved', value: 'Resolved' },
]

const statusFilterOptions = [
  { label: 'New', value: 'New' },
  { label: 'In Progress', value: 'InProgress' },
  { label: 'Resolved', value: 'Resolved' },
]

function statusLabel(s: string) {
  if (s === 'InProgress') return 'In Progress'
  return s
}

function convertToOrder(inq: Inquiry) {
  const params = new URLSearchParams({
    inquiryId: String(inq.id),
    vehicleId: String(inq.vehicleId),
    customerName: inq.name,
    customerEmail: inq.email,
    customerPhone: inq.phone ?? '',
  })
  router.push(`/sales/orders/new?${params.toString()}`)
}
</script>

<template>
  <AdminShell>
    <div class="inquiries">
      <div class="page-header">
        <h1>Inquiries</h1>
        <p>Review and manage customer inquiries.</p>
      </div>

      <!-- Stats -->
      <div class="stats-row">
        <div class="stat-card">
          <div class="stat-card__label">Total</div>
          <div class="stat-card__value">{{ store.stats.total }}</div>
        </div>
        <div class="stat-card stat-card--new">
          <div class="stat-card__label">New</div>
          <div class="stat-card__value">{{ store.stats.new }}</div>
        </div>
        <div class="stat-card stat-card--inprogress">
          <div class="stat-card__label">In Progress</div>
          <div class="stat-card__value">{{ store.stats.inProgress }}</div>
        </div>
        <div class="stat-card stat-card--resolved">
          <div class="stat-card__label">Resolved</div>
          <div class="stat-card__value">{{ store.stats.resolved }}</div>
        </div>
        <div class="stat-card">
          <div class="stat-card__label">Avg Response</div>
          <div class="stat-card__value">{{ formatAvg(store.stats.avgResponseMinutes) }}</div>
        </div>
      </div>

      <!-- Filters -->
      <div class="filter-bar">
        <IconField style="flex:1; max-width:300px;">
          <InputIcon class="pi pi-search" />
          <InputText v-model="store.filters.search" placeholder="Search by name, email, or message..." style="width:100%" />
        </IconField>
        <Select
          v-model="store.filters.status"
          :options="statusFilterOptions"
          option-label="label"
          option-value="value"
          placeholder="All Statuses"
          show-clear
          style="min-width:150px"
        />
        <InputText v-model="store.filters.from" type="date" style="min-width:140px" />
        <InputText v-model="store.filters.to" type="date" style="min-width:140px" />
        <Button label="Clear" severity="secondary" outlined @click="store.clearFilters()" />
      </div>

      <!-- Layout -->
      <div class="inquiries__layout">
        <!-- List -->
        <div class="inquiry-list">
          <div v-if="store.loading" class="inquiry-list__empty">Loading...</div>
          <div v-else-if="store.inquiries.length === 0" class="inquiry-list__empty">No inquiries found</div>

          <div
            v-else
            v-for="inq in store.inquiries"
            :key="inq.id"
            class="inquiry-row"
            :class="{ 'inquiry-row--active': selected?.id === inq.id, 'inquiry-row--new': inq.status === 'New' }"
            @click="selectInquiry(inq)"
          >
            <div class="inquiry-row__left">
              <span class="inquiry-row__name">{{ inq.name }}</span>
              <span class="inquiry-row__email">{{ inq.email }}</span>
              <span class="inquiry-row__vehicle" v-if="inq.vehicleName">{{ inq.vehicleName }}</span>
            </div>
            <div class="inquiry-row__right">
              <Tag :value="statusLabel(inq.status)" :severity="statusSeverity[inq.status]" />
              <span class="inquiry-row__date">{{ formatDate(inq.createdAt) }}</span>
            </div>
          </div>
        </div>

        <!-- Detail panel -->
        <aside class="inquiry-detail" v-if="selected">
          <div class="inquiry-detail__header">
            <h3>{{ selected.name }}</h3>
            <div class="status-actions">
              <Button
                v-for="s in statusOptions"
                :key="s.value"
                :label="s.label"
                :severity="selected.status === s.value ? statusSeverity[s.value] as any : 'secondary'"
                :outlined="selected.status !== s.value"
                size="small"
                @click="setStatus(selected.id, s.value)"
              />
            </div>
          </div>

          <div class="inquiry-detail__meta">
            <span>✉ {{ selected.email }}</span>
            <span v-if="selected.phone">📞 {{ selected.phone }}</span>
            <span v-if="selected.vehicleName">🚗 {{ selected.vehicleName }}</span>
            <span>🕐 {{ formatDate(selected.createdAt) }}</span>
          </div>

          <div class="inquiry-detail__message">{{ selected.message }}</div>

          <div class="detail-actions">
            <Button
              label="Reply via Email"
              icon="pi pi-envelope"
              severity="warn"
              as="a"
              :href="`mailto:${selected.email}`"
            />
            <Button
              label="Convert to Order"
              icon="pi pi-plus-circle"
              severity="secondary"
              outlined
              @click="convertToOrder(selected)"
            />
            <Button
              v-if="selected.status === 'Resolved'"
              label="Delete"
              icon="pi pi-trash"
              severity="danger"
              outlined
              @click="deleteSelected"
            />
          </div>
        </aside>

        <aside class="inquiry-detail inquiry-detail--empty" v-else>
          <p>Select an inquiry to view details.</p>
        </aside>
      </div>
    </div>
  </AdminShell>
</template>

<style scoped>
.inquiries { display: flex; flex-direction: column; gap: 20px; }

.page-header h1 { font-size: 1.5rem; font-weight: 700; }
.page-header p { font-size: 0.875rem; color: #9a9a9a; margin-top: 4px; }

.stats-row {
  display: grid;
  grid-template-columns: repeat(5, 1fr);
  gap: 12px;
}
.stat-card {
  background: #111; border: 1px solid #222; border-radius: 10px;
  padding: 16px 20px; border-left: 4px solid #333;
}
.stat-card--new       { border-left-color: #3b82f6; }
.stat-card--inprogress { border-left-color: #f59e0b; }
.stat-card--resolved  { border-left-color: #22c55e; }
.stat-card__label { font-size: 0.75rem; color: #777; text-transform: uppercase; letter-spacing: 0.05em; margin-bottom: 8px; }
.stat-card__value { font-size: 1.75rem; font-weight: 700; color: #f0f0f0; }

.filter-bar {
  background: #111; border: 1px solid #222; border-radius: 10px;
  padding: 14px 18px; display: flex; align-items: center; gap: 12px; flex-wrap: wrap;
}

.inquiries__layout {
  display: grid;
  grid-template-columns: 1fr 340px;
  gap: 16px;
  align-items: start;
}

.inquiry-list {
  background: #111; border: 1px solid #222; border-radius: 10px; overflow: hidden;
}
.inquiry-list__empty {
  padding: 48px; text-align: center; color: #555; font-size: 0.9rem;
}

.inquiry-row {
  display: flex; align-items: center; justify-content: space-between;
  padding: 14px 18px; border-bottom: 1px solid #1a1a1a; cursor: pointer;
  transition: background 0.15s; gap: 12px;
}
.inquiry-row:last-child { border-bottom: none; }
.inquiry-row:hover { background: #161616; }
.inquiry-row--active { background: #1a1a1a !important; }
.inquiry-row--new .inquiry-row__name { font-weight: 700; }
.inquiry-row__left { display: flex; flex-direction: column; gap: 2px; min-width: 0; }
.inquiry-row__name { font-size: 0.9rem; color: #f0f0f0; white-space: nowrap; overflow: hidden; text-overflow: ellipsis; }
.inquiry-row__email { font-size: 0.775rem; color: #777; }
.inquiry-row__vehicle { font-size: 0.75rem; color: #e63946; }
.inquiry-row__right { display: flex; flex-direction: column; align-items: flex-end; gap: 4px; flex-shrink: 0; }
.inquiry-row__date { font-size: 0.725rem; color: #555; white-space: nowrap; }

.inquiry-detail {
  background: #111; border: 1px solid #222; border-radius: 10px;
  padding: 20px; position: sticky; top: 20px;
}
.inquiry-detail__header { display: flex; flex-direction: column; gap: 12px; margin-bottom: 16px; }
.inquiry-detail__header h3 { font-size: 1.05rem; font-weight: 700; }
.status-actions { display: flex; gap: 6px; flex-wrap: wrap; }

.inquiry-detail__meta {
  display: flex; flex-direction: column; gap: 6px; font-size: 0.8rem; color: #9a9a9a;
  margin-bottom: 16px; padding-bottom: 16px; border-bottom: 1px solid #1a1a1a;
}
.inquiry-detail__message {
  font-size: 0.875rem; line-height: 1.7; white-space: pre-wrap; color: #d0d0d0; margin-bottom: 16px;
  word-break: break-word; overflow-wrap: break-word;
}
.inquiry-detail--empty {
  color: #555; font-size: 0.875rem; display: flex; align-items: center;
  justify-content: center; min-height: 120px;
}
.detail-actions { display: flex; gap: 10px; flex-wrap: wrap; }
</style>
