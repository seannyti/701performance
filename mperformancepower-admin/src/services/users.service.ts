import { api } from './api'
import type { AdminUser, AdminUpdateUserDto, UserListResult } from '@/types/user.types'

export async function getUsers(page = 1, pageSize = 20, search?: string, role?: string): Promise<UserListResult> {
  const { data } = await api.get('/users', { params: { page, pageSize, search, role } })
  return data
}

export async function updateUser(id: number, dto: AdminUpdateUserDto): Promise<AdminUser> {
  const { data } = await api.put(`/users/${id}`, dto)
  return data
}

export async function forceVerifyUser(id: number): Promise<void> {
  await api.post(`/users/${id}/verify`)
}

export async function toggleUserActive(id: number): Promise<{ isActive: boolean }> {
  const { data } = await api.post(`/users/${id}/toggle-active`)
  return data
}

export async function resetUserPassword(id: number): Promise<void> {
  await api.post(`/users/${id}/reset-password`)
}

export async function deleteUser(id: number): Promise<void> {
  await api.delete(`/users/${id}`)
}
