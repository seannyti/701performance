/**
 * Shared type definitions for Powersports Showcase
 * Used by both frontend and admin applications
 */

// Product Types
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
  specifications?: string | null;
  createdAt?: string;
  updatedAt?: string;
  // Image gallery
  productImages?: ProductImageInfo[];
}

export interface ProductImageInfo {
  id: number;
  productId: number;
  mediaFileId: number;
  isMain: boolean;
  sortOrder: number;
  url: string;
  thumbnailUrl?: string;
}

export type ProductCategory = 'ATV' | 'Dirtbike' | 'UTV' | 'Snowmobile' | 'Gear';

// Category Types
export interface Category {
  id: number;
  name: string;
  description: string;
  imageUrl?: string;
  isActive: boolean;
  createdAt: string;
  updatedAt: string;
}

// User & Authentication Types
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
  role: UserRole | string; // Support both enum number and string from API
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

// API Response Types
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

// Form Types
export interface ContactForm {
  name: string;
  email: string;
  subject?: string;
  message: string;
}

// Logger Utilities
export { Logger, LogLevel, createLogger, createLoggerFunctions } from './logger';