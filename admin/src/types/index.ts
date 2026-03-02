/**
 * Admin-specific type definitions
 * Imports shared types and adds admin-only extensions
 */

// Re-export all shared types for backward compatibility
export type {
  Product,
  Category,
  User,
  AuthResponse,
  ApiResponse,
  ApiError,
  ContactForm,
  ProductCategory,
  LoginRequest,
  RegisterRequest
} from '@powersports/shared-types';

export { UserRole } from '@powersports/shared-types';

// ============================================================================
// ADMIN-SPECIFIC TYPES
// ============================================================================

// Site settings for dynamic content management
export interface SiteSetting {
  id: number;
  key: string;
  displayName: string;
  value: string;
  settingType: SettingType;
  category: string;
  description?: string;
  lastModifiedBy: number;
  lastModifiedAt: string;
}

export enum SettingType {
  Text = 0,
  TextArea = 1,
  Number = 2,
  Boolean = 3,
  Email = 4,
  Phone = 5,
  Url = 6
}

// Create/update DTOs for admin operations
export interface CreateProductRequest {
  name: string;
  description: string;
  price: number;
  category: string;
  imageUrl: string;
}

export interface UpdateProductRequest extends CreateProductRequest {
  id: number;
}

export interface CreateUserRequest {
  firstName: string;
  lastName: string;
  email: string;
  password: string;
  phone?: string;
  role: UserRole;
}

// Full user model returned by admin endpoints (includes fields not in shared User)
export interface AdminUser {
  id: number;
  firstName: string;
  lastName: string;
  email: string;
  phone?: string;
  role: UserRole;
  roleName: string;
  createdAt: string;
  lastLoginAt?: string;
  lastLoginIp?: string;
  isActive: boolean;
  subscribeNewsletter?: boolean;
}

export interface UpdateUserInfoRequest {
  firstName: string;
  lastName: string;
  email: string;
  phone?: string;
  subscribeNewsletter: boolean;
}

export interface UpdateUserRequest {
  id: number;
  firstName: string;
  lastName: string;
  email: string;
  phone?: string;
  role: UserRole;
}

export interface UpdateUserPasswordRequest {
  id: number;
  newPassword: string;
}

export interface UpdateSiteSettingRequest {
  id: number;
  value: string;
}

// Dashboard statistics
export interface RecentUser {
  id: number;
  firstName: string;
  lastName: string;
  email: string;
  roleName: string;
  createdAt: string;
}

export interface DashboardStats {
  totalProducts: number;
  inactiveProducts: number;
  featuredProducts: number;
  totalCategories: number;
  totalUsers: number;
  totalAdmins: number;
  recentRegistrations: number;
  recentUsers: RecentUser[];
  generatedAt: string;
}