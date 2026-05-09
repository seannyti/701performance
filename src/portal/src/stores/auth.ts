import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import api from '../services/api'

export const usePortalAuthStore = defineStore('portalAuth', () => {
  const user = ref<{
    id: number
    firstName: string
    lastName: string
    email: string
    role: string
  } | null>(null)

  const token = ref<string | null>(localStorage.getItem('portal_access_token'))

  const isLoggedIn = computed(() => !!token.value && !!user.value)
  const isAdmin = computed(() => user.value?.role === 'admin')
  const fullName = computed(() => user.value ? `${user.value.firstName} ${user.value.lastName}` : '')
  const initials = computed(() => user.value ? `${user.value.firstName[0]}${user.value.lastName[0]}` : '')

  function hasRole(...roles: string[]) {
    return user.value ? roles.includes(user.value.role) : false
  }

  async function login(email: string, password: string) {
    const { data } = await api.post('/api/auth/login', { email, password })
    if (data.mfaRequired) return data // caller handles MFA step
    token.value = data.accessToken
    localStorage.setItem('portal_access_token', data.accessToken)
    user.value = {
      id: data.userId,
      firstName: data.firstName,
      lastName: data.lastName,
      email: data.email,
      role: data.role
    }
    return data
  }

  async function completeMfaLogin(mfaToken: string, code: string) {
    const { data } = await api.post('/api/auth/mfa/verify-login', { mfaToken, code })
    token.value = data.accessToken
    localStorage.setItem('portal_access_token', data.accessToken)
    user.value = {
      id: data.userId,
      firstName: data.firstName,
      lastName: data.lastName,
      email: data.email,
      role: data.role
    }
    return data
  }

  async function logout() {
    await api.post('/api/auth/logout').catch(() => {})
    token.value = null
    user.value = null
    localStorage.removeItem('portal_access_token')
  }

  async function fetchMe() {
    if (!token.value) return
    try {
      const { data } = await api.get('/api/auth/me')
      user.value = {
        id: data.id,
        firstName: data.firstName,
        lastName: data.lastName,
        email: data.email,
        role: data.role
      }
    } catch {
      token.value = null
      localStorage.removeItem('portal_access_token')
    }
  }

  return { user, token, isLoggedIn, isAdmin, fullName, initials, hasRole, login, completeMfaLogin, logout, fetchMe }
})
