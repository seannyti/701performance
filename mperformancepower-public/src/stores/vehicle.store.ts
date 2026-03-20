import { defineStore } from 'pinia'
import { ref } from 'vue'
import * as vehicleService from '@/services/vehicle.service'
import type { Vehicle, VehicleFilters, VehicleListItem } from '@/types/vehicle.types'

export const useVehicleStore = defineStore('vehicles', () => {
  const vehicles = ref<VehicleListItem[]>([])
  const selectedVehicle = ref<Vehicle | null>(null)
  const totalCount = ref(0)
  const totalPages = ref(0)
  const loading = ref(false)
  const error = ref<string | null>(null)

  async function fetchVehicles(filters: VehicleFilters) {
    loading.value = true
    error.value = null
    try {
      const result = await vehicleService.getVehicles(filters)
      vehicles.value = result.items
      totalCount.value = result.totalCount
      totalPages.value = result.totalPages
    } catch (e: any) {
      error.value = e?.title ?? 'Failed to load vehicles.'
    } finally {
      loading.value = false
    }
  }

  async function fetchVehicle(id: number) {
    loading.value = true
    error.value = null
    try {
      selectedVehicle.value = await vehicleService.getVehicle(id)
    } catch (e: any) {
      error.value = e?.title ?? 'Failed to load vehicle.'
    } finally {
      loading.value = false
    }
  }

  return { vehicles, selectedVehicle, totalCount, totalPages, loading, error, fetchVehicles, fetchVehicle }
})
