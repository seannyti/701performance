/**
 * Frontend-specific type definitions
 * Imports shared types and adds frontend-only extensions
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

// Frontend-specific types can be added here
// Example: UI state, view models, etc.