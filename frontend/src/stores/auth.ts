import { defineStore } from 'pinia';
import { ref, computed } from 'vue';
import axios from 'axios';
import type { User, AuthResponse } from '@/types';
import { UserRole } from '@/types';
import { useToast } from '@/composables/useToast';

const API_URL = `${import.meta.env.VITE_API_URL || 'http://localhost:5226'}/api/v1`;

interface SignupData {
  firstName: string;
  lastName: string;
  email: string;
  password: string;
  phone?: string;
  subscribeNewsletter?: boolean;
}

export const useAuthStore = defineStore('auth', () => {
  // Migrate any tokens that were previously stored in localStorage to sessionStorage,
  // then clear them so they no longer survive a browser close.
  const _migratedToken = localStorage.getItem('auth_token')
  const _migratedRefresh = localStorage.getItem('refresh_token')
  if (_migratedToken) {
    sessionStorage.setItem('auth_token', _migratedToken)
    localStorage.removeItem('auth_token')
  }
  if (_migratedRefresh) {
    sessionStorage.setItem('refresh_token', _migratedRefresh)
    localStorage.removeItem('refresh_token')
  }

  // State
  const user = ref<User | null>(null);
  const token = ref<string | null>(sessionStorage.getItem('auth_token'));
  const refreshToken = ref<string | null>(sessionStorage.getItem('refresh_token'));
  const isLoading = ref(false);
  const error = ref<string | null>(null);
  let refreshTimer: ReturnType<typeof setTimeout> | null = null;

  // Configure axios defaults
  if (token.value) {
    axios.defaults.headers.common['Authorization'] = `Bearer ${token.value}`;
  }

  // Getters
  const isAuthenticated = computed(() => !!token.value && !!user.value);
  const fullName = computed(() => {
    if (!user.value) return '';
    return `${user.value.firstName} ${user.value.lastName}`;
  });
  
  // Role checking getters
  const isAdmin = computed(() => 
    user.value?.role === 'Admin' || user.value?.role === 'SuperAdmin' || 
    user.value?.role === UserRole.Admin || user.value?.role === UserRole.SuperAdmin
  );
  const isSuperAdmin = computed(() => 
    user.value?.role === 'SuperAdmin' || user.value?.role === UserRole.SuperAdmin
  );
  const hasAdminAccess = computed(() => isAdmin.value || isSuperAdmin.value);

  // Actions
  const login = async (email: string, password: string, rememberMe: boolean = false) => {
    isLoading.value = true;
    error.value = null;
    
    try {
      const response = await axios.post<AuthResponse>(`${API_URL}/auth/login`, {
        email,
        password
      });

      const { token: authToken, user: userData, refreshToken: rt, expiresAt } = response.data;
      
      token.value = authToken;
      user.value = userData;
      refreshToken.value = rt;

      sessionStorage.setItem('auth_token', authToken);
      if (rt) sessionStorage.setItem('refresh_token', rt);
      
      axios.defaults.headers.common['Authorization'] = `Bearer ${authToken}`;
      scheduleRefresh(expiresAt);
      
    } catch (err: any) {
      // Surface email-not-verified as a recognizable message
      if (err.response?.status === 403 && err.response?.data?.message === 'EMAIL_NOT_VERIFIED') {
        const message = 'EMAIL_NOT_VERIFIED';
        error.value = message;
        throw new Error(message);
      }
      const message = err.response?.data?.message || 'Login failed. Please check your credentials.';
      error.value = message;
      throw new Error(message);
    } finally {
      isLoading.value = false;
    }
  };

  const signup = async (signupData: SignupData): Promise<{ requiresEmailVerification: boolean }> => {
    isLoading.value = true;
    error.value = null;
    
    try {
      const response = await axios.post<AuthResponse>(`${API_URL}/auth/register`, {
        firstName: signupData.firstName,
        lastName: signupData.lastName,
        email: signupData.email,
        password: signupData.password,
        phone: signupData.phone,
        subscribeNewsletter: signupData.subscribeNewsletter ?? false
      });

      if (response.data.requiresEmailVerification) {
        return { requiresEmailVerification: true };
      }

      const { token: authToken, user: userData, refreshToken: rt } = response.data;
      
      token.value = authToken;
      user.value = userData;
      refreshToken.value = rt;
      sessionStorage.setItem('auth_token', authToken);
      if (rt) sessionStorage.setItem('refresh_token', rt);
      
      axios.defaults.headers.common['Authorization'] = `Bearer ${authToken}`;
      return { requiresEmailVerification: false };
      
    } catch (err: any) {
      const message = err.response?.data?.message || 'Registration failed. Please try again.';
      error.value = message;
      throw new Error(message);
    } finally {
      isLoading.value = false;
    }
  };

  const logout = () => {
    if (refreshTimer) { clearTimeout(refreshTimer); refreshTimer = null; }
    user.value = null;
    token.value = null;
    refreshToken.value = null;
    error.value = null;
    sessionStorage.removeItem('auth_token');
    sessionStorage.removeItem('refresh_token');
    delete axios.defaults.headers.common['Authorization'];
  };

  // Returns true if the JWT access token expires within the next 5 minutes
  const isTokenNearExpiry = (t: string): boolean => {
    try {
      const payload = JSON.parse(atob(t.split('.')[1]));
      return Date.now() > (payload.exp * 1000) - 5 * 60_000;
    } catch { return true; }
  };

  const checkAuth = async () => {
    const storedToken = sessionStorage.getItem('auth_token');
    
    if (!storedToken) {
      return;
    }

    // Only consume the refresh token if the access token is actually near expiry.
    // Refreshing eagerly on every page load causes race conditions when multiple
    // clients (frontend + admin) share the same refresh token.
    const storedRefresh = sessionStorage.getItem('refresh_token');
    if (storedRefresh && isTokenNearExpiry(storedToken)) {
      const refreshed = await silentRefresh(storedRefresh);
      if (refreshed) return;
    }

    try {
      const response = await axios.get<User>(`${API_URL}/auth/me`, {
        headers: {
          Authorization: `Bearer ${storedToken}`
        }
      });

      token.value = storedToken;
      user.value = response.data;
      
      axios.defaults.headers.common['Authorization'] = `Bearer ${storedToken}`;

      // Schedule a refresh based on the token's actual expiry
      try {
        const payload = JSON.parse(atob(storedToken.split('.')[1]));
        scheduleRefresh(new Date(payload.exp * 1000).toISOString());
      } catch { }
      
    } catch (err: any) {
      if (err.response?.status === 401) {
        // Token expired — try to refresh before giving up
        if (storedRefresh) {
          const refreshed = await silentRefresh(storedRefresh);
          if (!refreshed) {
            logout();
            useToast().warning('Your session has expired. Please log in again.');
          }
        } else {
          logout();
          useToast().warning('Your session has expired. Please log in again.');
        }
      }
    }
  };

  const silentRefresh = async (rt: string): Promise<boolean> => {
    try {
      const response = await axios.post<AuthResponse>(`${API_URL}/auth/refresh-token`, { refreshToken: rt });
      const { token: newToken, refreshToken: newRt, user: userData, expiresAt } = response.data;
      token.value = newToken;
      refreshToken.value = newRt;
      user.value = userData;
      sessionStorage.setItem('auth_token', newToken);
      if (newRt) sessionStorage.setItem('refresh_token', newRt);
      axios.defaults.headers.common['Authorization'] = `Bearer ${newToken}`;
      scheduleRefresh(expiresAt);
      return true;
    } catch {
      return false;
    }
  };

  const scheduleRefresh = (expiresAt: string | Date) => {
    if (refreshTimer) clearTimeout(refreshTimer);
    const expiryMs = new Date(expiresAt).getTime() - Date.now();
    // Refresh at 80% of the token's lifetime (leaves 20% buffer)
    const refreshIn = Math.max(expiryMs * 0.8, 30_000);
    refreshTimer = setTimeout(async () => {
      const rt = refreshToken.value || sessionStorage.getItem('refresh_token');
      if (rt) await silentRefresh(rt);
    }, refreshIn);
  };

  const clearError = () => {
    error.value = null;
  };

  // Initialize auth on store creation
  checkAuth();

  // Heartbeat: ping /auth/me every 2 minutes to keep LastSeenAt fresh
  setInterval(() => {
    if (token.value) {
      axios.get(`${API_URL}/auth/me`, {
        headers: { Authorization: `Bearer ${token.value}` }
      }).catch(() => {});
    }
  }, 2 * 60_000);

  return {
    user,
    token,
    refreshToken,
    isLoading,
    isAuthenticated,
    fullName,
    error,
    login,
    signup,
    logout,
    checkAuth,
    silentRefresh,
    clearError,
    // Role checking
    isAdmin,
    isSuperAdmin,
    hasAdminAccess
  };
});