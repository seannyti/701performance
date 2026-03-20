export interface LoginDto {
  email: string
  password: string
}

export interface RegisterDto {
  firstName: string
  lastName: string
  email: string
  password: string
}

export interface AuthResponse {
  token: string
  expiresAt: string
  email: string
  role: string
  firstName: string | null
  lastName: string | null
}

export interface RegisterResponse {
  requiresVerification: boolean
  auth: AuthResponse | null
}
