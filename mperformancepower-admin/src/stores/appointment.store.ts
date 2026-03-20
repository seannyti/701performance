import { defineStore } from 'pinia'
import { ref } from 'vue'
import * as appointmentService from '@/services/appointment.service'
import type { Appointment, CreateAppointmentDto, UpdateAppointmentDto } from '@/types/appointment.types'

export const useAppointmentStore = defineStore('appointments', () => {
  const appointments = ref<Appointment[]>([])
  const loading = ref(false)
  const saving = ref(false)

  async function fetchAppointments(from: string, to: string) {
    loading.value = true
    try {
      appointments.value = await appointmentService.getAppointments(from, to)
    } finally {
      loading.value = false
    }
  }

  async function createAppointment(dto: CreateAppointmentDto): Promise<{ appointment?: Appointment; error?: string }> {
    saving.value = true
    try {
      const appointment = await appointmentService.createAppointment(dto)
      appointments.value.push(appointment)
      return { appointment }
    } catch (e: any) {
      return { error: e?.response?.data?.message ?? 'Failed to create appointment.' }
    } finally {
      saving.value = false
    }
  }

  async function updateAppointment(id: number, dto: UpdateAppointmentDto): Promise<{ appointment?: Appointment; error?: string }> {
    saving.value = true
    try {
      const appointment = await appointmentService.updateAppointment(id, dto)
      const idx = appointments.value.findIndex(a => a.id === id)
      if (idx !== -1) appointments.value[idx] = appointment
      return { appointment }
    } catch (e: any) {
      return { error: e?.response?.data?.message ?? 'Failed to update appointment.' }
    } finally {
      saving.value = false
    }
  }

  async function deleteAppointment(id: number) {
    await appointmentService.deleteAppointment(id)
    appointments.value = appointments.value.filter(a => a.id !== id)
  }

  return { appointments, loading, saving, fetchAppointments, createAppointment, updateAppointment, deleteAppointment }
})
