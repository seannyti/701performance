/**
 * Shared type definitions for Powersports Showcase
 * Used by both frontend and admin applications
 */

// ============================================================================
// PRODUCT TYPES
// ============================================================================

export interface Product {
  id: number;
  name: string;
  description: string;
  price: number;
  category: string;
  categoryId: number;
  imageUrl: string;
  isFeatured: boolean;
  isActive: boolean;
  // Inventory fields
  sku?: string | null;
  stockQuantity: number;
  lowStockThreshold: number;
  costPrice?: number | null;
  createdAt?: string;
  updatedAt?: string;
}

export type ProductCategory = 'ATV' | 'Dirtbike' | 'UTV' | 'Snowmobile' | 'Gear';

// ============================================================================
// CATEGORY TYPES
// ============================================================================

export interface Category {
  id: number;
  name: string;
  description: string;
  imageUrl?: string;
  isActive: boolean;
  createdAt: string;
  updatedAt: string;
}

// ============================================================================
// USER & AUTHENTICATION TYPES
// ============================================================================

export enum UserRole {
  User = 0,
  Admin = 1,
  SuperAdmin = 2
}

export interface User {
  id: number;
  firstName: string;
  lastName: string;
  email: string;
  phone?: string;
  fullName: string;
  role: UserRole;
  roleName: string;
  createdAt: string;
}

export interface AuthResponse {
  token: string;
  refreshToken: string;
  expiresAt: string;
  user: User;
  requiresEmailVerification?: boolean;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface RegisterRequest {
  firstName: string;
  lastName: string;
  email: string;
  password: string;
  phone?: string;
}

// ============================================================================
// API RESPONSE TYPES
// ============================================================================

export interface ApiResponse<T> {
  data: T;
  success: boolean;
  message?: string;
}

export interface ApiError {
  message: string;
  errors?: Record<string, string[]>;
  statusCode?: number;
}

// ============================================================================
// FORM TYPES
// ============================================================================

export interface ContactForm {
  name: string;
  email: string;
  message: string;
}
