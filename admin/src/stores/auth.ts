import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import type { User, AuthResponse } from '@/types'

const API_URL = `${import.meta.env.VITE_API_URL || 'http://localhost:5226'}/api/v1`

export const useAuthStore = defineStore('auth', () => {
  // Migrate any tokens previously in localStorage so old sessions don't persist after browser close.
  const _mt = localStorage.getItem('admin_token')
  const _mr = localStorage.getItem('admin_refresh_token')
  if (_mt) { sessionStorage.setItem('admin_token', _mt); localStorage.removeItem('admin_token') }
  if (_mr) { sessionStorage.setItem('admin_refresh_token', _mr); localStorage.removeItem('admin_refresh_token') }

  const token = ref<string | null>(sessionStorage.getItem('admin_token'))
  const refreshToken = ref<string | null>(sessionStorage.getItem('admin_refresh_token'))
  const user = ref<User | null>(null)

  const isAuthenticated = computed(() => !!token.value && !!user.value)
  const isAdmin = computed(() => user.value?.role === 'Admin' || user.value?.role === 'SuperAdmin')
  const isSuperAdmin = computed(() => user.value?.role === 'SuperAdmin')
  const hasAdminAccess = computed(() => isAdmin.value || isSuperAdmin.value)

  // Returns true if the JWT access token expires within the next 5 minutes
  const isTokenNearExpiry = (t: string): boolean => {
    try {
      const payload = JSON.parse(atob(t.split('.')[1]))
      return Date.now() > (payload.exp * 1000) - 5 * 60_000
    } catch { return true }
  }

  // Schedule a token refresh at 80% of the token's remaining lifetime
  let refreshTimer: ReturnType<typeof setTimeout> | undefined
  const scheduleRefresh = (t: string) => {
    if (refreshTimer) clearTimeout(refreshTimer)
    try {
      const payload = JSON.parse(atob(t.split('.')[1]))
      const expiryMs = payload.exp * 1000
      const refreshIn = Math.max((expiryMs - Date.now()) * 0.8, 30_000)
      refreshTimer = setTimeout(async () => {
        const refreshed = await refreshAccessToken()
        if (!refreshed) logout()
      }, refreshIn)
    } catch { }
  }

  // Restore auth state from sessionStorage
  const initializeAuth = async () => {
    if (token.value) {
      // Normal page load / reload — only refresh if the access token is near expiry
      if (isTokenNearExpiry(token.value)) {
        const refreshed = await refreshAccessToken()
        if (refreshed) return
      }

      try {
        await fetchUserProfile()
        scheduleRefresh(token.value)
      } catch {
        logout()
      }
    }
  }

  const setToken = (newToken: string, newRefreshToken?: string) => {
    token.value = newToken
    sessionStorage.setItem('admin_token', newToken)
    
    if (newRefreshToken) {
      refreshToken.value = newRefreshToken
      sessionStorage.setItem('admin_refresh_token', newRefreshToken)
    }
  }

  const fetchUserProfile = async () => {
    const response = await fetch(`${API_URL}/auth/me`, {
      headers: {
        'Authorization': `Bearer ${token.value}`
      }
    })

    if (!response.ok) {
      throw new Error('Failed to fetch user profile')
    }

    const userData = await response.json()
    user.value = userData
    
    // Verify user has admin access (check both string and numeric role values)
    const role = userData.role
    if (role !== 'Admin' && role !== 'SuperAdmin') {
      throw new Error('Insufficient permissions for admin dashboard')
    }
  }

  const login = async (email: string, password: string): Promise<void> => {
    const response = await fetch(`${API_URL}/auth/login`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json'
      },
      body: JSON.stringify({ email, password })
    })

    if (!response.ok) {
      const errorData = await response.json()
      throw new Error(errorData.message || 'Login failed')
    }

    const authData: AuthResponse = await response.json()
    
    // Verify user has admin access (check both string and numeric role values)
    const role = authData.user.role
    if (role !== 'Admin' && role !== 'SuperAdmin') {
      throw new Error('Insufficient permissions for admin dashboard')
    }

    setToken(authData.token, authData.refreshToken)
    user.value = authData.user
  }

  const logout = () => {
    token.value = null
    refreshToken.value = null
    user.value = null
    sessionStorage.removeItem('admin_token')
    sessionStorage.removeItem('admin_refresh_token')
  }

  const refreshAccessToken = async (): Promise<boolean> => {
      const rt = refreshToken.value || sessionStorage.getItem('admin_refresh_token')
    if (!rt) return false
    try {
      const response = await fetch(`${API_URL}/auth/refresh-token`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ refreshToken: rt })
      })
      if (!response.ok) return false
      const data = await response.json()
      setToken(data.token, data.refreshToken)
      user.value = data.user
      return true
    } catch {
      return false
    }
  }

  return {
    token,
    user,
    isAuthenticated,
    isAdmin,
    isSuperAdmin,
    hasAdminAccess,
    initializeAuth,
    login,
    logout,
    refreshAccessToken
  }
})