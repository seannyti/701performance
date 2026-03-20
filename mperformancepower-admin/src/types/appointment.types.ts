export type AppointmentStatus = 'Scheduled' | 'Completed' | 'Cancelled' | 'NoShow'

export interface Appointment {
  id: number
  title: string
  customerName: string
  customerEmail: string
  customerPhone: string
  userId: number | null
  userName: string | null
  vehicleId: number | null
  vehicleName: string | null
  startTime: string
  endTime: string
  status: AppointmentStatus
  notes: string | null
  createdAt: string
}

export interface CreateAppointmentDto {
  title: string
  customerName: string
  customerEmail: string
  customerPhone: string
  userId?: number | null
  vehicleId?: number | null
  startTime: string
  endTime: string
  notes?: string | null
}

export interface UpdateAppointmentDto {
  title: string
  customerName: string
  customerEmail: string
  customerPhone: string
  userId?: number | null
  vehicleId?: number | null
  startTime: string
  endTime: string
  status: AppointmentStatus
  notes?: string | null
}
