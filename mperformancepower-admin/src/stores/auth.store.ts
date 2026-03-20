import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import { login as loginService } from '@/services/auth.service'
import { startSignalR, stopSignalR } from '@/services/signalr.service'
import type { LoginDto, AuthResponse } from '@/types/auth.types'

export const useAuthStore = defineStore('auth', () => {
  const token = ref<string | null>(localStorage.getItem('mpp_token'))
  const email = ref<string | null>(localStorage.getItem('mpp_email'))
  const role = ref<string | null>(localStorage.getItem('mpp_role'))

  const isAuthenticated = computed(() => !!token.value)

  function setAuth(res: AuthResponse) {
    token.value = res.token
    email.value = res.email
    role.value = res.role
    localStorage.setItem('mpp_token', res.token)
    localStorage.setItem('mpp_email', res.email)
    localStorage.setItem('mpp_role', res.role)
  }

  async function login(dto: LoginDto): Promise<void> {
    const res = await loginService(dto)
    setAuth(res)
    await startSignalR(res.token)
  }

  function logout() {
    token.value = null
    email.value = null
    role.value = null
    localStorage.removeItem('mpp_token')
    localStorage.removeItem('mpp_email')
    localStorage.removeItem('mpp_role')
    stopSignalR()
  }

  // Re-connect SignalR on page reload if already logged in
  async function initSignalR() {
    if (token.value) {
      try { await startSignalR(token.value) } catch { /* reconnect will handle */ }
    }
  }

  return { token, email, role, isAuthenticated, login, logout, initSignalR }
})
