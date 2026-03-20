import { api } from './api'

export interface UserProfile {
  id: number
  email: string
  firstName: string | null
  lastName: string | null
  phone: string | null
  avatarPath: string | null
  createdAt: string
}

export interface ProfileAppointment {
  id: number
  title: string
  vehicleName: string | null
  startTime: string
  endTime: string
  status: string
  notes: string | null
}

export interface ProfileOrder {
  id: number
  vehicleName: string
  salePrice: number
  paymentMethod: string
  status: string
  trackingNumber: string | null
  createdAt: string
  deliveredAt: string | null
}

export async function getProfile(): Promise<UserProfile> {
  const { data } = await api.get('/profile')
  return data
}

export async function updateProfile(dto: { firstName: string; lastName: string; phone: string }): Promise<void> {
  await api.put('/profile', dto)
}

export async function changePassword(dto: { currentPassword: string; newPassword: string }): Promise<void> {
  await api.post('/profile/change-password', dto)
}

export async function uploadAvatar(file: File): Promise<{ avatarPath: string }> {
  const form = new FormData()
  form.append('file', file)
  const { data } = await api.post('/profile/avatar', form, {
    headers: { 'Content-Type': 'multipart/form-data' },
  })
  return data
}

export async function deleteAvatar(): Promise<void> {
  await api.delete('/profile/avatar')
}

export async function getMyAppointments(): Promise<ProfileAppointment[]> {
  const { data } = await api.get('/profile/appointments')
  return data
}

export async function getMyOrders(): Promise<ProfileOrder[]> {
  const { data } = await api.get('/profile/orders')
  return data
}
