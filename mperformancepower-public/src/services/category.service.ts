import { api } from './api'

export interface PublicCategory {
  id: number
  name: string
  isActive: boolean
  vehicleCount: number
}

export async function getActiveCategories(): Promise<PublicCategory[]> {
  const { data } = await api.get('/categories', { params: { activeOnly: true } })
  return data
}
