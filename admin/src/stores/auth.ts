import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import type { User, AuthResponse } from '@/types'

const API_URL = `${import.meta.env.VITE_API_URL || 'http://localhost:5226'}/api/v1`

export const useAuthStore = defineStore('auth', () => {
  const token = ref<string | null>(localStorage.getItem('admin_token'))
  const refreshToken = ref<string | null>(localStorage.getItem('admin_refresh_token'))
  const user = ref<User | null>(null)

  const isAuthenticated = computed(() => !!token.value && !!user.value)
  const isAdmin = computed(() => user.value?.role === 'Admin' || user.value?.role === 'SuperAdmin' || user.value?.role === 1 || user.value?.role === 2) // Admin or SuperAdmin
  const isSuperAdmin = computed(() => user.value?.role === 'SuperAdmin' || user.value?.role === 2) // SuperAdmin only
  const hasAdminAccess = computed(() => isAdmin.value || isSuperAdmin.value)

  // Check for token in URL parameters (for auto-login from main site)
  const checkForTokenInUrl = () => {
    const urlParams = new URLSearchParams(window.location.search)
    const urlToken = urlParams.get('token')
    const urlRefreshToken = urlParams.get('refresh')
    
    if (urlToken) {
      setToken(urlToken, urlRefreshToken || undefined)
      // Remove token from URL for security
      const newUrl = window.location.origin + window.location.pathname
      window.history.replaceState({}, document.title, newUrl)
      return true
    }
    return false
  }

  // Restore auth state from localStorage
  const initializeAuth = async () => {
    const hasUrlToken = checkForTokenInUrl()
    
    if (token.value) {
      // If we arrived via URL token, immediately refresh to get a full-lifetime token
      // This prevents sign-out when the passed token is near its expiry
      if (hasUrlToken) {
        const refreshed = await refreshAccessToken()
        if (!refreshed) {
          // Refresh failed but we have the URL token — try to validate it directly
          try {
            await fetchUserProfile()
          } catch {
            logout()
          }
        }
        return
      }
      
      // On normal page load, try silent refresh first, then fall back to validating stored token
      const refreshed = await refreshAccessToken()
      if (refreshed) return
      
      try {
        await fetchUserProfile()
      } catch (error) {
        logout()
      }
    }
  }

  const setToken = (newToken: string, newRefreshToken?: string) => {
    token.value = newToken
    localStorage.setItem('admin_token', newToken)
    
    if (newRefreshToken) {
      refreshToken.value = newRefreshToken
      localStorage.setItem('admin_refresh_token', newRefreshToken)
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
    if (!role || (role !== 'Admin' && role !== 'SuperAdmin' && role !== 1 && role !== 2)) {
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
    if (!role || (role !== 'Admin' && role !== 'SuperAdmin' && role !== 1 && role !== 2)) {
      throw new Error('Insufficient permissions for admin dashboard')
    }

    setToken(authData.token, authData.refreshToken)
    user.value = authData.user
  }

  const logout = () => {
    token.value = null
    refreshToken.value = null
    user.value = null
    localStorage.removeItem('admin_token')
    localStorage.removeItem('admin_refresh_token')
  }

  const refreshAccessToken = async (): Promise<boolean> => {
    const rt = refreshToken.value || localStorage.getItem('admin_refresh_token')
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
    refreshAccessToken,
    checkForTokenInUrl
  }
})