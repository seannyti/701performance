export interface Category {
  id: number
  name: string
  isActive: boolean
  vehicleCount: number
}

export interface CreateCategoryDto {
  name: string
}

export interface UpdateCategoryDto {
  name: string
}
