# Photo Upload System Specification

## 📋 Overview
Reusable photo upload system for products, categories, and future profile images with local file storage, auto-resizing, and management features.

## 🎯 Requirements

### File Storage
- **Location**: Local file system in `wwwroot/uploads/`
- **Structure**: 
  - `wwwroot/uploads/products/{productId}/`
  - `wwwroot/uploads/categories/{categoryId}/`
  - `wwwroot/uploads/profiles/{userId}/` (future)
- **Deployment**: Will migrate to Linode storage server

### File Limits & Formats
- **Max Size**: 10MB per file (handles phone photos)
- **Formats**: JPG, PNG, WebP
- **Processing**: Auto-resize to max 1920px width, generate 400px thumbnails
- **Quantity**: 
  - Products: Multiple images (max 10)
  - Categories: Single image
  - Profiles: Single image (future)

## 🗄️ Database Schema

### ProductImages Table
```sql
CREATE TABLE ProductImages (
    Id INT PRIMARY KEY IDENTITY,
    ProductId INT NOT NULL,
    FileName NVARCHAR(255) NOT NULL,
    OriginalName NVARCHAR(255) NOT NULL,
    FilePath NVARCHAR(500) NOT NULL,
    ThumbnailPath NVARCHAR(500) NOT NULL,
    FileSize BIGINT NOT NULL,
    MimeType NVARCHAR(100) NOT NULL,
    IsMain BIT NOT NULL DEFAULT 0,
    SortOrder INT NOT NULL DEFAULT 0,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    FOREIGN KEY (ProductId) REFERENCES Products(Id) ON DELETE CASCADE
);
```

### CategoryImages Table
```sql
CREATE TABLE CategoryImages (
    Id INT PRIMARY KEY IDENTITY,
    CategoryId INT NOT NULL,
    FileName NVARCHAR(255) NOT NULL,
    OriginalName NVARCHAR(255) NOT NULL,
    FilePath NVARCHAR(500) NOT NULL,
    ThumbnailPath NVARCHAR(500) NOT NULL,
    FileSize BIGINT NOT NULL,
    MimeType NVARCHAR(100) NOT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    FOREIGN KEY (CategoryId) REFERENCES Categories(Id) ON DELETE CASCADE
);
```

## 🔌 API Endpoints

### Upload
- `POST /api/v1/files/upload/product/{productId}` - Upload product images
- `POST /api/v1/files/upload/category/{categoryId}` - Upload category image

### Management
- `DELETE /api/v1/files/{fileId}` - Delete image
- `PUT /api/v1/files/{fileId}/main` - Set as main product image
- `PUT /api/v1/files/reorder` - Reorder product images
- `GET /api/v1/files/{entityType}/{entityId}` - Get entity images

### Static Files
- `GET /uploads/products/{productId}/{filename}` - Serve product images
- `GET /uploads/categories/{categoryId}/{filename}` - Serve category images

## 🖼️ Vue Component Usage

### Basic Usage
```vue
<PhotoUploader 
  v-model="images"
  entity-type="product"
  :entity-id="productId"
  :max-files="10"
  :show-reorder="true"
/>
```

### Category Usage
```vue
<PhotoUploader 
  v-model="categoryImage"
  entity-type="category"
  :entity-id="categoryId"
  :max-files="1"
/>
```

## ✨ Features

### Upload Features
- ✅ Drag & drop interface
- ✅ Click to browse files
- ✅ Multiple file selection (products)
- ✅ Progress indicators
- ✅ File type validation
- ✅ Size limit validation
- ✅ Image preview before upload

### Management Features
- ✅ Set main product image
- ✅ Reorder product images (drag & drop)
- ✅ Delete images with confirmation
- ✅ Auto-thumbnail generation
- ✅ Auto-resize large images
- ✅ File name collision handling (GUID-based names)

### Security Features
- ✅ File type validation (magic number checking)
- ✅ Size limit enforcement
- ✅ Secure file names (GUID + extension)
- ✅ Path traversal protection
- ✅ Authentication required for upload/delete

## 🔧 Technical Implementation

### Backend Services
- **FileService**: Handle upload, resize, thumbnail generation, deletion
- **ImageProcessingService**: Resize and optimize images
- **FileValidationService**: Validate file types and sizes

### Frontend Components
- **PhotoUploader.vue**: Main reusable component
- **ImagePreview.vue**: Individual image display with controls
- **UploadProgress.vue**: Progress indicator during upload

### Dependencies
- **Backend**: System.Drawing.Common or ImageSharp for image processing
- **Frontend**: Native HTML5 File API, Vue 3 Composition API

## 📁 File Naming Convention
- **Format**: `{GUID}.{extension}`
- **Example**: `a1b2c3d4-e5f6-7890-abcd-ef1234567890.jpg`
- **Thumbnail**: `thumb_{GUID}.{extension}`

## 🚀 Future Enhancements
- Cloud storage migration (Azure Blob/AWS S3)
- Image filters and editing
- Bulk upload operations
- Image compression options
- WebP conversion for better performance
- CDN integration

---
*Created: February 2026 | Last Updated: February 2026*