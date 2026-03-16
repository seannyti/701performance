import { useAuthStore } from '@/stores/auth'
import type { Router } from 'vue-router'
import { logError, logDebug } from '@/services/logger'

const API_URL = `${import.meta.env.VITE_API_URL || 'http://localhost:5226'}/api/v1`

// Global router instance - will be set during app initialization
let routerInstance: Router | null = null

export function setRouterInstance(router: Router) {
  routerInstance = router
}

/**
 * Custom API client that automatically handles authentication and 401 responses
 */
export async function apiClient(
  endpoint: string,
  options: RequestInit = {}
): Promise<Response> {
  const authStore = useAuthStore()
  
  // Add Authorization header if token exists
  const headers = {
    ...options.headers,
    ...(authStore.token ? { 'Authorization': `Bearer ${authStore.token}` } : {})
  }

  const url = endpoint.startsWith('http') ? endpoint : `${API_URL}${endpoint}`
  
  logDebug('API request', { 
    method: options.method || 'GET', 
    url,
    hasAuth: !!authStore.token 
  })

  try {
    const response = await fetch(url, {
      ...options,
      headers
    })

    // Handle 401 Unauthorized - token expired or invalid
    if (response.status === 401) {
      // Try to refresh the token first before logging out
      const refreshed = await authStore.refreshAccessToken()
      if (refreshed) {
        // Retry the original request with the new token
        const retryHeaders = {
          ...options.headers,
          ...(authStore.token ? { 'Authorization': `Bearer ${authStore.token}` } : {})
        }
        const retryResponse = await fetch(url, { ...options, headers: retryHeaders })
        if (retryResponse.status !== 401) {
          return retryResponse
        }
      }

      logError('Authentication failed - logging out', new Error('401 Unauthorized'), { url })
      
      // Clear auth state
      authStore.logout()
      
      // Show notification (the navigation will handle showing the message)
      localStorage.setItem('auth_logout_reason', 'Your session has expired. Please log in again.')
      
      // Redirect to login using the router instance
      if (routerInstance) {
        routerInstance.push('/login')
      } else {
        // Fallback to hard redirect
        window.location.href = '/login'
      }
      
      throw new Error('Session expired. Please log in again.')
    }

    return response
  } catch (error) {
    // If it's a network error or other fetch error, log it
    if (error instanceof Error && error.message !== 'Session expired. Please log in again.') {
      logError('API request failed', error, { url })
    }
    throw error
  }
}

/**
 * Helper for GET requests
 */
export async function apiGet<T = any>(endpoint: string): Promise<T> {
  const response = await apiClient(endpoint)
  if (!response.ok) {
    const errorText = await response.text()
    let errorMessage = `Request failed: ${response.status}`
    try {
      const errorJson = JSON.parse(errorText)
      errorMessage = errorJson.message || errorMessage
    } catch {
      errorMessage = errorText || errorMessage
    }
    throw new Error(errorMessage)
  }
  return response.json()
}

/**
 * Helper for POST requests
 */
export async function apiPost<T = any>(
  endpoint: string,
  data?: any
): Promise<T> {
  const response = await apiClient(endpoint, {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json'
    },
    body: data ? JSON.stringify(data) : undefined
  })
  
  if (!response.ok) {
    const errorText = await response.text()
    let errorMessage = `Request failed: ${response.status}`
    try {
      const errorJson = JSON.parse(errorText)
      errorMessage = errorJson.message || errorMessage
    } catch {
      errorMessage = errorText || errorMessage
    }
    throw new Error(errorMessage)
  }
  
  return response.json()
}

/**
 * Helper for PUT requests
 */
export async function apiPut<T = any>(
  endpoint: string,
  data?: any
): Promise<T> {
  const response = await apiClient(endpoint, {
    method: 'PUT',
    headers: {
      'Content-Type': 'application/json'
    },
    body: data ? JSON.stringify(data) : undefined
  })
  
  if (!response.ok) {
    const errorText = await response.text()
    let errorMessage = `Request failed: ${response.status}`
    try {
      const errorJson = JSON.parse(errorText)
      errorMessage = errorJson.message || errorMessage
    } catch {
      errorMessage = errorText || errorMessage
    }
    throw new Error(errorMessage)
  }
  
  return response.json()
}

/**
 * Helper for DELETE requests
 */
export async function apiDelete(endpoint: string): Promise<void> {
  const response = await apiClient(endpoint, {
    method: 'DELETE'
  })
  
  if (!response.ok) {
    const errorText = await response.text()
    let errorMessage = `Request failed: ${response.status}`
    try {
      const errorJson = JSON.parse(errorText)
      errorMessage = errorJson.message || errorMessage
    } catch {
      errorMessage = errorText || errorMessage
    }
    throw new Error(errorMessage)
  }
}
