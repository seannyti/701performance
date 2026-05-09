import api from './api'

export interface Vehicle {
  id: number
  stockNumber: string
  vin?: string
  year: number
  make: string
  model: string
  trim?: string
  type: string
  condition: string
  color?: string
  mileage?: number
  cost: number
  msrp: number
  salePrice: number
  status: string
  isFeatured: boolean
  description?: string
  createdAt: string
  updatedAt: string
  soldAt?: string
  images: VehicleImage[]
}

export interface VehicleImage {
  id: number
  vehicleId: number
  url: string
  thumbnailUrl: string
  sortOrder: number
  isPrimary: boolean
}

interface VehicleListParams {
  type?: string
  condition?: string
  make?: string
  model?: string
  status?: string
  yearMin?: number
  yearMax?: number
  priceMin?: number
  priceMax?: number
  sort?: string
  page?: number
  pageSize?: number
}

interface VehicleListResponse {
  total: number
  page: number
  pageSize: number
  data: Vehicle[]
}

const inventoryService = {
  async getAll(params: VehicleListParams = {}): Promise<VehicleListResponse> {
    const { data } = await api.get('/api/inventory', { params })
    return data
  },

  async getById(id: number): Promise<Vehicle> {
    const { data } = await api.get(`/api/inventory/${id}`)
    return data
  },

  async getFeatured(): Promise<Vehicle[]> {
    const { data } = await api.get('/api/inventory/featured')
    return data
  },

  async create(vehicle: Omit<Vehicle, 'id' | 'stockNumber' | 'createdAt' | 'updatedAt' | 'images'>): Promise<Vehicle> {
    const { data } = await api.post('/api/inventory', vehicle)
    return data
  },

  async update(id: number, vehicle: Partial<Vehicle>): Promise<Vehicle> {
    const { data } = await api.put(`/api/inventory/${id}`, vehicle)
    return data
  },

  async delete(id: number): Promise<void> {
    await api.delete(`/api/inventory/${id}`)
  },

  async toggleFeatured(id: number): Promise<{ id: number; isFeatured: boolean }> {
    const { data } = await api.put(`/api/inventory/${id}/featured`)
    return data
  },

  async uploadImages(id: number, files: File[]): Promise<VehicleImage[]> {
    const form = new FormData()
    files.forEach(f => form.append('files', f))
    const { data } = await api.post(`/api/inventory/${id}/images`, form, {
      headers: { 'Content-Type': 'multipart/form-data' }
    })
    return data
  },

  async deleteImage(vehicleId: number, imageId: number): Promise<void> {
    await api.delete(`/api/inventory/${vehicleId}/images/${imageId}`)
  },

  getMakes(): string[] {
    return ['Can-Am', 'Polaris', 'Yamaha', 'Honda', 'Kawasaki', 'Suzuki', 'KTM', 'Arctic Cat', 'Ski-Doo', 'Husqvarna', 'Beta', 'Triumph', 'Harley-Davidson', 'Indian', 'CF Moto', 'Kymco', 'Other']
  },

  async getMakesAsync(): Promise<string[]> {
    try {
      const { data } = await api.get('/api/settings')
      if (data['inventory_makes']) return JSON.parse(data['inventory_makes'])
    } catch {}
    return this.getMakes()
  },

  getTypes(): { label: string; value: string }[] {
    return [
      { label: 'ATV', value: 'atv' },
      { label: 'UTV / Side-by-Side', value: 'utv' },
      { label: 'Motorcycle', value: 'motorcycle' },
      { label: 'Dirt Bike', value: 'dirtbike' },
      { label: 'Snowmobile', value: 'snowmobile' },
      { label: 'PWC / Watercraft', value: 'pwc' },
      { label: 'Apparel & Gear', value: 'apparel' },
      { label: 'Other', value: 'other' },
    ]
  },

  // Item categories: vehicle types vs gear types. Gear items skip VIN/mileage/trim in the form.
  GEAR_TYPES: ['apparel'] as string[],
  isGearType(type: string): boolean {
    return this.GEAR_TYPES.includes(type)
  },

  async getTypesAsync(): Promise<{ label: string; value: string }[]> {
    try {
      const { data } = await api.get('/api/settings')
      if (data['inventory_types']) return JSON.parse(data['inventory_types'])
    } catch {}
    return this.getTypes()
  },

  getStatuses(): { label: string; value: string; severity: string }[] {
    return [
      { label: 'Available', value: 'available', severity: 'success' },
      { label: 'Pending', value: 'pending', severity: 'warn' },
      { label: 'Sold', value: 'sold', severity: 'danger' },
      { label: 'Hold', value: 'hold', severity: 'info' },
      { label: 'Trade', value: 'trade', severity: 'secondary' },
    ]
  },

  daysOnLot(createdAt: string): number {
    return Math.floor((Date.now() - new Date(createdAt).getTime()) / 86400000)
  },

  agingColor(days: number): string {
    if (days < 30) return '#4caf50'
    if (days < 60) return '#ff9800'
    if (days < 90) return '#ff5722'
    return '#f44336'
  }
}

export default inventoryService
