using Microsoft.EntityFrameworkCore;
using PowersportsApi.Data;
using PowersportsApi.Models;
using PowersportsApi.Models.Auth;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats.Webp;
using System.Security.Claims;

namespace PowersportsApi.Services;

/// <summary>
/// Service for handling file uploads, image processing, and file management.
/// Supports product and category image uploads with automatic resizing and thumbnail generation.
/// </summary>
public class FileService
{
    private readonly PowersportsDbContext _context;
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<FileService> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IConfiguration _configuration;
    
    private readonly string[] _allowedExtensions;
    private readonly string[] _allowedMimeTypes;
    private readonly long _maxFileSize;
    private readonly int _maxImageWidth;
    private readonly int _thumbnailSize;
    private readonly int _imageQuality;

    public FileService(PowersportsDbContext context, IWebHostEnvironment environment, ILogger<FileService> logger, IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
    {
        _context = context;
        _environment = environment;
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
        _configuration = configuration;
        
        var fileSettings = configuration.GetSection("FileUploadSettings");
        _maxFileSize = fileSettings.GetValue<int>("MaxFileSizeMB") * 1024 * 1024;
        _maxImageWidth = fileSettings.GetValue<int>("MaxImageWidth");
        _thumbnailSize = fileSettings.GetValue<int>("ThumbnailSize");
        _imageQuality = fileSettings.GetValue<int>("ImageQuality");
        _allowedExtensions = fileSettings.GetSection("AllowedExtensions").Get<string[]>() ?? new[] { ".jpg", ".jpeg", ".png", ".webp" };
        _allowedMimeTypes = fileSettings.GetSection("AllowedMimeTypes").Get<string[]>() ?? new[] { "image/jpeg", "image/png", "image/webp" };
    }

    /// <summary>
    /// Uploads an image for a product with automatic resizing and thumbnail generation.
    /// </summary>
    /// <param name="productId">The ID of the product.</param>
    /// <param name="file">The image file to upload.</param>
    /// <param name="userId">The ID of the user performing the upload.</param>
    /// <returns>The created product image record if successful.</returns>
    public async Task<AuthServiceResult<ProductImage>> UploadProductImageAsync(int productId, IFormFile file, int userId)
    {
        try
        {
            // Validate product exists
            var product = await _context.Products.FindAsync(productId);
            if (product == null)
            {
                return AuthServiceResult<ProductImage>.Failure("Product not found.");
            }

            // Check product image count (max 10)
            var existingCount = await _context.ProductImages.CountAsync(pi => pi.ProductId == productId);
            if (existingCount >= 10)
            {
                return AuthServiceResult<ProductImage>.Failure("Maximum of 10 images allowed per product.");
            }

            // Validate file
            var validationResult = ValidateImageFile(file);
            if (!validationResult.IsSuccess)
            {
                return AuthServiceResult<ProductImage>.Failure(validationResult.ErrorMessage!);
            }

            // Create upload directory with secure path handling
            var uploadsDir = GetSecureUploadPath("products", productId.ToString());
            if (uploadsDir == null)
            {
                return AuthServiceResult<ProductImage>.Failure("Invalid upload path.");
            }
            Directory.CreateDirectory(uploadsDir);

            // Generate unique filename
            var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
            var fileName = $"{Guid.NewGuid()}{fileExtension}";
            var thumbnailFileName = $"thumb_{fileName}";

            var filePath = Path.Combine(uploadsDir, fileName);
            var thumbnailPath = Path.Combine(uploadsDir, thumbnailFileName);

            // Process and save image
            using (var stream = file.OpenReadStream())
            {
                await ProcessAndSaveImageAsync(stream, filePath, thumbnailPath);
            }

            // Determine if this should be the main image (if no main image exists)
            var isMain = !await _context.ProductImages.AnyAsync(pi => pi.ProductId == productId && pi.IsMain);
            
            // Get next sort order (simpler query for EF Core)
            var maxSortOrder = await _context.ProductImages
                .Where(pi => pi.ProductId == productId)
                .MaxAsync(pi => (int?)pi.SortOrder);
            var sortOrder = (maxSortOrder ?? 0) + 1;

            // Create database record
            var productImage = new ProductImage
            {
                ProductId = productId,
                FileName = fileName,
                OriginalName = file.FileName,
                FilePath = $"/uploads/products/{productId}/{fileName}",
                ThumbnailPath = $"/uploads/products/{productId}/{thumbnailFileName}",
                FileSize = file.Length,
                MimeType = file.ContentType,
                IsMain = isMain,
                SortOrder = sortOrder,
                CreatedAt = DateTime.UtcNow
            };

            _context.ProductImages.Add(productImage);
            
            // If this is the first/main image, update the Product.ImageUrl
            if (isMain)
            {
                product.ImageUrl = $"/uploads/products/{productId}/thumb_{fileName}";
                product.UpdatedAt = DateTime.UtcNow;
            }
            
            await _context.SaveChangesAsync();

            _logger.LogInformation("Product image uploaded: {FileName} for Product {ProductId} by User {UserId}", 
                fileName, productId, userId);

            return AuthServiceResult<ProductImage>.Success(productImage);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading product image for Product {ProductId}: {Message}", productId, ex.Message);
            return AuthServiceResult<ProductImage>.Failure($"An error occurred while uploading the image: {ex.Message}");
        }
    }

    /// <summary>
    /// Uploads and processes an image for a category. Only one image is allowed per category.
    /// </summary>
    /// <param name="categoryId">The ID of the category to upload the image for.</param>
    /// <param name="file">The image file to upload.</param>
    /// <param name="userId">The ID of the user performing the upload.</param>
    /// <returns>A result containing the created CategoryImage on success, or an error message on failure.</returns>
    public async Task<AuthServiceResult<CategoryImage>> UploadCategoryImageAsync(int categoryId, IFormFile file, int userId)
    {
        try
        {
            // Validate category exists
            var category = await _context.Categories.FindAsync(categoryId);
            if (category == null)
            {
                return AuthServiceResult<CategoryImage>.Failure("Category not found.");
            }

            // Check if category already has an image (only one allowed)
            var existingImage = await _context.CategoryImages.FirstOrDefaultAsync(ci => ci.CategoryId == categoryId);
            if (existingImage != null)
            {
                // Delete existing image first
                var deleteResult = await DeleteCategoryImageAsync(existingImage.Id, userId);
                if (!deleteResult.IsSuccess)
                {
                    return AuthServiceResult<CategoryImage>.Failure("Failed to replace existing category image.");
                }
            }

            // Validate file
            var validationResult = ValidateImageFile(file);
            if (!validationResult.IsSuccess)
            {
                return AuthServiceResult<CategoryImage>.Failure(validationResult.ErrorMessage!);
            }

            // Create upload directory with secure path handling
            var uploadsDir = GetSecureUploadPath("categories", categoryId.ToString());
            if (uploadsDir == null)
            {
                return AuthServiceResult<CategoryImage>.Failure("Invalid upload path.");
            }
            Directory.CreateDirectory(uploadsDir);

            // Generate unique filename
            var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
            var fileName = $"{Guid.NewGuid()}{fileExtension}";
            var thumbnailFileName = $"thumb_{fileName}";

            var filePath = Path.Combine(uploadsDir, fileName);
            var thumbnailPath = Path.Combine(uploadsDir, thumbnailFileName);

            // Process and save image
            using (var stream = file.OpenReadStream())
            {
                await ProcessAndSaveImageAsync(stream, filePath, thumbnailPath);
            }

            // Create database record
            var categoryImage = new CategoryImage
            {
                CategoryId = categoryId,
                FileName = fileName,
                OriginalName = file.FileName,
                FilePath = $"/uploads/categories/{categoryId}/{fileName}",
                ThumbnailPath = $"/uploads/categories/{categoryId}/{thumbnailFileName}",
                FileSize = file.Length,
                MimeType = file.ContentType,
                CreatedAt = DateTime.UtcNow
            };

            _context.CategoryImages.Add(categoryImage);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Category image uploaded: {FileName} for Category {CategoryId} by User {UserId}", 
                fileName, categoryId, userId);

            return AuthServiceResult<CategoryImage>.Success(categoryImage);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading category image for Category {CategoryId}: {Message}", categoryId, ex.Message);
            return AuthServiceResult<CategoryImage>.Failure($"An error occurred while uploading the image: {ex.Message}");
        }
    }

    /// <summary>
    /// Deletes a product image from the database and file system. If the deleted image was the main image,
    /// automatically promotes another image to main or clears the product's ImageUrl if no images remain.
    /// </summary>
    /// <param name="imageId">The ID of the image to delete.</param>
    /// <param name="userId">The ID of the user performing the deletion.</param>
    /// <returns>A result indicating success or failure of the deletion operation.</returns>
    public async Task<AuthServiceResult<object>> DeleteProductImageAsync(int imageId, int userId)
    {
        try
        {
            var image = await _context.ProductImages.FindAsync(imageId);
            if (image == null)
            {
                return AuthServiceResult<object>.Failure("Image not found.");
            }

            // Delete physical files
            var fullPath = Path.Combine(_environment.WebRootPath, image.FilePath.TrimStart('/'));
            var thumbnailFullPath = Path.Combine(_environment.WebRootPath, image.ThumbnailPath.TrimStart('/'));

            if (File.Exists(fullPath))
                File.Delete(fullPath);
            
            if (File.Exists(thumbnailFullPath))
                File.Delete(thumbnailFullPath);

            // If this was the main image, set another image as main
            if (image.IsMain)
            {
                var nextMainImage = await _context.ProductImages
                    .Where(pi => pi.ProductId == image.ProductId && pi.Id != imageId)
                    .OrderBy(pi => pi.SortOrder)
                    .FirstOrDefaultAsync();

                if (nextMainImage != null)
                {
                    nextMainImage.IsMain = true;
                    
                    // Update Product.ImageUrl to the new main image
                    var product = await _context.Products.FindAsync(image.ProductId);
                    if (product != null)
                    {
                        product.ImageUrl = $"/uploads/products/{image.ProductId}/thumb_{nextMainImage.FileName}";
                        product.UpdatedAt = DateTime.UtcNow;
                    }
                }
                else
                {
                    // No more images, clear Product.ImageUrl
                    var product = await _context.Products.FindAsync(image.ProductId);
                    if (product != null)
                    {
                        product.ImageUrl = string.Empty;
                        product.UpdatedAt = DateTime.UtcNow;
                    }
                }
            }

            _context.ProductImages.Remove(image);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Product image deleted: {ImageId} by User {UserId}", imageId, userId);
            
            return AuthServiceResult<object>.Success(new { message = "Image deleted successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting product image {ImageId}", imageId);
            return AuthServiceResult<object>.Failure("An error occurred while deleting the image.");
        }
    }

    /// <summary>
    /// Deletes a category image from the database and file system.
    /// </summary>
    /// <param name="imageId">The ID of the image to delete.</param>
    /// <param name="userId">The ID of the user performing the deletion.</param>
    /// <returns>A result indicating success or failure of the deletion operation.</returns>
    public async Task<AuthServiceResult<object>> DeleteCategoryImageAsync(int imageId, int userId)
    {
        try
        {
            var image = await _context.CategoryImages.FindAsync(imageId);
            if (image == null)
            {
                return AuthServiceResult<object>.Failure("Image not found.");
            }

            // Delete physical files
            var fullPath = Path.Combine(_environment.WebRootPath, image.FilePath.TrimStart('/'));
            var thumbnailFullPath = Path.Combine(_environment.WebRootPath, image.ThumbnailPath.TrimStart('/'));

            if (File.Exists(fullPath))
                File.Delete(fullPath);
            
            if (File.Exists(thumbnailFullPath))
                File.Delete(thumbnailFullPath);

            _context.CategoryImages.Remove(image);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Category image deleted: {ImageId} by User {UserId}", imageId, userId);
            
            return AuthServiceResult<object>.Success(new { message = "Image deleted successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting category image {ImageId}", imageId);
            return AuthServiceResult<object>.Failure("An error occurred while deleting the image.");
        }
    }

    /// <summary>
    /// Sets a specific product image as the main/primary image. Unsets the previous main image
    /// and updates the product's ImageUrl to reflect the new main image.
    /// </summary>
    /// <param name="imageId">The ID of the image to set as main.</param>
    /// <param name="userId">The ID of the user performing the operation.</param>
    /// <returns>A result indicating success or failure of the operation.</returns>
    public async Task<AuthServiceResult<object>> SetMainProductImageAsync(int imageId, int userId)
    {
        try
        {
            var image = await _context.ProductImages.FindAsync(imageId);
            if (image == null)
            {
                return AuthServiceResult<object>.Failure("Image not found.");
            }

            // Unset current main image
            var currentMain = await _context.ProductImages
                .FirstOrDefaultAsync(pi => pi.ProductId == image.ProductId && pi.IsMain);
            
            if (currentMain != null)
            {
                currentMain.IsMain = false;
            }

            // Set new main image
            image.IsMain = true;
            
            // Update Product.ImageUrl to reflect the new main image
            var product = await _context.Products.FindAsync(image.ProductId);
            if (product != null)
            {
                product.ImageUrl = $"/uploads/products/{image.ProductId}/thumb_{image.FileName}";
                product.UpdatedAt = DateTime.UtcNow;
            }
            
            await _context.SaveChangesAsync();

            _logger.LogInformation("Main product image set: {ImageId} for Product {ProductId} by User {UserId}", 
                imageId, image.ProductId, userId);

            return AuthServiceResult<object>.Success(new { message = "Main image updated successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting main product image {ImageId}", imageId);
            return AuthServiceResult<object>.Failure("An error occurred while updating the main image.");
        }
    }

    private AuthServiceResult<object> ValidateImageFile(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return AuthServiceResult<object>.Failure("No file provided.");
        }

        if (file.Length > _maxFileSize)
        {
            return AuthServiceResult<object>.Failure($"File size exceeds {_maxFileSize / 1024 / 1024}MB limit.");
        }

        // Validate filename for path traversal attempts
        var fileName = file.FileName;
        if (string.IsNullOrWhiteSpace(fileName) || 
            fileName.Contains("..") || 
            fileName.Contains("/") || 
            fileName.Contains("\\") ||
            Path.GetInvalidFileNameChars().Any(c => fileName.Contains(c)))
        {
            return AuthServiceResult<object>.Failure("Invalid filename.");
        }

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!_allowedExtensions.Contains(extension))
        {
            return AuthServiceResult<object>.Failure("Only JPG, PNG, and WebP files are allowed.");
        }

        if (!_allowedMimeTypes.Contains(file.ContentType))
        {
            return AuthServiceResult<object>.Failure("Invalid file type.");
        }

        // Verify file content matches its extension (magic number validation)
        if (!VerifyFileSignature(file, extension))
        {
            return AuthServiceResult<object>.Failure("File content does not match its extension.");
        }

        return AuthServiceResult<object>.Success(new { });
    }

    /// <summary>
    /// Verifies file signature (magic numbers) to ensure the file content matches the declared extension.
    /// </summary>
    private bool VerifyFileSignature(IFormFile file, string extension)
    {
        try
        {
            using var stream = file.OpenReadStream();
            var buffer = new byte[8];
            var bytesRead = stream.Read(buffer, 0, buffer.Length);
            
            if (bytesRead < 2)
            {
                return false;
            }

            // Check magic numbers for common image formats
            return extension switch
            {
                ".jpg" or ".jpeg" => buffer[0] == 0xFF && buffer[1] == 0xD8 && buffer[2] == 0xFF,
                ".png" => buffer[0] == 0x89 && buffer[1] == 0x50 && buffer[2] == 0x4E && buffer[3] == 0x47,
                ".webp" => buffer[0] == 0x52 && buffer[1] == 0x49 && buffer[2] == 0x46 && buffer[3] == 0x46,
                _ => false
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying file signature");
            return false;
        }
    }

    private async Task ProcessAndSaveImageAsync(Stream inputStream, string fullImagePath, string thumbnailPath)
    {
        using var image = await Image.LoadAsync(inputStream);
        
        if (image.Width > _maxImageWidth)
        {
            var ratio = (double)_maxImageWidth / image.Width;
            var newHeight = (int)(image.Height * ratio);
            image.Mutate(x => x.Resize(_maxImageWidth, newHeight));
        }

        var encoder = GetImageEncoder(fullImagePath);
        await image.SaveAsync(fullImagePath, encoder);

        var thumbnailRatio = Math.Min((double)_thumbnailSize / image.Width, (double)_thumbnailSize / image.Height);
        var thumbnailWidth = (int)(image.Width * thumbnailRatio);
        var thumbnailHeight = (int)(image.Height * thumbnailRatio);
        
        using var thumbnail = image.Clone(ctx => ctx.Resize(thumbnailWidth, thumbnailHeight));
        await thumbnail.SaveAsync(thumbnailPath, encoder);
    }

    private IImageEncoder GetImageEncoder(string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        return extension switch
        {
            ".png" => new PngEncoder(),
            ".webp" => new WebpEncoder { Quality = _imageQuality },
            _ => new JpegEncoder { Quality = _imageQuality },
        };
    }

    /// <summary>
    /// Generic file upload method that routes to specific upload handlers based on entity type.
    /// </summary>
    /// <param name="file">The file to upload.</param>
    /// <param name="entityType">The type of entity ("product" or "category").</param>
    /// <param name="entityId">The ID of the entity to associate the file with.</param>
    /// <returns>A result containing upload details on success, or an error message on failure.</returns>
    public async Task<AuthServiceResult<object>> UploadFileAsync(IFormFile file, string entityType, int entityId)
    {
        var userId = GetCurrentUserId();
        
        return entityType.ToLower() switch
        {
            "product" => await UploadProductImageAsync(entityId, file, userId) is var result 
                ? (result.IsSuccess 
                    ? AuthServiceResult<object>.Success(new { 
                        fileName = result.Data?.FileName, 
                        fileSize = result.Data?.FileSize, 
                        uploadDate = result.Data?.CreatedAt 
                      }) 
                    : AuthServiceResult<object>.Failure(result.ErrorMessage ?? "Upload failed"))
                : AuthServiceResult<object>.Failure("Upload failed"),
                
            "category" => await UploadCategoryImageAsync(entityId, file, userId) is var catResult 
                ? (catResult.IsSuccess 
                    ? AuthServiceResult<object>.Success(new { 
                        fileName = catResult.Data?.FileName, 
                        fileSize = catResult.Data?.FileSize, 
                        uploadDate = catResult.Data?.CreatedAt 
                      }) 
                    : AuthServiceResult<object>.Failure(catResult.ErrorMessage ?? "Upload failed"))
                : AuthServiceResult<object>.Failure("Upload failed"),
                
            _ => AuthServiceResult<object>.Failure($"Unsupported entity type: {entityType}")
        };
    }

    /// <summary>
    /// Generic file deletion method that routes to specific delete handlers based on entity type.
    /// </summary>
    /// <param name="fileName">The name of the file to delete.</param>
    /// <param name="entityType">The type of entity ("product" or "category").</param>
    /// <param name="entityId">The ID of the entity the file is associated with.</param>
    /// <returns>A result indicating success or failure of the deletion operation.</returns>
    public async Task<AuthServiceResult<object>> DeleteFileAsync(string fileName, string entityType, int entityId)
    {
        var userId = GetCurrentUserId();
        
        try
        {
            if (entityType.ToLower() == "product")
            {
                var image = await _context.ProductImages
                    .FirstOrDefaultAsync(pi => pi.ProductId == entityId && pi.FileName == fileName);
                if (image != null)
                {
                    return await DeleteProductImageAsync(image.Id, userId);
                }
            }
            else if (entityType.ToLower() == "category")
            {
                var image = await _context.CategoryImages
                    .FirstOrDefaultAsync(ci => ci.CategoryId == entityId && ci.FileName == fileName);
                if (image != null)
                {
                    return await DeleteCategoryImageAsync(image.Id, userId);
                }
            }
            
            return AuthServiceResult<object>.Failure("Image not found");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting file {FileName} for {EntityType} {EntityId}", fileName, entityType, entityId);
            return AuthServiceResult<object>.Failure($"Error deleting file: {ex.Message}");
        }
    }

    /// <summary>
    /// Retrieves all files associated with a specific entity.
    /// </summary>
    /// <param name="entityType">The type of entity ("product" or "category").</param>
    /// <param name="entityId">The ID of the entity to retrieve files for.</param>
    /// <returns>A list of file metadata objects including fileName, fileSize, uploadDate, and isDefault.</returns>
    public async Task<List<dynamic>> GetEntityFilesAsync(string entityType, int entityId)
    {
        try
        {
            if (entityType.ToLower() == "product")
            {
                var images = await _context.ProductImages
                    .Where(pi => pi.ProductId == entityId)
                    .OrderByDescending(pi => pi.IsMain)
                    .ThenBy(pi => pi.CreatedAt)
                    .ToListAsync();
                    
                return images.Select(i => (dynamic)new {
                    fileName = i.FileName,
                    fileSize = i.FileSize,
                    uploadDate = i.CreatedAt,
                    isDefault = i.IsMain
                }).ToList();
            }
            else if (entityType.ToLower() == "category")
            {
                var image = await _context.CategoryImages
                    .Where(ci => ci.CategoryId == entityId)
                    .FirstOrDefaultAsync();
                    
                if (image != null)
                {
                    return new List<dynamic> { new {
                        fileName = image.FileName,
                        fileSize = image.FileSize,
                        uploadDate = image.CreatedAt,
                        isDefault = true
                    }};
                }
            }
            
            return new List<dynamic>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting files for {EntityType} {EntityId}", entityType, entityId);
            return new List<dynamic>();
        }
    }

    /// <summary>
    /// Sets a specific image as the default/main image for an entity. Only applies to products.
    /// </summary>
    /// <param name="fileName">The name of the file to set as default.</param>
    /// <param name="entityType">The type of entity ("product" or "category").</param>
    /// <param name="entityId">The ID of the entity the file is associated with.</param>
    /// <returns>A result indicating success or failure of the operation.</returns>
    public async Task<AuthServiceResult<object>> SetDefaultImageAsync(string fileName, string entityType, int entityId)
    {
        var userId = GetCurrentUserId();
        
        try
        {
            if (entityType.ToLower() == "product")
            {
                var image = await _context.ProductImages
                    .FirstOrDefaultAsync(pi => pi.ProductId == entityId && pi.FileName == fileName);
                if (image != null)
                {
                    return await SetMainProductImageAsync(image.Id, userId);
                }
            }
            // Categories only have one image, so setting default doesn't apply
            
            return AuthServiceResult<object>.Failure("Image not found");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting default image {FileName} for {EntityType} {EntityId}", fileName, entityType, entityId);
            return AuthServiceResult<object>.Failure($"Error setting default image: {ex.Message}");
        }
    }

    /// <summary>
    /// Constructs a secure upload path by validating and sanitizing path components.
    /// Prevents path traversal attacks by ensuring all paths stay within the uploads directory.
    /// </summary>
    private string? GetSecureUploadPath(string entityType, string entityId)
    {
        try
        {
            // Validate inputs don't contain path traversal attempts
            if (string.IsNullOrWhiteSpace(entityType) || 
                string.IsNullOrWhiteSpace(entityId) ||
                entityType.Contains("..", StringComparison.Ordinal) || 
                entityType.Contains("/", StringComparison.Ordinal) || 
                entityType.Contains("\\", StringComparison.Ordinal) ||
                entityId.Contains("..", StringComparison.Ordinal) || 
                entityId.Contains("/", StringComparison.Ordinal) || 
                entityId.Contains("\\", StringComparison.Ordinal))
            {
                _logger.LogWarning("Invalid path components detected: EntityType={EntityType}, EntityId={EntityId}", 
                    entityType, entityId);
                return null;
            }

            // Build path using safe components
            var basePath = _environment.WebRootPath;
            var uploadsPath = Path.Combine(basePath, "uploads", entityType, entityId);
            
            // Get the full path and verify it's still within the expected directory
            var fullPath = Path.GetFullPath(uploadsPath);
            var expectedBasePath = Path.GetFullPath(Path.Combine(basePath, "uploads"));
            
            if (!fullPath.StartsWith(expectedBasePath, StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning("Path traversal attempt detected: {FullPath} is outside {BasePath}", 
                    fullPath, expectedBasePath);
                return null;
            }

            return fullPath;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error constructing secure upload path");
            return null;
        }
    }

    private int GetCurrentUserId()
    {
        var userIdClaim = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(userIdClaim, out var userId) ? userId : 1;
    }
}