import { api } from './api'
import type { Appointment, CreateAppointmentDto, UpdateAppointmentDto } from '@/types/appointment.types'

export async function getAppointments(from: string, to: string): Promise<Appointment[]> {
  const { data } = await api.get('/appointments', { params: { from, to } })
  return data
}

export async function getAppointment(id: number): Promise<Appointment> {
  const { data } = await api.get(`/appointments/${id}`)
  return data
}

export async function createAppointment(dto: CreateAppointmentDto): Promise<Appointment> {
  const { data } = await api.post('/appointments', dto)
  return data
}

export async function updateAppointment(id: number, dto: UpdateAppointmentDto): Promise<Appointment> {
  const { data } = await api.put(`/appointments/${id}`, dto)
  return data
}

export async function deleteAppointment(id: number): Promise<void> {
  await api.delete(`/appointments/${id}`)
}
