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

export interface Vehicle {
  id: number
  make: string
  model: string
  year: number
  categoryId: number
  category: string
  price: number
  mileage: number | null
  condition: VehicleCondition
  description: string
  stock: number
  featured: boolean
  createdAt: string
  specs: VehicleSpec[]
  images: VehicleImage[]
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

export interface CreateVehicleDto {
  make: string
  model: string
  year: number
  categoryId: number
  price: number
  mileage: number | null
  condition: VehicleCondition
  description: string
  stock: number
  featured: boolean
  specs?: VehicleSpec[]
}

export type UpdateVehicleDto = CreateVehicleDto
