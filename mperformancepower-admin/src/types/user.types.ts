export interface AdminUser {
  id: number
  email: string
  firstName: string | null
  lastName: string | null
  phone: string | null
  role: string
  emailVerified: boolean
  isActive: boolean
  createdAt: string
}

export interface AdminUpdateUserDto {
  firstName?: string | null
  lastName?: string | null
  phone?: string | null
  email?: string
  role?: string
}

export interface UserListResult {
  items: AdminUser[]
  totalCount: number
  page: number
  pageSize: number
}
