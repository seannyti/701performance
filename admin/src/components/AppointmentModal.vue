<template>
  <Teleport to="body">
    <div v-if="isOpen" class="modal-overlay" @click="closeModal">
      <div class="modal-content" @click.stop>
        <div class="modal-header">
          <h2>{{ isEditing ? 'Edit Appointment' : 'New Appointment' }}</h2>
          <button @click="closeModal" class="close-btn" type="button">&times;</button>
        </div>

        <form @submit.prevent="handleSubmit" class="modal-body">
          <!-- Date & Time Section -->
          <div class="form-section">
            <h3>📅 Date & Time</h3>
            <div class="form-row">
              <div class="form-group">
                <label for="startTime">Start Time *</label>
                <input
                  id="startTime"
                  v-model="formData.startTime"
                  type="datetime-local"
                  required
                  class="form-control"
                />
              </div>
              <div class="form-group">
                <label for="endTime">End Time *</label>
                <input
                  id="endTime"
                  v-model="formData.endTime"
                  type="datetime-local"
                  required
                  class="form-control"
                />
              </div>
            </div>
          </div>

          <!-- Customer Type Selection -->
          <div class="form-section">
            <h3>👤 Customer</h3>
            <div class="customer-type-toggle">
              <button
                type="button"
                class="toggle-btn"
                :class="{ active: customerType === 'guest' }"
                @click="customerType = 'guest'"
              >
                New/Guest Customer
              </button>
              <button
                type="button"
                class="toggle-btn"
                :class="{ active: customerType === 'registered' }"
                @click="customerType = 'registered'"
              >
                Registered User
              </button>
            </div>

            <!-- Registered User Selection -->
            <div v-if="customerType === 'registered'" class="form-group">
              <label for="userId">Select User *</label>
              <select
                id="userId"
                v-model="formData.userId"
                @change="handleUserSelection"
                class="form-control"
                required
              >
                <option :value="null">-- Select a user --</option>
                <option v-for="user in registeredUsers" :key="user.id" :value="user.id">
                  {{ user.fullName }} ({{ user.email }})
                </option>
              </select>
            </div>

            <!-- Guest Customer Details -->
            <div v-if="customerType === 'guest'">
              <div class="form-group">
                <label for="customerName">Customer Name *</label>
                <input
                  id="customerName"
                  v-model="formData.customerName"
                  type="text"
                  required
                  class="form-control"
                  placeholder="John Doe"
                />
              </div>
              <div class="form-row">
                <div class="form-group">
                  <label for="customerEmail">Email</label>
                  <input
                    id="customerEmail"
                    v-model="formData.customerEmail"
                    type="email"
                    class="form-control"
                    placeholder="john@example.com"
                  />
                </div>
                <div class="form-group">
                  <label for="customerPhone">Phone</label>
                  <input
                    id="customerPhone"
                    v-model="formData.customerPhone"
                    type="tel"
                    class="form-control"
                    placeholder="(555) 123-4567"
                  />
                </div>
              </div>
            </div>
          </div>

          <!-- Service & Notes -->
          <div class="form-section">
            <h3>🔧 Service Details</h3>
            <div class="form-group">
              <label for="serviceType">Service Type</label>
              <input
                id="serviceType"
                v-model="formData.serviceType"
                type="text"
                class="form-control"
                placeholder="e.g., Test Ride, Consultation, Service Appointment"
              />
            </div>
            <div class="form-group">
              <label for="notes">Notes</label>
              <textarea
                id="notes"
                v-model="formData.notes"
                class="form-control"
                rows="3"
                placeholder="Additional information..."
              ></textarea>
            </div>
          </div>

          <!-- Status (only for editing) -->
          <div v-if="isEditing" class="form-section">
            <h3>📋 Status</h3>
            <div class="form-group">
              <label for="status">Appointment Status *</label>
              <select
                id="status"
                v-model="formData.status"
                class="form-control"
                required
              >
                <option value="Scheduled">Scheduled</option>
                <option value="Completed">Completed</option>
                <option value="Cancelled">Cancelled</option>
                <option value="NoShow">No Show</option>
              </select>
            </div>
          </div>

          <!-- Error Message -->
          <div v-if="error" class="alert alert-error">
            {{ error }}
          </div>

          <!-- Actions -->
          <div class="modal-actions">
            <button type="button" @click="closeModal" class="btn btn-secondary" :disabled="loading">
              Cancel
            </button>
            <button type="submit" class="btn btn-primary" :disabled="loading">
              {{ loading ? 'Saving...' : (isEditing ? 'Update' : 'Create') }} Appointment
            </button>
          </div>
        </form>
      </div>
    </div>
  </Teleport>
</template>

<script setup lang="ts">
import { ref, computed, watch } from 'vue'
import type { Appointment, CreateAppointmentRequest, UpdateAppointmentRequest, AppointmentUser } from '@/types'
import { apiGet, apiPost, apiPut } from '@/utils/apiClient'

const props = defineProps<{
  isOpen: boolean
  appointment?: Appointment | null
  selectedDate?: Date | null
}>()

const emit = defineEmits<{
  close: []
  saved: []
}>()

const isEditing = computed(() => !!props.appointment)
const customerType = ref<'guest' | 'registered'>('guest')
const registeredUsers = ref<AppointmentUser[]>([])
const loading = ref(false)
const error = ref('')

const formData = ref<UpdateAppointmentRequest>({
  startTime: '',
  endTime: '',
  customerName: '',
  customerEmail: '',
  customerPhone: '',
  serviceType: '',
  notes: '',
  status: 'Scheduled',
  userId: undefined
})

// Initialize form data
const initializeForm = () => {
  if (props.appointment) {
    // Editing existing appointment
    formData.value = {
      startTime: formatDateTimeLocal(props.appointment.startTime),
      endTime: formatDateTimeLocal(props.appointment.endTime),
      customerName: props.appointment.customerName,
      customerEmail: props.appointment.customerEmail || '',
      customerPhone: props.appointment.customerPhone || '',
      serviceType: props.appointment.serviceType || '',
      notes: props.appointment.notes || '',
      status: props.appointment.status,
      userId: props.appointment.userId
    }
    customerType.value = props.appointment.userId ? 'registered' : 'guest'
  } else if (props.selectedDate) {
    // New appointment with selected date
    const startDate = new Date(props.selectedDate)
    startDate.setHours(9, 0, 0, 0) // Default to 9 AM
    const endDate = new Date(startDate)
    endDate.setHours(10, 0, 0, 0) // Default 1 hour duration

    formData.value = {
      startTime: formatDateTimeLocal(startDate.toISOString()),
      endTime: formatDateTimeLocal(endDate.toISOString()),
      customerName: '',
      customerEmail: '',
      customerPhone: '',
      serviceType: '',
      notes: '',
      status: 'Scheduled',
      userId: undefined
    }
  }
}

const formatDateTimeLocal = (isoString: string): string => {
  const date = new Date(isoString)
  const year = date.getFullYear()
  const month = String(date.getMonth() + 1).padStart(2, '0')
  const day = String(date.getDate()).padStart(2, '0')
  const hours = String(date.getHours()).padStart(2, '0')
  const minutes = String(date.getMinutes()).padStart(2, '0')
  return `${year}-${month}-${day}T${hours}:${minutes}`
}

const loadRegisteredUsers = async () => {
  try {
    const response = await apiGet<AppointmentUser[]>('/appointments/users')
    registeredUsers.value = response
  } catch (err) {
    console.error('Failed to load users:', err)
  }
}

const handleUserSelection = () => {
  if (formData.value.userId) {
    const user = registeredUsers.value.find(u => u.id === formData.value.userId)
    if (user) {
      formData.value.customerName = user.fullName
      formData.value.customerEmail = user.email
      formData.value.customerPhone = user.phone || ''
    }
  }
}

const handleSubmit = async () => {
  loading.value = true
  error.value = ''

  try {
    // Prepare request data
    const requestData: CreateAppointmentRequest | UpdateAppointmentRequest = {
      startTime: new Date(formData.value.startTime).toISOString(),
      endTime: new Date(formData.value.endTime).toISOString(),
      customerName: formData.value.customerName,
      customerEmail: formData.value.customerEmail || undefined,
      customerPhone: formData.value.customerPhone || undefined,
      serviceType: formData.value.serviceType || undefined,
      notes: formData.value.notes || undefined,
      userId: customerType.value === 'registered' ? formData.value.userId : undefined,
      ...(isEditing.value && { status: formData.value.status })
    }

    if (isEditing.value && props.appointment) {
      await apiPut(`/appointments/${props.appointment.id}`, requestData)
    } else {
      await apiPost('/appointments', requestData)
    }

    emit('saved')
    closeModal()
  } catch (err: any) {
    error.value = err.response?.data?.message || err.message || 'Failed to save appointment'
  } finally {
    loading.value = false
  }
}

const closeModal = () => {
  error.value = ''
  emit('close')
}

// Watch for modal opening
watch(() => props.isOpen, (isOpen) => {
  if (isOpen) {
    initializeForm()
    loadRegisteredUsers()
  }
})

// Watch customer type to clear userId when switching to guest
watch(customerType, (newType) => {
  if (newType === 'guest') {
    formData.value.userId = undefined
  }
})
</script>

<style scoped>
.modal-overlay {
  position: fixed;
  inset: 0;
  background: rgba(0, 0, 0, 0.6);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 1000;
  backdrop-filter: blur(4px);
  padding: 1rem;
}

.modal-content {
  background: white;
  border-radius: 12px;
  width: 100%;
  max-width: 600px;
  max-height: 90vh;
  display: flex;
  flex-direction: column;
  box-shadow: 0 20px 25px -5px rgba(0, 0, 0, 0.3);
}

.modal-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 1.5rem;
  border-bottom: 1px solid #e2e8f0;
}

.modal-header h2 {
  font-size: 1.5rem;
  font-weight: 600;
  color: #1e293b;
  margin: 0;
}

.close-btn {
  background: none;
  border: none;
  font-size: 2rem;
  color: #64748b;
  cursor: pointer;
  line-height: 1;
  padding: 0;
  width: 32px;
  height: 32px;
  display: flex;
  align-items: center;
  justify-content: center;
  border-radius: 6px;
  transition: all 0.2s;
}

.close-btn:hover {
  background: #f1f5f9;
  color: #1e293b;
}

.modal-body {
  padding: 1.5rem;
  overflow-y: auto;
}

.form-section {
  margin-bottom: 1.5rem;
}

.form-section h3 {
  font-size: 1rem;
  font-weight: 600;
  color: #475569;
  margin: 0 0 1rem 0;
  padding-bottom: 0.5rem;
  border-bottom: 2px solid #f1f5f9;
}

.form-group {
  margin-bottom: 1rem;
}

.form-group label {
  display: block;
  font-weight: 500;
  color: #475569;
  margin-bottom: 0.5rem;
  font-size: 0.875rem;
}

.form-control {
  width: 100%;
  padding: 0.625rem;
  border: 1px solid #cbd5e1;
  border-radius: 6px;
  font-size: 0.875rem;
  transition: all 0.2s;
}

.form-control:focus {
  outline: none;
  border-color: #4f46e5;
  box-shadow: 0 0 0 3px rgba(79, 70, 229, 0.1);
}

textarea.form-control {
  resize: vertical;
  font-family: inherit;
}

.form-row {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 1rem;
}

.customer-type-toggle {
  display: flex;
  gap: 0.5rem;
  margin-bottom: 1rem;
}

.toggle-btn {
  flex: 1;
  padding: 0.75rem 1rem;
  border: 2px solid #e2e8f0;
  background: white;
  border-radius: 8px;
  font-weight: 500;
  color: #64748b;
  cursor: pointer;
  transition: all 0.2s;
}

.toggle-btn:hover {
  border-color: #cbd5e1;
  background: #f8fafc;
}

.toggle-btn.active {
  border-color: #4f46e5;
  background: #eef2ff;
  color: #4f46e5;
}

.alert {
  padding: 0.75rem 1rem;
  border-radius: 6px;
  margin-bottom: 1rem;
}

.alert-error {
  background: #fef2f2;
  border: 1px solid #fecaca;
  color: #991b1b;
}

.modal-actions {
  display: flex;
  gap: 0.75rem;
  justify-content: flex-end;
  padding-top: 1rem;
  border-top: 1px solid #e2e8f0;
}

.btn {
  padding: 0.625rem 1.25rem;
  border-radius: 6px;
  font-weight: 500;
  font-size: 0.875rem;
  cursor: pointer;
  transition: all 0.2s;
  border: none;
}

.btn:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

.btn-secondary {
  background: #f1f5f9;
  color: #475569;
}

.btn-secondary:hover:not(:disabled) {
  background: #e2e8f0;
}

.btn-primary {
  background: #4f46e5;
  color: white;
}

.btn-primary:hover:not(:disabled) {
  background: #4338ca;
  transform: translateY(-1px);
  box-shadow: 0 4px 6px rgba(79, 70, 229, 0.2);
}

@media (max-width: 640px) {
  .form-row {
    grid-template-columns: 1fr;
  }

  .customer-type-toggle {
    flex-direction: column;
  }

  .modal-content {
    max-height: 95vh;
  }
}
</style>
