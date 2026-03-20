<script setup lang="ts">
import { ref, computed, watch, onMounted } from 'vue'
import { useAppointmentStore } from '@/stores/appointment.store'
import AdminShell from '@/components/layout/AdminShell.vue'
import type { Appointment, CreateAppointmentDto, UpdateAppointmentDto, AppointmentStatus } from '@/types/appointment.types'

const API_URL = import.meta.env.VITE_API_URL as string

// ── Customer search ──────────────────────────────────────────────
interface UserResult { id: number; firstName: string | null; lastName: string | null; email: string; phone: string | null }
const userQuery = ref('')
const userResults = ref<UserResult[]>([])
const userSearchOpen = ref(false)
const selectedUser = ref<UserResult | null>(null)
let userSearchTimer: number

async function searchUsers(q: string) {
  if (!q.trim()) { userResults.value = []; return }
  const token = localStorage.getItem('mpp_token')
  const res = await fetch(`${API_URL}/users/search?q=${encodeURIComponent(q)}`, {
    headers: { Authorization: `Bearer ${token}` },
  })
  userResults.value = await res.json()
  userSearchOpen.value = true
}

function onUserInput() {
  clearTimeout(userSearchTimer)
  selectedUser.value = null
  form.value.userId = null
  userSearchTimer = setTimeout(() => searchUsers(userQuery.value), 250)
}

function fullName(u: UserResult): string {
  return [u.firstName, u.lastName].filter(Boolean).join(' ') || u.email
}

function pickUser(u: UserResult) {
  selectedUser.value = u
  form.value.userId = u.id
  form.value.customerFirstName = u.firstName ?? ''
  form.value.customerLastName = u.lastName ?? ''
  form.value.customerEmail = u.email
  form.value.customerPhone = u.phone ?? ''
  userQuery.value = fullName(u)
  userResults.value = []
  userSearchOpen.value = false
}

function clearUser() {
  selectedUser.value = null
  form.value.userId = null
  form.value.customerFirstName = ''
  form.value.customerLastName = ''
  userQuery.value = ''
  userResults.value = []
}

const store = useAppointmentStore()

// ── Calendar state ──────────────────────────────────────────────
const today = new Date()
const viewMode = ref<'month' | 'week'>('month')
const currentDate = ref(new Date(today.getFullYear(), today.getMonth(), 1))

// ── Modal state ─────────────────────────────────────────────────
const showModal = ref(false)
const editingId = ref<number | null>(null)
const modalError = ref('')
const confirmDelete = ref(false)

const form = ref({
  title: '',
  userId: null as number | null,
  customerFirstName: '',
  customerLastName: '',
  customerEmail: '',
  customerPhone: '',
  vehicleId: null as number | null,
  date: '',
  startHour: '09',
  startMin: '00',
  endHour: '10',
  endMin: '00',
  status: 'Scheduled' as AppointmentStatus,
  notes: '',
})

// ── Date range for fetch ─────────────────────────────────────────
const fetchRange = computed(() => {
  if (viewMode.value === 'month') {
    const y = currentDate.value.getFullYear()
    const m = currentDate.value.getMonth()
    const from = new Date(y, m, 1)
    const to = new Date(y, m + 1, 1)
    return { from, to }
  } else {
    const from = startOfWeek(currentDate.value)
    const to = new Date(from)
    to.setDate(to.getDate() + 7)
    return { from, to }
  }
})

watch(fetchRange, loadAppointments, { immediate: true })
onMounted(loadAppointments)

async function loadAppointments() {
  const { from, to } = fetchRange.value
  await store.fetchAppointments(from.toISOString(), to.toISOString())
}

// ── Navigation ───────────────────────────────────────────────────
function prev() {
  if (viewMode.value === 'month') {
    currentDate.value = new Date(currentDate.value.getFullYear(), currentDate.value.getMonth() - 1, 1)
  } else {
    const d = new Date(currentDate.value)
    d.setDate(d.getDate() - 7)
    currentDate.value = d
  }
}
function next() {
  if (viewMode.value === 'month') {
    currentDate.value = new Date(currentDate.value.getFullYear(), currentDate.value.getMonth() + 1, 1)
  } else {
    const d = new Date(currentDate.value)
    d.setDate(d.getDate() + 7)
    currentDate.value = d
  }
}
function goToday() {
  currentDate.value = viewMode.value === 'month'
    ? new Date(today.getFullYear(), today.getMonth(), 1)
    : startOfWeek(today)
}

// ── Month grid ───────────────────────────────────────────────────
const monthDays = computed(() => {
  const y = currentDate.value.getFullYear()
  const m = currentDate.value.getMonth()
  const firstDay = new Date(y, m, 1)
  const lastDay = new Date(y, m + 1, 0)

  // pad start (Sunday = 0)
  const days: (Date | null)[] = []
  for (let i = 0; i < firstDay.getDay(); i++) days.push(null)
  for (let d = 1; d <= lastDay.getDate(); d++) days.push(new Date(y, m, d))
  // pad end to complete grid row
  while (days.length % 7 !== 0) days.push(null)
  return days
})

function appointmentsForDay(date: Date) {
  const key = dateKey(date)
  return store.appointments.filter(a => dateKey(new Date(a.startTime)) === key)
    .sort((a, b) => new Date(a.startTime).getTime() - new Date(b.startTime).getTime())
}

// ── Week grid ────────────────────────────────────────────────────
const HOURS = Array.from({ length: 14 }, (_, i) => i + 7) // 7am–8pm

const weekDays = computed(() => {
  const start = startOfWeek(currentDate.value)
  return Array.from({ length: 7 }, (_, i) => {
    const d = new Date(start)
    d.setDate(d.getDate() + i)
    return d
  })
})

function appointmentsForWeekDay(date: Date) {
  const key = dateKey(date)
  return store.appointments.filter(a => dateKey(new Date(a.startTime)) === key)
}

function apptTop(appt: Appointment): number {
  const start = new Date(appt.startTime)
  return ((start.getHours() - 7) * 60 + start.getMinutes()) * (56 / 60)
}
function apptHeight(appt: Appointment): number {
  const diff = (new Date(appt.endTime).getTime() - new Date(appt.startTime).getTime()) / 60000
  return Math.max(diff * (56 / 60), 24)
}

// ── Header label ─────────────────────────────────────────────────
const headerLabel = computed(() => {
  if (viewMode.value === 'month') {
    return currentDate.value.toLocaleString('en-US', { month: 'long', year: 'numeric' })
  }
  const start = startOfWeek(currentDate.value)
  const end = new Date(start)
  end.setDate(end.getDate() + 6)
  const opts: Intl.DateTimeFormatOptions = { month: 'short', day: 'numeric' }
  return `${start.toLocaleDateString('en-US', opts)} – ${end.toLocaleDateString('en-US', { ...opts, year: 'numeric' })}`
})

// ── Modal helpers ────────────────────────────────────────────────
function openCreate(date?: Date, hour?: number) {
  editingId.value = null
  modalError.value = ''
  confirmDelete.value = false
  const d = date ?? today
  selectedUser.value = null
  userQuery.value = ''
  userResults.value = []
  form.value = {
    title: '',
    userId: null,
    customerFirstName: '',
    customerLastName: '',
    customerEmail: '',
    customerPhone: '',
    vehicleId: null,
    date: toInputDate(d),
    startHour: String(hour ?? 9).padStart(2, '0'),
    startMin: '00',
    endHour: String((hour ?? 9) + 1).padStart(2, '0'),
    endMin: '00',
    status: 'Scheduled',
    notes: '',
  }
  showModal.value = true
}

function openEdit(appt: Appointment) {
  editingId.value = appt.id
  modalError.value = ''
  confirmDelete.value = false
  selectedUser.value = appt.userId ? { id: appt.userId, firstName: appt.userName ?? null, lastName: null, email: appt.customerEmail, phone: appt.customerPhone } : null
  userQuery.value = appt.userId ? (appt.userName ?? appt.customerEmail) : ''
  userResults.value = []
  const start = new Date(appt.startTime)
  const end = new Date(appt.endTime)
  // Split stored customerName back into first/last for editing
  const nameParts = (appt.customerName ?? '').split(' ')
  const customerFirstName = nameParts[0] ?? ''
  const customerLastName = nameParts.slice(1).join(' ')
  form.value = {
    title: appt.title,
    userId: appt.userId,
    customerFirstName,
    customerLastName,
    customerEmail: appt.customerEmail,
    customerPhone: appt.customerPhone,
    vehicleId: appt.vehicleId,
    date: toInputDate(start),
    startHour: String(start.getHours()).padStart(2, '0'),
    startMin: String(start.getMinutes()).padStart(2, '0'),
    endHour: String(end.getHours()).padStart(2, '0'),
    endMin: String(end.getMinutes()).padStart(2, '0'),
    status: appt.status,
    notes: appt.notes ?? '',
  }
  showModal.value = true
}

function closeModal() {
  showModal.value = false
  confirmDelete.value = false
}

async function submitModal() {
  modalError.value = ''
  if (!form.value.title.trim()) { modalError.value = 'Title is required.'; return }
  if (!form.value.customerFirstName.trim()) { modalError.value = 'First name is required.'; return }
  if (!form.value.date) { modalError.value = 'Date is required.'; return }

  const startTime = `${form.value.date}T${form.value.startHour}:${form.value.startMin}:00`
  const endTime = `${form.value.date}T${form.value.endHour}:${form.value.endMin}:00`

  if (endTime <= startTime) { modalError.value = 'End time must be after start time.'; return }

  const customerName = [form.value.customerFirstName.trim(), form.value.customerLastName.trim()].filter(Boolean).join(' ')

  if (editingId.value) {
    const dto: UpdateAppointmentDto = {
      title: form.value.title,
      customerName,
      customerEmail: form.value.customerEmail,
      customerPhone: form.value.customerPhone,
      userId: form.value.userId,
      vehicleId: form.value.vehicleId,
      startTime,
      endTime,
      status: form.value.status,
      notes: form.value.notes || null,
    }
    const { error } = await store.updateAppointment(editingId.value, dto)
    if (error) { modalError.value = error; return }
  } else {
    const dto: CreateAppointmentDto = {
      title: form.value.title,
      customerName,
      customerEmail: form.value.customerEmail,
      customerPhone: form.value.customerPhone,
      userId: form.value.userId,
      vehicleId: form.value.vehicleId,
      startTime,
      endTime,
      notes: form.value.notes || null,
    }
    const { error } = await store.createAppointment(dto)
    if (error) { modalError.value = error; return }
  }
  closeModal()
}

async function doDelete() {
  if (!editingId.value) return
  await store.deleteAppointment(editingId.value)
  closeModal()
}

// ── Utilities ────────────────────────────────────────────────────
function startOfWeek(d: Date): Date {
  const result = new Date(d)
  result.setDate(result.getDate() - result.getDay())
  result.setHours(0, 0, 0, 0)
  return result
}

function dateKey(d: Date): string {
  return `${d.getFullYear()}-${d.getMonth()}-${d.getDate()}`
}

function toInputDate(d: Date): string {
  return `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}-${String(d.getDate()).padStart(2, '0')}`
}

function isToday(d: Date): boolean {
  return dateKey(d) === dateKey(today)
}

function formatTime(iso: string): string {
  const d = new Date(iso)
  return d.toLocaleTimeString('en-US', { hour: 'numeric', minute: '2-digit', hour12: true })
}

function formatHour(h: number): string {
  return h === 12 ? '12 PM' : h < 12 ? `${h} AM` : `${h - 12} PM`
}

const statusColor: Record<string, string> = {
  Scheduled: '#3b82f6',
  Completed: '#22c55e',
  Cancelled: '#ef4444',
  NoShow: '#f59e0b',
}

const HOURS_SELECT = Array.from({ length: 24 }, (_, i) => String(i).padStart(2, '0'))
const MINS_SELECT = ['00', '15', '30', '45']
const { setTimeout } = window
</script>

<template>
  <AdminShell>
    <div class="calendar-page">
      <!-- Header -->
      <div class="cal-header">
        <div class="cal-header__left">
          <h2>Calendar</h2>
          <button class="today-btn" @click="goToday">Today</button>
          <div class="nav-btns">
            <button class="nav-btn" @click="prev"><i class="pi pi-chevron-left" /></button>
            <button class="nav-btn" @click="next"><i class="pi pi-chevron-right" /></button>
          </div>
          <span class="cal-header__label">{{ headerLabel }}</span>
        </div>
        <div class="cal-header__right">
          <div class="view-toggle">
            <button :class="{ active: viewMode === 'month' }" @click="viewMode = 'month'">Month</button>
            <button :class="{ active: viewMode === 'week' }" @click="viewMode = 'week'">Week</button>
          </div>
          <button class="btn-primary" @click="openCreate()">
            <i class="pi pi-plus" /> New Appointment
          </button>
        </div>
      </div>

      <!-- Month view -->
      <div v-if="viewMode === 'month'" class="month-grid">
        <div class="month-grid__dow" v-for="d in ['Sun','Mon','Tue','Wed','Thu','Fri','Sat']" :key="d">{{ d }}</div>

        <div
          v-for="(day, i) in monthDays"
          :key="i"
          class="month-cell"
          :class="{
            'month-cell--empty': !day,
            'month-cell--today': day && isToday(day)
          }"
          @click="day && openCreate(day)"
        >
          <template v-if="day">
            <div class="month-cell__num">{{ day.getDate() }}</div>
            <div class="month-cell__events">
              <div
                v-for="appt in appointmentsForDay(day).slice(0, 3)"
                :key="appt.id"
                class="month-event"
                :style="{ '--c': statusColor[appt.status] }"
                @click.stop="openEdit(appt)"
              >
                {{ formatTime(appt.startTime) }} {{ appt.title }}
              </div>
              <div
                v-if="appointmentsForDay(day).length > 3"
                class="month-event month-event--more"
                @click.stop
              >
                +{{ appointmentsForDay(day).length - 3 }} more
              </div>
            </div>
          </template>
        </div>
      </div>

      <!-- Week view -->
      <div v-else class="week-view">
        <!-- Day headers -->
        <div class="week-header">
          <div class="week-time-gutter" />
          <div
            v-for="day in weekDays"
            :key="day.toISOString()"
            class="week-day-header"
            :class="{ 'week-day-header--today': isToday(day) }"
          >
            <span class="week-day-header__dow">{{ day.toLocaleString('en-US', { weekday: 'short' }) }}</span>
            <span class="week-day-header__num" :class="{ today: isToday(day) }">{{ day.getDate() }}</span>
          </div>
        </div>

        <!-- Time grid -->
        <div class="week-body">
          <!-- Hour rows -->
          <div class="week-rows">
            <div v-for="h in HOURS" :key="h" class="week-hour-row">
              <div class="week-time-label">{{ formatHour(h) }}</div>
              <div
                v-for="day in weekDays"
                :key="day.toISOString()"
                class="week-slot"
                @click="openCreate(day, h)"
              />
            </div>
          </div>

          <!-- Appointment overlays per day -->
          <div class="week-events">
            <div class="week-events-gutter" />
            <div
              v-for="day in weekDays"
              :key="day.toISOString()"
              class="week-events-col"
            >
              <div
                v-for="appt in appointmentsForWeekDay(day)"
                :key="appt.id"
                class="week-event"
                :style="{
                  top: `${apptTop(appt)}px`,
                  height: `${apptHeight(appt)}px`,
                  '--c': statusColor[appt.status]
                }"
                @click.stop="openEdit(appt)"
              >
                <div class="week-event__title">{{ appt.title }}</div>
                <div class="week-event__time">{{ formatTime(appt.startTime) }} – {{ formatTime(appt.endTime) }}</div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- Modal -->
    <Teleport to="body">
      <div v-if="showModal" class="modal-backdrop" @click.self="closeModal">
        <div class="modal">
          <div class="modal__header">
            <h3>{{ editingId ? 'Edit Appointment' : 'New Appointment' }}</h3>
            <button class="modal__close" @click="closeModal"><i class="pi pi-times" /></button>
          </div>

          <div class="modal__body">
            <div class="form-row">
              <div class="form-group form-group--full">
                <label>Title <span class="req">*</span></label>
                <input v-model="form.title" type="text" class="form-input" placeholder="e.g. Test Drive – 2024 RZR" />
              </div>
            </div>

            <!-- Customer selector -->
            <div class="form-row">
              <div class="form-group form-group--full">
                <label>Registered Customer <span class="form-hint">(optional — search to link account)</span></label>
                <div class="customer-search">
                  <div class="customer-search__input-wrap">
                    <i class="pi pi-search customer-search__icon" />
                    <input
                      v-model="userQuery"
                      type="text"
                      class="form-input customer-search__input"
                      placeholder="Search registered customers by name or email…"
                      @input="onUserInput"
                      @focus="userQuery && (userSearchOpen = true)"
                      @blur="setTimeout(() => userSearchOpen = false, 150)"
                    />
                    <button v-if="selectedUser" class="customer-search__clear" @click="clearUser" title="Clear">
                      <i class="pi pi-times" />
                    </button>
                  </div>
                  <!-- Linked badge -->
                  <div v-if="selectedUser" class="customer-linked">
                    <i class="pi pi-link" />
                    Linked to registered account
                  </div>
                  <!-- Dropdown results -->
                  <div v-if="userSearchOpen && userResults.length > 0" class="customer-dropdown">
                    <button
                      v-for="u in userResults"
                      :key="u.id"
                      class="customer-dropdown__item"
                      @mousedown.prevent="pickUser(u)"
                    >
                      <span class="customer-dropdown__name">{{ fullName(u) }}</span>
                      <span class="customer-dropdown__email">{{ u.email }}</span>
                    </button>
                  </div>
                  <div v-else-if="userSearchOpen && userQuery.length > 0 && userResults.length === 0" class="customer-dropdown">
                    <div class="customer-dropdown__empty">No registered customers found</div>
                  </div>
                </div>
              </div>
            </div>
            <!-- Manual customer info — always visible for guest or pre-filled from registered account -->
            <div class="form-row">
              <div class="form-group">
                <label>First Name <span class="req">*</span></label>
                <input v-model="form.customerFirstName" type="text" class="form-input" placeholder="John" />
              </div>
              <div class="form-group">
                <label>Last Name</label>
                <input v-model="form.customerLastName" type="text" class="form-input" placeholder="Doe" />
              </div>
            </div>
            <div class="form-row">
              <div class="form-group">
                <label>Email</label>
                <input v-model="form.customerEmail" type="email" class="form-input" placeholder="john@example.com" />
              </div>
              <div class="form-group">
                <label>Phone</label>
                <input v-model="form.customerPhone" type="text" class="form-input" placeholder="(555) 123-4567" />
              </div>
            </div>

            <div class="form-row">
              <div class="form-group">
                <label>Date <span class="req">*</span></label>
                <input v-model="form.date" type="date" class="form-input" />
              </div>
              <div class="form-group">
                <label>Start Time</label>
                <div class="time-pick">
                  <select v-model="form.startHour" class="form-select">
                    <option v-for="h in HOURS_SELECT" :key="h" :value="h">{{ h }}</option>
                  </select>
                  <span>:</span>
                  <select v-model="form.startMin" class="form-select form-select--min">
                    <option v-for="m in MINS_SELECT" :key="m" :value="m">{{ m }}</option>
                  </select>
                </div>
              </div>
              <div class="form-group">
                <label>End Time</label>
                <div class="time-pick">
                  <select v-model="form.endHour" class="form-select">
                    <option v-for="h in HOURS_SELECT" :key="h" :value="h">{{ h }}</option>
                  </select>
                  <span>:</span>
                  <select v-model="form.endMin" class="form-select form-select--min">
                    <option v-for="m in MINS_SELECT" :key="m" :value="m">{{ m }}</option>
                  </select>
                </div>
              </div>
            </div>

            <div class="form-row" v-if="editingId">
              <div class="form-group">
                <label>Status</label>
                <select v-model="form.status" class="form-select form-select--full">
                  <option value="Scheduled">Scheduled</option>
                  <option value="Completed">Completed</option>
                  <option value="Cancelled">Cancelled</option>
                  <option value="NoShow">No Show</option>
                </select>
              </div>
            </div>

            <div class="form-row">
              <div class="form-group form-group--full">
                <label>Notes</label>
                <textarea v-model="form.notes" class="form-input form-textarea" rows="3" placeholder="Optional notes..." />
              </div>
            </div>

            <div class="modal-error" v-if="modalError">{{ modalError }}</div>
          </div>

          <div class="modal__footer">
            <div class="modal__footer-left">
              <template v-if="editingId">
                <button v-if="!confirmDelete" class="btn-danger-outline" @click="confirmDelete = true">Delete</button>
                <template v-else>
                  <span class="confirm-text">Are you sure?</span>
                  <button class="btn-danger" @click="doDelete">Yes, delete</button>
                  <button class="btn-ghost" @click="confirmDelete = false">Cancel</button>
                </template>
              </template>
            </div>
            <div class="modal__footer-right">
              <button class="btn-ghost" @click="closeModal">Cancel</button>
              <button class="btn-primary" :disabled="store.saving" @click="submitModal">
                <i class="pi pi-spin pi-spinner" v-if="store.saving" />
                {{ editingId ? 'Save Changes' : 'Create' }}
              </button>
            </div>
          </div>
        </div>
      </div>
    </Teleport>
  </AdminShell>
</template>

<style scoped>
.calendar-page { display: flex; flex-direction: column; gap: 16px; height: 100%; }

/* Header */
.cal-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  flex-wrap: wrap;
  gap: 12px;
}
.cal-header__left { display: flex; align-items: center; gap: 12px; }
.cal-header__right { display: flex; align-items: center; gap: 12px; }
.cal-header__label { font-size: 1.1rem; font-weight: 700; color: #f0f0f0; }

.today-btn {
  padding: 6px 14px;
  background: transparent;
  border: 1px solid #2a2a2a;
  border-radius: 6px;
  color: #9a9a9a;
  font-size: 0.825rem;
  cursor: pointer;
}
.today-btn:hover { border-color: #555; color: #f0f0f0; }

.nav-btns { display: flex; gap: 4px; }
.nav-btn {
  width: 30px; height: 30px;
  background: transparent;
  border: 1px solid #2a2a2a;
  border-radius: 6px;
  color: #9a9a9a;
  cursor: pointer;
  display: flex; align-items: center; justify-content: center;
  font-size: 0.75rem;
}
.nav-btn:hover { border-color: #555; color: #f0f0f0; }

.view-toggle {
  display: flex;
  background: #0d0d0d;
  border: 1px solid #2a2a2a;
  border-radius: 6px;
  overflow: hidden;
}
.view-toggle button {
  padding: 6px 14px;
  background: transparent;
  border: none;
  color: #777;
  font-size: 0.825rem;
  cursor: pointer;
}
.view-toggle button.active {
  background: #1e1e1e;
  color: #f0f0f0;
}

.btn-primary {
  display: flex; align-items: center; gap: 8px;
  padding: 8px 16px;
  background: #e63946;
  color: #fff;
  border: none;
  border-radius: 6px;
  font-size: 0.875rem;
  font-weight: 600;
  cursor: pointer;
}
.btn-primary:hover { opacity: 0.85; }
.btn-primary:disabled { opacity: 0.5; cursor: not-allowed; }

/* Month grid */
.month-grid {
  display: grid;
  grid-template-columns: repeat(7, 1fr);
  border: 1px solid #222;
  border-radius: 10px;
  overflow: hidden;
  flex: 1;
}
.month-grid__dow {
  padding: 8px;
  text-align: center;
  font-size: 0.7rem;
  font-weight: 700;
  color: #555;
  text-transform: uppercase;
  letter-spacing: 0.05em;
  background: #0d0d0d;
  border-bottom: 1px solid #1a1a1a;
}
.month-cell {
  min-height: 110px;
  border-right: 1px solid #1a1a1a;
  border-bottom: 1px solid #1a1a1a;
  padding: 6px;
  cursor: pointer;
  transition: background 0.1s;
  position: relative;
  overflow: hidden;
}
.month-cell:nth-child(7n) { border-right: none; }
.month-cell:hover { background: #141414; }
.month-cell--empty { background: #0a0a0a; cursor: default; }
.month-cell--today { background: color-mix(in srgb, #e63946 6%, transparent); }
.month-cell--today .month-cell__num {
  background: #e63946;
  color: #fff;
  border-radius: 50%;
  width: 22px;
  height: 22px;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 0.75rem;
}
.month-cell__num {
  font-size: 0.8rem;
  color: #777;
  margin-bottom: 4px;
  width: 22px;
  height: 22px;
  display: flex;
  align-items: center;
  justify-content: center;
}
.month-cell__events { display: flex; flex-direction: column; gap: 2px; }
.month-event {
  font-size: 0.7rem;
  padding: 2px 6px;
  border-radius: 4px;
  background: color-mix(in srgb, var(--c) 20%, transparent);
  color: var(--c);
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
  cursor: pointer;
}
.month-event:hover { filter: brightness(1.2); }
.month-event--more { color: #555; background: transparent; font-style: italic; }

/* Week view */
.week-view {
  display: flex;
  flex-direction: column;
  background: #111;
  border: 1px solid #222;
  border-radius: 10px;
  overflow: hidden;
  flex: 1;
}
.week-header {
  display: flex;
  border-bottom: 1px solid #1a1a1a;
  background: #0d0d0d;
}
.week-time-gutter {
  width: 60px;
  flex-shrink: 0;
  border-right: 1px solid #1a1a1a;
}
.week-day-header {
  flex: 1;
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 4px;
  padding: 8px 0;
  border-right: 1px solid #1a1a1a;
}
.week-day-header:last-child { border-right: none; }
.week-day-header__dow { font-size: 0.65rem; color: #555; text-transform: uppercase; letter-spacing: 0.05em; }
.week-day-header__num {
  font-size: 1rem;
  font-weight: 700;
  color: #9a9a9a;
  width: 30px; height: 30px;
  display: flex; align-items: center; justify-content: center;
  border-radius: 50%;
}
.week-day-header__num.today {
  background: #e63946;
  color: #fff;
}

.week-body {
  position: relative;
  overflow-y: auto;
  max-height: calc(100vh - 280px);
}

.week-rows {
  display: flex;
  flex-direction: column;
}
.week-hour-row {
  display: flex;
  height: 56px;
  border-bottom: 1px solid #181818;
}
.week-time-label {
  width: 60px;
  flex-shrink: 0;
  font-size: 0.65rem;
  color: #444;
  text-align: right;
  padding-right: 8px;
  padding-top: 4px;
  border-right: 1px solid #1a1a1a;
}
.week-slot {
  flex: 1;
  border-right: 1px solid #181818;
  cursor: pointer;
}
.week-slot:last-child { border-right: none; }
.week-slot:hover { background: #151515; }

/* Event overlays */
.week-events {
  position: absolute;
  top: 0; left: 0; right: 0;
  pointer-events: none;
  display: flex;
}
.week-events-gutter { width: 60px; flex-shrink: 0; }
.week-events-col { flex: 1; position: relative; }

.week-event {
  position: absolute;
  left: 2px; right: 2px;
  border-radius: 6px;
  background: color-mix(in srgb, var(--c) 20%, #111);
  border-left: 3px solid var(--c);
  padding: 3px 6px;
  cursor: pointer;
  pointer-events: all;
  overflow: hidden;
  z-index: 1;
}
.week-event:hover { filter: brightness(1.15); }
.week-event__title { font-size: 0.725rem; font-weight: 700; color: var(--c); white-space: nowrap; overflow: hidden; text-overflow: ellipsis; }
.week-event__time { font-size: 0.65rem; color: color-mix(in srgb, var(--c) 70%, #fff); }

/* Modal */
.modal-backdrop {
  position: fixed;
  inset: 0;
  background: rgba(0,0,0,0.6);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 1000;
  padding: 20px;
}
.modal {
  background: #111;
  border: 1px solid #222;
  border-radius: 12px;
  width: 100%;
  max-width: 600px;
  max-height: 90vh;
  display: flex;
  flex-direction: column;
  overflow: hidden;
}
.modal__header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 18px 20px;
  border-bottom: 1px solid #1a1a1a;
}
.modal__header h3 { font-size: 1rem; font-weight: 700; }
.modal__close {
  background: transparent;
  border: none;
  color: #555;
  cursor: pointer;
  font-size: 1rem;
}
.modal__close:hover { color: #f0f0f0; }
.modal__body {
  padding: 20px;
  display: flex;
  flex-direction: column;
  gap: 14px;
  overflow-y: auto;
}
.modal__footer {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 16px 20px;
  border-top: 1px solid #1a1a1a;
}
.modal__footer-left { display: flex; align-items: center; gap: 8px; }
.modal__footer-right { display: flex; align-items: center; gap: 8px; }

/* Form */
.form-row { display: grid; grid-template-columns: repeat(3, 1fr); gap: 12px; }
.form-group { display: flex; flex-direction: column; gap: 5px; }
.form-group--full { grid-column: 1 / -1; }
.form-group label { font-size: 0.775rem; color: #777; font-weight: 500; }
.req { color: #e63946; }
.form-hint { color: #555; font-size: 0.7rem; font-weight: 400; }
.form-input {
  background: #0d0d0d;
  border: 1px solid #2a2a2a;
  border-radius: 6px;
  padding: 8px 10px;
  color: #f0f0f0;
  font-size: 0.85rem;
  width: 100%;
  box-sizing: border-box;
}
.form-input:focus { outline: none; border-color: #e63946; }
.form-select {
  background: #0d0d0d;
  border: 1px solid #2a2a2a;
  border-radius: 6px;
  padding: 8px 10px;
  color: #f0f0f0;
  font-size: 0.85rem;
  cursor: pointer;
}
.form-select--full { width: 100%; }
.form-select--min { width: 70px; }
.form-textarea { resize: vertical; }
.time-pick { display: flex; align-items: center; gap: 6px; color: #555; }

.modal-error {
  padding: 10px 14px;
  background: color-mix(in srgb, #ef4444 15%, transparent);
  border: 1px solid color-mix(in srgb, #ef4444 40%, transparent);
  border-radius: 6px;
  color: #ef4444;
  font-size: 0.825rem;
}

.confirm-text { font-size: 0.8rem; color: #f0f0f0; }
.btn-ghost {
  padding: 8px 14px;
  background: transparent;
  border: 1px solid #2a2a2a;
  border-radius: 6px;
  color: #9a9a9a;
  font-size: 0.85rem;
  cursor: pointer;
}
.btn-ghost:hover { border-color: #555; color: #f0f0f0; }
.btn-danger-outline {
  padding: 8px 14px;
  background: transparent;
  border: 1px solid color-mix(in srgb, #ef4444 50%, transparent);
  border-radius: 6px;
  color: #ef4444;
  font-size: 0.85rem;
  cursor: pointer;
}
.btn-danger-outline:hover { background: color-mix(in srgb, #ef4444 10%, transparent); }
.btn-danger {
  padding: 8px 14px;
  background: #ef4444;
  border: none;
  border-radius: 6px;
  color: #fff;
  font-size: 0.85rem;
  font-weight: 600;
  cursor: pointer;
}
.btn-danger:hover { opacity: 0.85; }

/* Customer search */
.customer-search { position: relative; display: flex; flex-direction: column; gap: 6px; }
.customer-search__input-wrap { position: relative; display: flex; align-items: center; }
.customer-search__icon {
  position: absolute; left: 10px; color: #555; font-size: 0.8rem; pointer-events: none;
}
.customer-search__input { padding-left: 30px !important; padding-right: 32px !important; }
.customer-search__clear {
  position: absolute; right: 8px;
  background: none; border: none; color: #555; cursor: pointer; font-size: 0.75rem;
  display: flex; align-items: center; padding: 4px;
}
.customer-search__clear:hover { color: #ef4444; }

.customer-linked {
  display: flex; align-items: center; gap: 5px;
  font-size: 0.72rem; color: #22c55e; font-weight: 600;
}
.customer-linked .pi { font-size: 0.7rem; }

.customer-dropdown {
  position: absolute;
  top: calc(100% + 4px);
  left: 0; right: 0;
  background: #0d0d0d;
  border: 1px solid #2a2a2a;
  border-radius: 8px;
  z-index: 200;
  overflow: hidden;
  box-shadow: 0 8px 24px rgba(0,0,0,0.5);
  max-height: 200px;
  overflow-y: auto;
}
.customer-dropdown__item {
  display: flex; flex-direction: column; gap: 2px;
  width: 100%; text-align: left;
  padding: 10px 14px;
  background: none; border: none; border-bottom: 1px solid #1a1a1a;
  cursor: pointer;
}
.customer-dropdown__item:last-child { border-bottom: none; }
.customer-dropdown__item:hover { background: #161616; }
.customer-dropdown__name { font-size: 0.85rem; color: #f0f0f0; font-weight: 500; }
.customer-dropdown__email { font-size: 0.75rem; color: #666; }
.customer-dropdown__empty { padding: 12px 14px; font-size: 0.8rem; color: #555; text-align: center; }
</style>
