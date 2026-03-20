import { reactive, watch } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import type { VehicleCondition, VehicleFilters } from '@/types/vehicle.types'

export function useVehicleFilters() {
  const router = useRouter()
  const route = useRoute()

  const filters = reactive<VehicleFilters>({
    page: Number(route.query.page) || 1,
    pageSize: 12,
    categoryId: route.query.categoryId ? Number(route.query.categoryId) : undefined,
    condition: (route.query.condition as VehicleCondition) || undefined,
    search: (route.query.search as string) || undefined,
  })

  watch(filters, (val) => {
    router.replace({
      query: {
        ...(val.categoryId && { categoryId: val.categoryId }),
        ...(val.condition && { condition: val.condition }),
        ...(val.search && { search: val.search }),
        ...(val.page > 1 && { page: val.page }),
      }
    })
  })

  function resetFilters() {
    filters.page = 1
    filters.categoryId = undefined
    filters.condition = undefined
    filters.search = undefined
  }

  return { filters, resetFilters }
}
