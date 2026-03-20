export type VehicleCondition = 'New' | 'Used'

export interface VehicleSpec {
  label: string
  value: string
}

export interface VehicleImage {
  id: number
  fileName: string
  isPrimary: boolean
  displayOrder: number
}

export interface VehicleListItem {
  id: number
  make: string
  model: string
  year: number
  categoryId: number
  category: string
  price: number
  mileage: number | null
  condition: VehicleCondition
  stock: number
  featured: boolean
  primaryImage: string | null
}

export interface Vehicle extends VehicleListItem {
  description: string
  createdAt: string
  specs: VehicleSpec[]
  images: VehicleImage[]
}

export interface VehicleFilters {
  page: number
  pageSize: number
  categoryId?: number
  condition?: VehicleCondition
  featured?: boolean
  search?: string
}
