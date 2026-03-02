import { describe, it, expect, beforeEach, vi } from 'vitest'
import { setActivePinia, createPinia } from 'pinia'
import { useAuthStore } from '@/stores/auth'
import { UserRole } from '@/types'
import axios from 'axios'

// Mock the entire axios module so the store never makes real HTTP requests
vi.mock('axios', () => ({
  default: {
    post: vi.fn(),
    get: vi.fn(),
    defaults: {
      headers: {
        common: {} as Record<string, string>
      }
    }
  }
}))

const mockedAxios = vi.mocked(axios)

// Helpers to build mock response shapes
const makeAuthResponse = (overrides: Partial<{ role: UserRole }> = {}) => ({
  data: {
    token: 'mock-jwt-token',
    refreshToken: 'mock-refresh-token',
    expiresAt: new Date(Date.now() + 3600_000).toISOString(),
    user: {
      id: 1,
      firstName: 'John',
      lastName: 'Doe',
      email: 'john@example.com',
      role: overrides.role ?? UserRole.User,
      roleName: 'User',
      createdAt: new Date().toISOString()
    }
  }
})

describe('useAuthStore', () => {
  beforeEach(() => {
    // Fresh pinia + cleared axios mocks for every test.
    // localStorage is already cleared by the global setup file.
    setActivePinia(createPinia())
    vi.clearAllMocks()
    // checkAuth() runs on store init but returns early when localStorage is empty
    mockedAxios.get.mockResolvedValue({ data: [] })
  })

  // ─── Initial state ────────────────────────────────────────────────────────

  describe('initial state', () => {
    it('has no user or token by default', () => {
      const store = useAuthStore()

      expect(store.user).toBeNull()
      expect(store.token).toBeNull()
    })

    it('isAuthenticated is false with no credentials', () => {
      const store = useAuthStore()

      expect(store.isAuthenticated).toBe(false)
    })

    it('error is null initially', () => {
      const store = useAuthStore()

      expect(store.error).toBeNull()
    })
  })

  // ─── login ────────────────────────────────────────────────────────────────

  describe('login', () => {
    it('sets token and user on success', async () => {
      mockedAxios.post.mockResolvedValueOnce(makeAuthResponse())
      const store = useAuthStore()

      await store.login('john@example.com', 'password')

      expect(store.token).toBe('mock-jwt-token')
      expect(store.user?.email).toBe('john@example.com')
    })

    it('isAuthenticated becomes true after login', async () => {
      mockedAxios.post.mockResolvedValueOnce(makeAuthResponse())
      const store = useAuthStore()

      await store.login('john@example.com', 'password')

      expect(store.isAuthenticated).toBe(true)
    })

    it('persists token to localStorage', async () => {
      mockedAxios.post.mockResolvedValueOnce(makeAuthResponse())
      const store = useAuthStore()

      await store.login('john@example.com', 'password')

      expect(localStorage.getItem('auth_token')).toBe('mock-jwt-token')
    })

    it('sets Authorization header on axios', async () => {
      mockedAxios.post.mockResolvedValueOnce(makeAuthResponse())
      const store = useAuthStore()

      await store.login('john@example.com', 'password')

      expect(mockedAxios.defaults.headers.common['Authorization']).toBe('Bearer mock-jwt-token')
    })

    it('sets error and throws on failed login', async () => {
      mockedAxios.post.mockRejectedValueOnce({
        response: { data: { message: 'Invalid email or password.' } }
      })
      const store = useAuthStore()

      await expect(store.login('bad@example.com', 'wrong')).rejects.toThrow('Invalid email or password.')
      expect(store.error).toBe('Invalid email or password.')
    })

    it('does not set user on failed login', async () => {
      mockedAxios.post.mockRejectedValueOnce({
        response: { data: { message: 'Invalid email or password.' } }
      })
      const store = useAuthStore()

      try { await store.login('bad@example.com', 'wrong') } catch {}

      expect(store.user).toBeNull()
      expect(store.token).toBeNull()
    })

    it('resets isLoading to false after login regardless of outcome', async () => {
      mockedAxios.post.mockRejectedValueOnce({ response: { data: { message: 'error' } } })
      const store = useAuthStore()

      try { await store.login('a@b.com', 'pw') } catch {}

      expect(store.isLoading).toBe(false)
    })
  })

  // ─── logout ───────────────────────────────────────────────────────────────

  describe('logout', () => {
    it('clears user, token, and localStorage', async () => {
      mockedAxios.post.mockResolvedValueOnce(makeAuthResponse())
      mockedAxios.get.mockResolvedValue({ data: [] })
      const store = useAuthStore()
      await store.login('john@example.com', 'password')

      await store.logout()

      expect(store.user).toBeNull()
      expect(store.token).toBeNull()
      expect(localStorage.getItem('auth_token')).toBeNull()
    })

    it('removes Authorization header from axios', async () => {
      mockedAxios.post.mockResolvedValueOnce(makeAuthResponse())
      mockedAxios.get.mockResolvedValue({ data: [] })
      const store = useAuthStore()
      await store.login('john@example.com', 'password')

      await store.logout()

      expect(mockedAxios.defaults.headers.common['Authorization']).toBeUndefined()
    })
  })

  // ─── Role-based computed properties ───────────────────────────────────────

  describe('role computeds', () => {
    const loginAs = async (role: UserRole) => {
      mockedAxios.post.mockResolvedValueOnce(makeAuthResponse({ role }))
      const store = useAuthStore()
      await store.login('test@example.com', 'password')
      return store
    }

    it('isAdmin is false for regular user', async () => {
      const store = await loginAs(UserRole.User)

      expect(store.isAdmin).toBe(false)
    })

    it('isAdmin is true for admin role', async () => {
      const store = await loginAs(UserRole.Admin)

      expect(store.isAdmin).toBe(true)
    })

    it('isAdmin is true for super admin role', async () => {
      const store = await loginAs(UserRole.SuperAdmin)

      expect(store.isAdmin).toBe(true)
    })

    it('isSuperAdmin is false for admin role', async () => {
      const store = await loginAs(UserRole.Admin)

      expect(store.isSuperAdmin).toBe(false)
    })

    it('isSuperAdmin is true for super admin role', async () => {
      const store = await loginAs(UserRole.SuperAdmin)

      expect(store.isSuperAdmin).toBe(true)
    })

    it('hasAdminAccess is true for both admin and super admin', async () => {
      const adminStore = await loginAs(UserRole.Admin)
      expect(adminStore.hasAdminAccess).toBe(true)

      setActivePinia(createPinia())
      vi.clearAllMocks()
      mockedAxios.get.mockResolvedValue({ data: [] })
      const superAdminStore = await loginAs(UserRole.SuperAdmin)
      expect(superAdminStore.hasAdminAccess).toBe(true)
    })
  })

  // ─── clearError ───────────────────────────────────────────────────────────

  describe('clearError', () => {
    it('resets error to null', async () => {
      mockedAxios.post.mockRejectedValueOnce({ response: { data: { message: 'oops' } } })
      const store = useAuthStore()
      try { await store.login('a@b.com', 'pw') } catch {}
      expect(store.error).toBe('oops')

      store.clearError()

      expect(store.error).toBeNull()
    })
  })

  // ─── fullName computed ────────────────────────────────────────────────────

  describe('fullName', () => {
    it('returns empty string when not logged in', () => {
      const store = useAuthStore()

      expect(store.fullName).toBe('')
    })

    it('returns first + last name when logged in', async () => {
      mockedAxios.post.mockResolvedValueOnce(makeAuthResponse())
      const store = useAuthStore()

      await store.login('john@example.com', 'password')

      expect(store.fullName).toBe('John Doe')
    })
  })
})
