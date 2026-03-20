import { api } from './api'
import type { PagedResult } from '@/types/common.types'
import type { Vehicle, VehicleListItem, CreateVehicleDto, UpdateVehicleDto, VehicleImage } from '@/types/vehicle.types'

export async function getVehicles(page = 1, pageSize = 20, categoryId?: number, search?: string): Promise<PagedResult<VehicleListItem>> {
  const { data } = await api.get('/vehicles', { params: { page, pageSize, categoryId, search } })
  return data
}

export async function getVehicle(id: number): Promise<Vehicle> {
  const { data } = await api.get(`/vehicles/${id}`)
  return data
}

export async function createVehicle(dto: CreateVehicleDto): Promise<Vehicle> {
  const { data } = await api.post('/vehicles', dto)
  return data
}

export async function updateVehicle(id: number, dto: UpdateVehicleDto): Promise<Vehicle> {
  const { data } = await api.put(`/vehicles/${id}`, dto)
  return data
}

export async function deleteVehicle(id: number): Promise<void> {
  await api.delete(`/vehicles/${id}`)
}

export async function uploadImages(vehicleId: number, files: FileList): Promise<VehicleImage[]> {
  const form = new FormData()
  Array.from(files).forEach(f => form.append('files', f))
  const { data } = await api.post(`/vehicles/${vehicleId}/images`, form, {
    headers: { 'Content-Type': 'multipart/form-data' },
  })
  return data
}

export async function deleteImage(imageId: number): Promise<void> {
  await api.delete(`/images/${imageId}`)
}

export async function setPrimaryImage(imageId: number): Promise<void> {
  await api.put(`/images/${imageId}/set-primary`)
}

export async function reorderImages(items: { id: number; order: number }[]): Promise<void> {
  await api.put('/images/reorder', items)
}
