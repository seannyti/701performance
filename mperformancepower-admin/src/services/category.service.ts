import { api } from './api'
import type { Category, CreateCategoryDto, UpdateCategoryDto } from '@/types/category.types'

export async function getCategories(activeOnly = false): Promise<Category[]> {
  const { data } = await api.get('/categories', { params: { activeOnly } })
  return data
}

export async function createCategory(dto: CreateCategoryDto): Promise<Category> {
  const { data } = await api.post('/categories', dto)
  return data
}

export async function updateCategory(id: number, dto: UpdateCategoryDto): Promise<Category> {
  const { data } = await api.put(`/categories/${id}`, dto)
  return data
}

export async function toggleCategory(id: number): Promise<Category> {
  const { data } = await api.patch(`/categories/${id}/toggle`)
  return data
}

export async function deleteCategory(id: number): Promise<void> {
  await api.delete(`/categories/${id}`)
}
