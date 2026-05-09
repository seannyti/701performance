import { describe, it, expect, beforeEach, vi } from 'vitest'
import { setActivePinia, createPinia } from 'pinia'
import { usePortalAuthStore } from './auth'
import api from '../services/api'

vi.mock('../services/api', () => ({
  default: {
    post: vi.fn(),
    get:  vi.fn(),
  },
}))

const mockAuthData = {
  accessToken: 'mock-portal-token',
  role:        'admin',
  userId:      1,
  firstName:   'Admin',
  lastName:    'User',
  email:       'admin@example.com',
}

describe('portal auth store', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
    localStorage.clear()
    vi.clearAllMocks()
  })

  it('starts unauthenticated when no token in localStorage', () => {
    const store = usePortalAuthStore()
    expect(store.isLoggedIn).toBe(false)
    expect(store.user).toBeNull()
  })

  it('login stores token, user, and persists to localStorage', async () => {
    vi.mocked(api.post).mockResolvedValue({ data: mockAuthData })
    const store = usePortalAuthStore()

    await store.login('admin@example.com', 'password')

    expect(store.token).toBe('mock-portal-token')
    expect(store.user?.email).toBe('admin@example.com')
    expect(store.isLoggedIn).toBe(true)
    expect(localStorage.getItem('portal_access_token')).toBe('mock-portal-token')
  })

  it('isAdmin is true for admin role', async () => {
    vi.mocked(api.post).mockResolvedValue({ data: mockAuthData })
    const store = usePortalAuthStore()
    await store.login('admin@example.com', 'password')

    expect(store.isAdmin).toBe(true)
  })

  it('isAdmin is false for non-admin role', async () => {
    vi.mocked(api.post).mockResolvedValue({ data: { ...mockAuthData, role: 'sales' } })
    const store = usePortalAuthStore()
    await store.login('sales@example.com', 'password')

    expect(store.isAdmin).toBe(false)
  })

  it('hasRole matches any of the provided roles', async () => {
    vi.mocked(api.post).mockResolvedValue({ data: { ...mockAuthData, role: 'fi' } })
    const store = usePortalAuthStore()
    await store.login('fi@example.com', 'password')

    expect(store.hasRole('sales', 'fi', 'admin')).toBe(true)
    expect(store.hasRole('admin', 'service')).toBe(false)
  })

  it('fullName combines firstName and lastName', async () => {
    vi.mocked(api.post).mockResolvedValue({ data: mockAuthData })
    const store = usePortalAuthStore()
    await store.login('admin@example.com', 'password')

    expect(store.fullName).toBe('Admin User')
  })

  it('initials returns first letter of each name', async () => {
    vi.mocked(api.post).mockResolvedValue({ data: mockAuthData })
    const store = usePortalAuthStore()
    await store.login('admin@example.com', 'password')

    expect(store.initials).toBe('AU')
  })

  it('logout clears state and localStorage', async () => {
    vi.mocked(api.post).mockResolvedValue({ data: mockAuthData })
    const store = usePortalAuthStore()
    await store.login('admin@example.com', 'password')

    vi.mocked(api.post).mockResolvedValue({})
    await store.logout()

    expect(store.token).toBeNull()
    expect(store.user).toBeNull()
    expect(store.isLoggedIn).toBe(false)
    expect(localStorage.getItem('portal_access_token')).toBeNull()
  })

  it('logout succeeds even if API call fails', async () => {
    vi.mocked(api.post).mockResolvedValueOnce({ data: mockAuthData })
    const store = usePortalAuthStore()
    await store.login('admin@example.com', 'password')

    vi.mocked(api.post).mockRejectedValue(new Error('network error'))
    await store.logout() // should not throw

    expect(store.isLoggedIn).toBe(false)
  })

  it('fetchMe populates user from API', async () => {
    localStorage.setItem('portal_access_token', 'existing-token')
    vi.mocked(api.get).mockResolvedValue({
      data: { id: 2, firstName: 'Sales', lastName: 'Rep', email: 'sales@example.com', role: 'sales' },
    })
    const store = usePortalAuthStore()

    await store.fetchMe()

    expect(store.user?.email).toBe('sales@example.com')
    expect(store.user?.role).toBe('sales')
  })

  it('fetchMe clears token when API returns an error', async () => {
    localStorage.setItem('portal_access_token', 'expired-token')
    vi.mocked(api.get).mockRejectedValue(new Error('401'))
    const store = usePortalAuthStore()

    await store.fetchMe()

    expect(store.token).toBeNull()
    expect(localStorage.getItem('portal_access_token')).toBeNull()
  })

  it('fetchMe does nothing when no token exists', async () => {
    const store = usePortalAuthStore()
    await store.fetchMe()

    expect(api.get).not.toHaveBeenCalled()
  })
})
