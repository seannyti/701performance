import { defineStore } from 'pinia'
import { ref } from 'vue'
import * as categoryService from '@/services/category.service'
import type { Category, CreateCategoryDto, UpdateCategoryDto } from '@/types/category.types'

export const useCategoryStore = defineStore('categories', () => {
  const categories = ref<Category[]>([])
  const loading = ref(false)
  const saving = ref(false)

  async function fetchCategories() {
    loading.value = true
    try {
      categories.value = await categoryService.getCategories()
    } finally {
      loading.value = false
    }
  }

  async function createCategory(dto: CreateCategoryDto) {
    saving.value = true
    try {
      const c = await categoryService.createCategory(dto)
      categories.value.push(c)
      return c
    } finally {
      saving.value = false
    }
  }

  async function updateCategory(id: number, dto: UpdateCategoryDto) {
    saving.value = true
    try {
      const c = await categoryService.updateCategory(id, dto)
      const idx = categories.value.findIndex(x => x.id === id)
      if (idx !== -1) categories.value[idx] = c
      return c
    } finally {
      saving.value = false
    }
  }

  async function toggleCategory(id: number) {
    const c = await categoryService.toggleCategory(id)
    const idx = categories.value.findIndex(x => x.id === id)
    if (idx !== -1) categories.value[idx] = c
    return c
  }

  async function deleteCategory(id: number) {
    await categoryService.deleteCategory(id)
    categories.value = categories.value.filter(c => c.id !== id)
  }

  return { categories, loading, saving, fetchCategories, createCategory, updateCategory, toggleCategory, deleteCategory }
})
