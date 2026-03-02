# @powersports/shared-types

Shared TypeScript type definitions for the Powersports Showcase application.

## Usage

This package contains all shared types used across the frontend and admin applications, ensuring type consistency and reducing duplication.

### Included Types

- **Product Types**: `Product`, `ProductCategory`
- **Category Types**: `Category`
- **User Types**: `User`, `UserRole`, `AuthResponse`
- **API Types**: `ApiResponse<T>`, `ApiError`
- **Form Types**: `ContactForm`, `LoginRequest`, `RegisterRequest`

### Import Examples

```typescript
import { Product, User, AuthResponse, UserRole } from '@powersports/shared-types';
```

## Development

```bash
# Type checking
npm run typecheck
```

## Version

Current version: 1.0.0
