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

// Admin-Specific Types

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
  lastSeenAt?: string;
  lastLoginIp?: string;
  isActive: boolean;
  subscribeNewsletter?: boolean;
  isEmailVerified: boolean;
}

export interface UpdateUserInfoRequest {
  firstName: string;
  lastName: string;
  email: string;
  phone?: string;
  subscribeNewsletter: boolean;
  isEmailVerified: boolean;
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
  totalOrders: number;
  pendingOrders: number;
  totalRevenue: number;
  newInquiries: number;
  recentUsers: RecentUser[];
  generatedAt: string;
}

// Appointments
export interface Appointment {
  id: number;
  startTime: string;
  endTime: string;
  customerName: string;
  customerEmail?: string;
  customerPhone?: string;
  serviceType?: string;
  notes?: string;
  status: AppointmentStatus;
  userId?: number;
  user?: {
    id: number;
    firstName: string;
    lastName: string;
    email: string;
  };
  createdAt: string;
  updatedAt?: string;
  createdBy?: {
    firstName: string;
    lastName: string;
  };
}

export type AppointmentStatus = 'Scheduled' | 'Completed' | 'Cancelled' | 'NoShow';

export interface CreateAppointmentRequest {
  startTime: string;
  endTime: string;
  customerName: string;
  customerEmail?: string;
  customerPhone?: string;
  serviceType?: string;
  notes?: string;
  userId?: number;
}

export interface UpdateAppointmentRequest extends CreateAppointmentRequest {
  status: AppointmentStatus;
}

export interface UpdateAppointmentStatusRequest {
  status: AppointmentStatus;
}

export interface AppointmentUser {
  id: number;
  firstName: string;
  lastName: string;
  email: string;
  phone?: string;
  fullName: string;
}
