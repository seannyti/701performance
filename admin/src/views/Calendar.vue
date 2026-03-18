<template>
  <AdminLayout>
    <div class="calendar-container">
      <div class="calendar-header">
        <div>
          <h1>📅 Calendar</h1>
          <p class="subtitle">Manage your schedule and appointments</p>
        </div>
        <button @click="openNewAppointmentModal" class="btn btn-primary">
          <span class="btn-icon">+</span> New Appointment
        </button>
      </div>

      <div class="calendar-card">
        <!-- Loading Spinner -->
        <div v-if="isLoading" class="loading-overlay">
          <div class="spinner"></div>
          <p>Loading appointments...</p>
        </div>

        <!-- Calendar Navigation -->
        <div class="calendar-nav">
          <button @click="previousMonth" class="nav-btn">
            <span>←</span>
          </button>
          <h2 class="current-month">{{ currentMonthYear }}</h2>
          <button @click="nextMonth" class="nav-btn">
            <span>→</span>
          </button>
        </div>

        <!-- Calendar Grid -->
        <div class="calendar-grid">
          <!-- Day Headers -->
          <div v-for="day in weekDays" :key="day" class="day-header">
            {{ day }}
          </div>

          <!-- Calendar Days -->
          <div
            v-for="(day, index) in calendarDays"
            :key="index"
            class="calendar-day"
            :class="{
              'other-month': day.isOtherMonth,
              'today': day.isToday,
              'has-events': day.hasEvents,
              'selected': selectedDay && day.fullDate.toDateString() === selectedDay.fullDate.toDateString()
            }"
            @click="selectDay(day)"
            @mouseenter="hoveredDay = day"
            @mouseleave="hoveredDay = null"
          >
            <span class="day-number">{{ day.date }}</span>
            <div v-if="day.eventCount > 0" class="event-count">{{ day.eventCount }}</div>
            
            <!-- Hover Tooltip -->
            <div v-if="hoveredDay && hoveredDay.fullDate.toDateString() === day.fullDate.toDateString() && day.hasEvents && !day.isOtherMonth" class="appointment-tooltip">
              <div class="tooltip-header">{{ formatTooltipDate(day) }}</div>
              <div class="tooltip-appointments">
                <div v-for="apt in getAppointmentsForDay(day)" :key="apt.id" class="tooltip-apt">
                  <span class="tooltip-time">{{ formatTime(apt.startTime) }}</span>
                  <span class="tooltip-customer">{{ apt.customerName }}</span>
                </div>
              </div>
            </div>
          </div>
        </div>

        <!-- Selected Day Info -->
        <div v-if="selectedDay" class="day-info">
          <div class="day-info-header">
            <h3>{{ formatSelectedDate(selectedDay) }}</h3>
            <button @click="openNewAppointmentModal" class="btn btn-sm btn-primary">
              + Add Appointment
            </button>
          </div>

          <!-- Appointments List -->
          <div v-if="dayAppointments.length > 0" class="appointments-list">
            <div
              v-for="appointment in dayAppointments"
              :key="appointment.id"
              class="appointment-card"
              :class="`status-${appointment.status.toLowerCase()}`"
            >
              <div class="appointment-header">
                <div class="appointment-time">
                  <span class="time-icon">🕐</span>
                  {{ formatTime(appointment.startTime) }} - {{ formatTime(appointment.endTime) }}
                </div>
                <span class="status-badge" :class="`status-${appointment.status.toLowerCase()}`">
                  {{ appointment.status }}
                </span>
              </div>
              <div class="appointment-body">
                <div class="customer-info">
                  <strong>{{ appointment.customerName }}</strong>
                  <div class="contact-info">
                    <span v-if="appointment.customerEmail">📧 {{ appointment.customerEmail }}</span>
                    <span v-if="appointment.customerPhone">📞 {{ appointment.customerPhone }}</span>
                  </div>
                </div>
                <div v-if="appointment.serviceType" class="service-type">
                  🔧 {{ appointment.serviceType }}
                </div>
                <div v-if="appointment.notes" class="notes">
                  💬 {{ appointment.notes }}
                </div>
              </div>
              <div class="appointment-actions">
                <button
                  v-if="appointment.status === 'Scheduled'"
                  @click="markAsComplete(appointment.id)"
                  class="btn btn-xs btn-success"
                  title="Mark as complete"
                  :disabled="isActionLoading(appointment.id)"
                >
                  <span v-if="isActionLoading(appointment.id)" class="btn-spinner"></span>
                  <span v-else>✓</span> Complete
                </button>
                <button
                  @click="editAppointment(appointment)"
                  class="btn btn-xs btn-secondary"
                  title="Edit appointment"
                  :disabled="isActionLoading(appointment.id)"
                >
                  ✏️ Edit
                </button>
                <button
                  @click="deleteAppointment(appointment.id)"
                  class="btn btn-xs btn-danger"
                  title="Delete appointment"
                  :disabled="isActionLoading(appointment.id)"
                >
                  <span v-if="isActionLoading(appointment.id)" class="btn-spinner"></span>
                  <span v-else>🗑️</span> Delete
                </button>
              </div>
            </div>
          </div>
          <p v-else class="no-events">No appointments scheduled for this day</p>
        </div>
      </div>
    </div>

    <!-- Appointment Modal -->
    <AppointmentModal
      :is-open="isModalOpen"
      :appointment="selectedAppointment"
      :selected-date="selectedDay?.fullDate"
      @close="closeModal"
      @saved="handleAppointmentSaved"
    />

    <!-- Confirm Modal -->
    <div v-if="confirmModal.show" class="modal-overlay" @click.self="closeConfirmModal">
      <div class="modal modal-sm">
        <div class="modal-header">
          <h2>{{ confirmModal.title }}</h2>
          <button @click="closeConfirmModal" class="close-btn">&times;</button>
        </div>
        <div class="modal-body">
          <p>{{ confirmModal.message }}</p>
          <div class="modal-footer">
            <button @click="closeConfirmModal" class="btn btn-secondary">Cancel</button>
            <button @click="executeConfirmModal" :class="confirmModal.dangerous ? 'btn btn-danger' : 'btn btn-primary'">Confirm</button>
          </div>
        </div>
      </div>
    </div>
  </AdminLayout>
</template>

<script setup lang="ts">
import { ref, reactive, computed, onMounted } from 'vue'
import AdminLayout from '@/components/AdminLayout.vue'
import AppointmentModal from '@/components/AppointmentModal.vue'
import type { Appointment } from '@/types'
import { apiGet, apiDelete, apiClient } from '@/utils/apiClient'
import { logError } from '@/services/logger'
import { useToast } from '@/composables/useToast'
import { useLoadingState } from '@/composables/useLoadingState'

const toast = useToast()
const { isLoading, actionLoading, executeWithLoading, isActionLoading } = useLoadingState()

interface CalendarDay {
  date: number
  fullDate: Date
  isOtherMonth: boolean
  isToday: boolean
  hasEvents: boolean
  eventCount: number
}

const weekDays = ['Sun', 'Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat']

const currentDate = ref(new Date())
const selectedDay = ref<CalendarDay | null>(null)
const hoveredDay = ref<CalendarDay | null>(null)
const appointments = ref<Appointment[]>([])
const isModalOpen = ref(false)
const selectedAppointment = ref<Appointment | null>(null)

const currentMonthYear = computed(() => {
  const options: Intl.DateTimeFormatOptions = { month: 'long', year: 'numeric' }
  return currentDate.value.toLocaleDateString('en-US', options)
})

const dayAppointments = computed(() => {
  if (!selectedDay.value) return []
  
  const dayStr = selectedDay.value.fullDate.toDateString()
  return appointments.value.filter(apt => {
    const aptDate = new Date(apt.startTime).toDateString()
    return aptDate === dayStr
  }).sort((a, b) => new Date(a.startTime).getTime() - new Date(b.startTime).getTime())
})

const calendarDays = computed(() => {
  const days: CalendarDay[] = []
  const year = currentDate.value.getFullYear()
  const month = currentDate.value.getMonth()

  // Build appointment count map once
  const countMap = new Map<string, number>()
  for (const apt of appointments.value) {
    const key = new Date(apt.startTime).toDateString()
    countMap.set(key, (countMap.get(key) ?? 0) + 1)
  }

  const firstDay = new Date(year, month, 1)
  const firstDayOfWeek = firstDay.getDay()
  const lastDay = new Date(year, month + 1, 0)
  const lastDateOfMonth = lastDay.getDate()
  const prevMonthLastDay = new Date(year, month, 0).getDate()
  const today = new Date()

  for (let i = firstDayOfWeek - 1; i >= 0; i--) {
    const date = prevMonthLastDay - i
    days.push({ date, fullDate: new Date(year, month - 1, date), isOtherMonth: true, isToday: false, hasEvents: false, eventCount: 0 })
  }

  for (let date = 1; date <= lastDateOfMonth; date++) {
    const fullDate = new Date(year, month, date)
    const isToday = date === today.getDate() && month === today.getMonth() && year === today.getFullYear()
    const eventCount = countMap.get(fullDate.toDateString()) ?? 0
    days.push({ date, fullDate, isOtherMonth: false, isToday, hasEvents: eventCount > 0, eventCount })
  }

  const remainingDays = 42 - days.length
  for (let date = 1; date <= remainingDays; date++) {
    days.push({ date, fullDate: new Date(year, month + 1, date), isOtherMonth: true, isToday: false, hasEvents: false, eventCount: 0 })
  }

  return days
})

const loadAppointments = async () => {
  await executeWithLoading(async () => {
    try {
      // Load appointments for the current month and surrounding months
      const year = currentDate.value.getFullYear()
      const month = currentDate.value.getMonth()
      const startDate = new Date(year, month - 1, 1).toISOString()
      const endDate = new Date(year, month + 2, 0).toISOString()
      
      const response = await apiGet<Appointment[]>(`/appointments?startDate=${startDate}&endDate=${endDate}`)
      appointments.value = response
    } catch (error) {
      logError('Failed to load appointments', error)
      toast.error('Failed to load appointments')
    }
  })
}

const navigateMonth = (delta: number) => {
  currentDate.value = new Date(
    currentDate.value.getFullYear(),
    currentDate.value.getMonth() + delta,
    1
  )
  loadAppointments()
  const today = calendarDays.value.find(day => day.isToday && !day.isOtherMonth)
  selectedDay.value = today || calendarDays.value.find(day => !day.isOtherMonth) || null
}

const previousMonth = () => navigateMonth(-1)
const nextMonth = () => navigateMonth(1)

// Inline confirm modal
const confirmModal = reactive({ show: false, title: '', message: '', dangerous: false, onConfirm: null as (() => void) | null })
const showConfirmModal = (title: string, message: string, onConfirm: () => void, dangerous = false) => {
  Object.assign(confirmModal, { show: true, title, message, dangerous, onConfirm })
}
const closeConfirmModal = () => { confirmModal.show = false; confirmModal.onConfirm = null }
const executeConfirmModal = () => { confirmModal.onConfirm?.(); closeConfirmModal() }

const selectDay = (day: CalendarDay) => {
  if (!day.isOtherMonth) {
    selectedDay.value = day
  }
}

const openNewAppointmentModal = () => {
  selectedAppointment.value = null
  isModalOpen.value = true
}

const editAppointment = (appointment: Appointment) => {
  selectedAppointment.value = appointment
  isModalOpen.value = true
}

const closeModal = () => {
  isModalOpen.value = false
  selectedAppointment.value = null
}

const handleAppointmentSaved = async () => {
  await loadAppointments()
  toast.success('Appointment saved successfully')
}

const markAsComplete = (id: number) => {
  showConfirmModal('Mark as Complete', 'Mark this appointment as complete?', async () => {
  await executeWithLoading(async () => {
    try {
      await apiClient(`/appointments/${id}/status`, {
        method: 'PATCH',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ status: 'Completed' })
      })
      await loadAppointments()
      toast.success('Appointment marked as complete')
    } catch (error) {
      logError('Failed to update appointment:', error)
      toast.error('Failed to update appointment')
    }
  }, id)
  })
}

const deleteAppointment = (id: number) => {
  showConfirmModal('Delete Appointment', 'Are you sure you want to delete this appointment? This action cannot be undone.', async () => {
    await executeWithLoading(async () => {
      try {
        await apiDelete(`/appointments/${id}`)
        await loadAppointments()
        toast.success('Appointment deleted successfully')
      } catch (error) {
        logError('Failed to delete appointment:', error)
        toast.error('Failed to delete appointment')
      }
    }, id)
  }, true)
}

const formatSelectedDate = (day: CalendarDay) => {
  const options: Intl.DateTimeFormatOptions = { 
    weekday: 'long', 
    year: 'numeric', 
    month: 'long', 
    day: 'numeric' 
  }
  return day.fullDate.toLocaleDateString('en-US', options)
}

const formatTime = (isoString: string) => {
  const date = new Date(isoString)
  return date.toLocaleTimeString('en-US', { hour: 'numeric', minute: '2-digit', hour12: true })
}
const formatTooltipDate = (day: CalendarDay) => {
  const options: Intl.DateTimeFormatOptions = { 
    weekday: 'short', 
    month: 'short', 
    day: 'numeric' 
  }
  return day.fullDate.toLocaleDateString('en-US', options)
}

const getAppointmentsForDay = (day: CalendarDay) => {
  const dayStr = day.fullDate.toDateString()
  return appointments.value
    .filter(apt => new Date(apt.startTime).toDateString() === dayStr)
    .sort((a, b) => new Date(a.startTime).getTime() - new Date(b.startTime).getTime())
    .slice(0, 5) // Show max 5 appointments in tooltip
}


onMounted(async () => {
  await loadAppointments()
  // Select today by default
  const today = calendarDays.value.find(day => day.isToday)
  if (today) {
    selectedDay.value = today
  }
})
</script>

<style scoped>
.calendar-container {
  padding: 2rem;
  max-width: 1400px;
  margin: 0 auto;
}

.calendar-header {
  margin-bottom: 2rem;
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
}

.calendar-header h1 {
  font-size: 2rem;
  font-weight: 700;
  color: #1e293b;
  margin: 0 0 0.5rem 0;
}

.subtitle {
  color: #64748b;
  font-size: 1rem;
  margin: 0;
}

.btn {
  padding: 0.625rem 1.25rem;
  border-radius: 6px;
  font-weight: 500;
  font-size: 0.875rem;
  cursor: pointer;
  transition: all 0.2s;
  border: none;
  display: inline-flex;
  align-items: center;
  gap: 0.5rem;
}

.btn-primary {
  background: #4f46e5;
  color: white;
}

.btn-primary:hover {
  background: #4338ca;
  transform: translateY(-1px);
  box-shadow: 0 4px 6px rgba(79, 70, 229, 0.2);
}

.btn-secondary {
  background: #f1f5f9;
  color: #475569;
}

.btn-secondary:hover {
  background: #e2e8f0;
}

.btn-success {
  background: #10b981;
  color: white;
}

.btn-success:hover {
  background: #059669;
}

.btn-danger {
  background: #ef4444;
  color: white;
}

.btn-danger:hover {
  background: #dc2626;
}

.btn-sm {
  padding: 0.5rem 1rem;
  font-size: 0.8125rem;
}

.btn-xs {
  padding: 0.375rem 0.75rem;
  font-size: 0.75rem;
}

.btn-icon {
  font-size: 1.25rem;
  line-height: 1;
}

.calendar-card {
  background: white;
  border-radius: 12px;
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.1);
  padding: 2rem;
}

.calendar-nav {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-bottom: 2rem;
  padding-bottom: 1.5rem;
  border-bottom: 2px solid #f1f5f9;
}

.current-month {
  font-size: 1.5rem;
  font-weight: 600;
  color: #1e293b;
  margin: 0;
}

.nav-btn {
  background: #f8fafc;
  border: 1px solid #e2e8f0;
  border-radius: 8px;
  width: 40px;
  height: 40px;
  display: flex;
  align-items: center;
  justify-content: center;
  cursor: pointer;
  transition: all 0.2s;
  font-size: 1.2rem;
  color: #475569;
}

.nav-btn:hover {
  background: #4f46e5;
  border-color: #4f46e5;
  color: white;
  transform: translateY(-1px);
}

.calendar-grid {
  display: grid;
  grid-template-columns: repeat(7, 1fr);
  gap: 1px;
  background: #e2e8f0;
  border: 1px solid #e2e8f0;
  border-radius: 8px;
  overflow: visible;
}

.day-header {
  background: #f8fafc;
  padding: 1rem;
  text-align: center;
  font-weight: 600;
  font-size: 0.875rem;
  color: #475569;
  text-transform: uppercase;
  letter-spacing: 0.05em;
}

.calendar-day {
  background: white;
  padding: 1rem;
  min-height: 80px;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: flex-start;
  cursor: pointer;
  transition: all 0.2s;
  position: relative;
}

.calendar-day:hover {
  background: #f8fafc;
  transform: scale(1.02);
  z-index: 10;
}

.calendar-day.other-month {
  opacity: 0.3;
  cursor: default;
}

.calendar-day.other-month:hover {
  background: white;
  transform: none;
}

.calendar-day.today {
  background: #eef2ff;
  border: 2px solid #4f46e5;
}

.calendar-day.today .day-number {
  color: #4f46e5;
  font-weight: 700;
}

.calendar-day.selected {
  background: #dbeafe;
  border: 2px solid #3b82f6;
}

.day-number {
  font-size: 1rem;
  font-weight: 500;
  color: #1e293b;
  margin-bottom: 0.25rem;
}

.appointment-tooltip {
  position: absolute;
  top: 100%;
  left: 50%;
  transform: translateX(-50%);
  margin-top: 8px;
  background: white;
  border-radius: 8px;
  box-shadow: 0 10px 25px rgba(0, 0, 0, 0.2);
  padding: 0.75rem;
  min-width: 200px;
  max-width: 280px;
  z-index: 100;
  pointer-events: none;
  animation: tooltipFadeIn 0.15s ease-out;
}

.appointment-tooltip::before {
  content: '';
  position: absolute;
  bottom: 100%;
  left: 50%;
  transform: translateX(-50%);
  border: 6px solid transparent;
  border-bottom-color: white;
}

@keyframes tooltipFadeIn {
  from {
    opacity: 0;
    transform: translateX(-50%) translateY(-4px);
  }
  to {
    opacity: 1;
    transform: translateX(-50%) translateY(0);
  }
}

.tooltip-header {
  font-weight: 600;
  font-size: 0.8125rem;
  color: #1e293b;
  margin-bottom: 0.5rem;
  padding-bottom: 0.5rem;
  border-bottom: 1px solid #e2e8f0;
}

.tooltip-appointments {
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
}

.tooltip-apt {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  font-size: 0.75rem;
}

.tooltip-time {
  color: #4f46e5;
  font-weight: 600;
  min-width: 60px;
}

.tooltip-customer {
  color: #475569;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.event-count {
  background: #4f46e5;
  color: white;
  border-radius: 50%;
  width: 24px;
  height: 24px;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 0.75rem;
  font-weight: 600;
  margin-top: auto;
}

.day-info {
  margin-top: 2rem;
  padding: 1.5rem;
  background: #f8fafc;
  border-radius: 8px;
  border-left: 4px solid #4f46e5;
}

.day-info-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 1rem;
}

.day-info h3 {
  font-size: 1.125rem;
  font-weight: 600;
  color: #1e293b;
  margin: 0;
}

.no-events {
  color: #64748b;
  font-size: 0.875rem;
  margin: 0;
  font-style: italic;
}

.appointments-list {
  display: flex;
  flex-direction: column;
  gap: 1rem;
}

.appointment-card {
  background: white;
  border-radius: 8px;
  padding: 1rem;
  border-left: 4px solid #94a3b8;
  box-shadow: 0 1px 2px rgba(0, 0, 0, 0.05);
  transition: all 0.2s;
}

.appointment-card:hover {
  box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1);
  transform: translateY(-1px);
}

.appointment-card.status-scheduled {
  border-left-color: #3b82f6;
}

.appointment-card.status-completed {
  border-left-color: #10b981;
}

.appointment-card.status-cancelled {
  border-left-color: #ef4444;
}

.appointment-card.status-noshow {
  border-left-color: #f59e0b;
}

.appointment-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 0.75rem;
}

.appointment-time {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  font-weight: 600;
  color: #1e293b;
}

.time-icon {
  font-size: 1rem;
}

.status-badge {
  padding: 0.25rem 0.75rem;
  border-radius: 12px;
  font-size: 0.75rem;
  font-weight: 600;
  text-transform: uppercase;
  letter-spacing: 0.025em;
}

.status-badge.status-scheduled {
  background: #dbeafe;
  color: #1e40af;
}

.status-badge.status-completed {
  background: #d1fae5;
  color: #065f46;
}

.status-badge.status-cancelled {
  background: #fee2e2;
  color: #991b1b;
}

.status-badge.status-noshow {
  background: #fef3c7;
  color: #92400e;
}

.appointment-body {
  margin-bottom: 0.75rem;
}

.customer-info strong {
  color: #1e293b;
  font-size: 0.9375rem;
}

.contact-info {
  display: flex;
  flex-direction: column;
  gap: 0.25rem;
  margin-top: 0.25rem;
  font-size: 0.8125rem;
  color: #64748b;
}

.service-type {
  margin-top: 0.5rem;
  color: #475569;
  font-size: 0.875rem;
}

.notes {
  margin-top: 0.5rem;
  padding: 0.5rem;
  background: #f8fafc;
  border-radius: 4px;
  font-size: 0.8125rem;
  color: #475569;
  font-style: italic;
}

.appointment-actions {
  display: flex;
  gap: 0.5rem;
  padding-top: 0.75rem;
  border-top: 1px solid #f1f5f9;
}

/* Responsive Design */
@media (max-width: 768px) {
  .calendar-container {
    padding: 1rem;
  }

  .calendar-header {
    flex-direction: column;
    gap: 1rem;
  }

  .calendar-card {
    padding: 1rem;
  }

  .calendar-nav {
    padding-bottom: 1rem;
  }

  .current-month {
    font-size: 1.25rem;
  }

  .calendar-day {
    min-height: 60px;
    padding: 0.5rem;
  }

  .day-number {
    font-size: 0.875rem;
  }

  .day-header {
    padding: 0.75rem 0.25rem;
    font-size: 0.75rem;
  }

  .day-info-header {
    flex-direction: column;
    align-items: flex-start;
    gap: 0.75rem;
  }

  .appointment-actions {
    flex-wrap: wrap;
  }
}

/* Calendar-specific positioning for loading overlay */
.calendar-card {
  position: relative;
}
</style>
