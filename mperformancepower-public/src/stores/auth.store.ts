import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import { login as loginService, register as registerService } from '@/services/auth.service'
import type { LoginDto, RegisterDto, AuthResponse } from '@/types/auth.types'

export const useAuthStore = defineStore('auth', () => {
  const token = ref<string | null>(localStorage.getItem('mpp_token'))
  const email = ref<string | null>(localStorage.getItem('mpp_email'))
  const role  = ref<string | null>(localStorage.getItem('mpp_role'))
  const firstName = ref<string | null>(localStorage.getItem('mpp_firstName'))
  const lastName  = ref<string | null>(localStorage.getItem('mpp_lastName'))

  const isAuthenticated = computed(() => !!token.value)
  const isAdmin         = computed(() => role.value === 'Admin')
  const isCustomer      = computed(() => role.value === 'Customer')
  const fullName        = computed(() => [firstName.value, lastName.value].filter(Boolean).join(' ') || null)

  function applyAuth(res: AuthResponse) {
    token.value     = res.token
    email.value     = res.email
    role.value      = res.role
    firstName.value = res.firstName ?? null
    lastName.value  = res.lastName ?? null
    localStorage.setItem('mpp_token', res.token)
    localStorage.setItem('mpp_email', res.email)
    localStorage.setItem('mpp_role',  res.role)
    if (res.firstName) localStorage.setItem('mpp_firstName', res.firstName)
    else localStorage.removeItem('mpp_firstName')
    if (res.lastName) localStorage.setItem('mpp_lastName', res.lastName)
    else localStorage.removeItem('mpp_lastName')
  }

  async function login(dto: LoginDto): Promise<void> {
    const res = await loginService(dto)
    applyAuth(res)
  }

  /**
   * Returns true if email verification is required (user must check email).
   * Returns false if the user was auto-logged in.
   */
  async function register(dto: RegisterDto): Promise<boolean> {
    const res = await registerService(dto)
    if (res.requiresVerification) return true
    if (res.auth) applyAuth(res.auth)
    return false
  }

  function logout() {
    token.value     = null
    email.value     = null
    role.value      = null
    firstName.value = null
    lastName.value  = null
    localStorage.removeItem('mpp_token')
    localStorage.removeItem('mpp_email')
    localStorage.removeItem('mpp_role')
    localStorage.removeItem('mpp_firstName')
    localStorage.removeItem('mpp_lastName')
    // Clear chat session so the next user starts fresh
    localStorage.removeItem('mpp_chat_session')
    localStorage.removeItem('mpp_chat_name')
  }

  return { token, email, role, firstName, lastName, fullName, isAuthenticated, isAdmin, isCustomer, login, register, logout }
})
