import { api } from './api'
import type { PagedResult } from '@/types/common.types'
import type { Vehicle, VehicleFilters, VehicleListItem } from '@/types/vehicle.types'

export async function getVehicles(filters: VehicleFilters): Promise<PagedResult<VehicleListItem>> {
  const { data } = await api.get('/vehicles', { params: filters })
  return data
}

export async function getVehicle(id: number): Promise<Vehicle> {
  const { data } = await api.get(`/vehicles/${id}`)
  return data
}
