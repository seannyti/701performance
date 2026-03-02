import { defineStore } from 'pinia';
import { ref, computed } from 'vue';
import axios from 'axios';
import type { User, AuthResponse } from '@/types';
import { UserRole } from '@/types';

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
  // State
  const user = ref<User | null>(null);
  const token = ref<string | null>(localStorage.getItem('auth_token'));
  const isLoading = ref(false);
  const error = ref<string | null>(null);

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
    user.value?.role === UserRole.Admin || user.value?.role === UserRole.SuperAdmin
  );
  const isSuperAdmin = computed(() => user.value?.role === UserRole.SuperAdmin);
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

      const { token: authToken, user: userData } = response.data;
      
      token.value = authToken;
      user.value = userData;
      
      if (rememberMe || true) {
        localStorage.setItem('auth_token', authToken);
      }
      
      axios.defaults.headers.common['Authorization'] = `Bearer ${authToken}`;
      
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
        phone: signupData.phone
      });

      if (response.data.requiresEmailVerification) {
        return { requiresEmailVerification: true };
      }

      const { token: authToken, user: userData } = response.data;
      
      token.value = authToken;
      user.value = userData;
      localStorage.setItem('auth_token', authToken);
      
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

  const logout = async (): Promise<string | null> => {
    user.value = null;
    token.value = null;
    error.value = null;
    localStorage.removeItem('auth_token');
    
    delete axios.defaults.headers.common['Authorization'];
    
    try {
      const response = await axios.get(`${import.meta.env.VITE_API_URL || 'http://localhost:5226'}/api/v1/settings`);
      if (response.data) {
        const maintenanceSetting = response.data.find((s: any) => s.key === 'enable_maintenance_mode');
        if (maintenanceSetting?.value === 'true') {
          return '/maintenance';
        }
      }
    } catch (err) {
      // Silently handle error
    }
    
    return null;
  };

  const checkAuth = async () => {
    const storedToken = localStorage.getItem('auth_token');
    
    if (!storedToken) {
      return;
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
      
    } catch (err: any) {
      if (err.response?.status === 401) {
        logout();
      }
    }
  };

  const clearError = () => {
    error.value = null;
  };

  // Initialize auth on store creation
  checkAuth();

  return {
    user,
    token,
    isLoading,
    isAuthenticated,
    fullName,
    error,
    login,
    signup,
    logout,
    checkAuth,
    clearError,
    // Role checking
    isAdmin,
    isSuperAdmin,
    hasAdminAccess
  };
});