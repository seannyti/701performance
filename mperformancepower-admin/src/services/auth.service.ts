import { api } from './api'
import type { LoginDto, AuthResponse } from '@/types/auth.types'

export async function login(dto: LoginDto): Promise<AuthResponse> {
  const { data } = await api.post('/auth/login', dto)
  return data
}
