export type InquiryStatus = 'New' | 'InProgress' | 'Resolved'

export interface Inquiry {
  id: number
  vehicleId: number | null
  vehicleName: string | null
  name: string
  email: string
  phone: string
  message: string
  createdAt: string
  status: InquiryStatus
  respondedAt: string | null
}

export interface InquiryStats {
  total: number
  new: number
  inProgress: number
  resolved: number
  avgResponseMinutes: number | null
}

export interface InquiryFilters {
  status: string
  search: string
  from: string
  to: string
}
