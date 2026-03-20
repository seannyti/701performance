import { api } from './api'
import type { LoginDto, RegisterDto, AuthResponse, RegisterResponse } from '@/types/auth.types'

export async function login(dto: LoginDto): Promise<AuthResponse> {
  const { data } = await api.post('/auth/login', dto)
  return data
}

export async function register(dto: RegisterDto): Promise<RegisterResponse> {
  const { data } = await api.post('/auth/register', dto)
  return data
}

export async function verifyEmail(token: string): Promise<void> {
  await api.get('/auth/verify-email', { params: { token } })
}
