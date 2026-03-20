import { defineStore } from 'pinia'
import { ref } from 'vue'
import * as vehicleService from '@/services/vehicle.service'
import type { Vehicle, VehicleListItem, CreateVehicleDto, UpdateVehicleDto } from '@/types/vehicle.types'

export const useVehicleStore = defineStore('vehicles', () => {
  const vehicles = ref<VehicleListItem[]>([])
  const selectedVehicle = ref<Vehicle | null>(null)
  const totalCount = ref(0)
  const loading = ref(false)
  const saving = ref(false)

  async function fetchVehicles(page = 1, pageSize = 20) {
    loading.value = true
    try {
      const result = await vehicleService.getVehicles(page, pageSize)
      vehicles.value = result.items
      totalCount.value = result.totalCount
    } finally {
      loading.value = false
    }
  }

  async function fetchVehicle(id: number) {
    loading.value = true
    try {
      selectedVehicle.value = await vehicleService.getVehicle(id)
    } finally {
      loading.value = false
    }
  }

  async function createVehicle(dto: CreateVehicleDto): Promise<Vehicle> {
    saving.value = true
    try {
      const v = await vehicleService.createVehicle(dto)
      await fetchVehicles()
      return v
    } finally {
      saving.value = false
    }
  }

  async function updateVehicle(id: number, dto: UpdateVehicleDto): Promise<Vehicle> {
    saving.value = true
    try {
      const v = await vehicleService.updateVehicle(id, dto)
      await fetchVehicles()
      return v
    } finally {
      saving.value = false
    }
  }

  async function deleteVehicle(id: number) {
    await vehicleService.deleteVehicle(id)
    vehicles.value = vehicles.value.filter(v => v.id !== id)
    totalCount.value--
  }

  return { vehicles, selectedVehicle, totalCount, loading, saving, fetchVehicles, fetchVehicle, createVehicle, updateVehicle, deleteVehicle }
})
