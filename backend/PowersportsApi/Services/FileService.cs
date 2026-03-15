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

    // ===== Media Library Methods =====

    /// <summary>
    /// Upload a file to the media library
    /// </summary>
    public async Task<AuthServiceResult<MediaFile>> UploadToMediaLibraryAsync(IFormFile file, int userId, string? altText = null, string? caption = null, string? tags = null, int? sectionId = null)
    {
        try
        {
            // Validate file
            if (file == null || file.Length == 0)
            {
                return AuthServiceResult<MediaFile>.Failure("No file uploaded");
            }

            if (file.Length > _maxFileSize)
            {
                return AuthServiceResult<MediaFile>.Failure($"File size exceeds maximum limit of {_maxFileSize / 1024 / 1024}MB");
            }

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!_allowedExtensions.Contains(extension))
            {
                return AuthServiceResult<MediaFile>.Failure("File type not allowed");
            }

            if (!_allowedMimeTypes.Contains(file.ContentType))
            {
                return AuthServiceResult<MediaFile>.Failure("File MIME type not allowed");
            }

            // Get section name for folder organization
            string sectionFolder = "General"; // Default folder for files without a section
            
            if (sectionId.HasValue)
            {
                var section = await _context.MediaSections.FindAsync(sectionId.Value);
                if (section != null)
                {
                    // Create safe folder name from section name
                    sectionFolder = string.Concat(section.Name.Where(c => char.IsLetterOrDigit(c) || c == '-' || c == '_' || c == ' '))
                        .Trim()
                        .Replace(" ", "_");
                }
            }

            // Generate storage path organized by section
            var uploadDate = DateTime.UtcNow;
            var uploadsFolder = GetSecureUploadPath("media", sectionFolder);
            
            if (uploadsFolder == null)
            {
                return AuthServiceResult<MediaFile>.Failure("Failed to create upload directory");
            }

            Directory.CreateDirectory(uploadsFolder);

            // Generate unique filename
            var originalFileName = Path.GetFileNameWithoutExtension(file.FileName);
            var safeFileName = string.Concat(originalFileName.Where(c => char.IsLetterOrDigit(c) || c == '-' || c == '_'));
            var storedFileName = $"{safeFileName}_{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(uploadsFolder, storedFileName);
            var relativeFilePath = $"/uploads/media/{sectionFolder}/{storedFileName}";

            int? width = null;
            int? height = null;
            string? thumbnailPath = null;

            // Process image if it's an image file
            if (file.ContentType.StartsWith("image/"))
            {
                using (var image = await Image.LoadAsync(file.OpenReadStream()))
                {
                    width = image.Width;
                    height = image.Height;

                    // Resize if too large
                    if (image.Width > _maxImageWidth)
                    {
                        image.Mutate(x => x.Resize(_maxImageWidth, 0));
                    }

                    // Save main image
                    var encoder = GetImageEncoder(extension);
                    await image.SaveAsync(filePath, encoder);

                    // Generate thumbnail
                    var thumbnailFileName = $"{Path.GetFileNameWithoutExtension(storedFileName)}_thumb{extension}";
                    var thumbnailFilePath = Path.Combine(uploadsFolder, thumbnailFileName);
                    thumbnailPath = $"/uploads/media/{sectionFolder}/{thumbnailFileName}";

                    using (var thumbnail = await Image.LoadAsync(file.OpenReadStream()))
                    {
                        thumbnail.Mutate(x => x.Resize(_thumbnailSize, 0));
                        await thumbnail.SaveAsync(thumbnailFilePath, encoder);
                    }
                }
            }
            else
            {
                // For non-image files, just save them
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }
            }

            // Determine media type
            var mediaType = MediaType.Other;
            if (file.ContentType.StartsWith("image/")) mediaType = MediaType.Image;
            else if (file.ContentType.StartsWith("video/")) mediaType = MediaType.Video;
            else if (file.ContentType.StartsWith("application/pdf") || file.ContentType.Contains("document")) mediaType = MediaType.Document;

            // Create database record
            var mediaFile = new MediaFile
            {
                FileName = file.FileName,
                StoredFileName = storedFileName,
                FilePath = relativeFilePath,
                ThumbnailPath = thumbnailPath,
                MimeType = file.ContentType,
                FileSize = file.Length,
                MediaType = mediaType,
                Width = width,
                Height = height,
                AltText = altText,
                Caption = caption,
                Tags = tags,
                SectionId = sectionId,
                UploadedByUserId = userId,
                UploadedAt = uploadDate
            };

            _context.MediaFiles.Add(mediaFile);
            await _context.SaveChangesAsync();

            _logger.LogInformation("File uploaded to media library: {FileName} by user {UserId}", file.FileName, userId);

            return AuthServiceResult<MediaFile>.Success(mediaFile);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading file to media library");
            return AuthServiceResult<MediaFile>.Failure("An error occurred while uploading the file");
        }
    }

    /// <summary>
    /// Get media files with optional filtering
    /// </summary>
    public async Task<(List<MediaFile> Files, int TotalCount)> GetMediaFilesAsync(
        string? search = null,
        MediaType? mediaType = null,
        int? sectionId = null,
        int page = 1,
        int pageSize = 50)
    {
        var query = _context.MediaFiles.AsQueryable();

        // Apply filters
        if (!string.IsNullOrEmpty(search))
        {
            search = search.ToLower();
            query = query.Where(m => 
                m.FileName.ToLower().Contains(search) ||
                (m.AltText != null && m.AltText.ToLower().Contains(search)) ||
                (m.Caption != null && m.Caption.ToLower().Contains(search)) ||
                (m.Tags != null && m.Tags.ToLower().Contains(search)));
        }

        if (mediaType.HasValue)
        {
            query = query.Where(m => m.MediaType == mediaType.Value);
        }

        if (sectionId.HasValue)
        {
            query = query.Where(m => m.SectionId == sectionId.Value);
        }

        var totalCount = await query.CountAsync();

        var files = await query
            .OrderByDescending(m => m.UploadedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Include(m => m.UploadedByUser)
            .ToListAsync();

        return (files, totalCount);
    }

    /// <summary>
    /// Get a single media file by ID
    /// </summary>
    public async Task<MediaFile?> GetMediaFileByIdAsync(int id)
    {
        return await _context.MediaFiles
            .Include(m => m.UploadedByUser)
            .FirstOrDefaultAsync(m => m.Id == id);
    }

    /// <summary>
    /// Update media file metadata
    /// </summary>
    public async Task<AuthServiceResult<MediaFile>> UpdateMediaFileAsync(int id, string? altText, string? caption, string? tags)
    {
        try
        {
            var mediaFile = await _context.MediaFiles.FindAsync(id);
            if (mediaFile == null)
            {
                return AuthServiceResult<MediaFile>.Failure("Media file not found");
            }

            mediaFile.AltText = altText;
            mediaFile.Caption = caption;
            mediaFile.Tags = tags;
            mediaFile.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return AuthServiceResult<MediaFile>.Success(mediaFile);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating media file {Id}", id);
            return AuthServiceResult<MediaFile>.Failure("An error occurred while updating the media file");
        }
    }

    /// <summary>
    /// Delete a media file
    /// </summary>
    public async Task<AuthServiceResult<object>> DeleteMediaFileAsync(int id)
    {
        try
        {
            var mediaFile = await _context.MediaFiles.FindAsync(id);
            if (mediaFile == null)
            {
                return AuthServiceResult<object>.Failure("Media file not found");
            }

            // Delete physical files
            var basePath = _environment.WebRootPath;
            var mainFilePath = Path.Combine(basePath, mediaFile.FilePath.TrimStart('/'));
            
            if (File.Exists(mainFilePath))
            {
                File.Delete(mainFilePath);
            }

            if (!string.IsNullOrEmpty(mediaFile.ThumbnailPath))
            {
                var thumbnailPath = Path.Combine(basePath, mediaFile.ThumbnailPath.TrimStart('/'));
                if (File.Exists(thumbnailPath))
                {
                    File.Delete(thumbnailPath);
                }
            }

            // Delete database record
            _context.MediaFiles.Remove(mediaFile);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Media file deleted: {Id}", id);

            return AuthServiceResult<object>.Success(new { });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting media file {Id}", id);
            return AuthServiceResult<object>.Failure("An error occurred while deleting the media file");
        }
    }

    /// <summary>
    /// Delete media section associated with an entity and all its files
    /// </summary>
    /// <param name="entityId">The ID of the entity (Category, Product, etc.)</param>
    /// <param name="entityType">Type of entity: "Category", "Product", "TeamMember", etc.</param>
    /// <returns>Number of files deleted</returns>
    public async Task<(bool Success, int DeletedFileCount, string? ErrorMessage)> DeleteEntityMediaSectionAsync(int entityId, string entityType)
    {
        try
        {
            // Find the media section by entity type
            MediaSection? mediaSection = null;
            
            switch (entityType.ToLower())
            {
                case "category":
                    mediaSection = await _context.MediaSections
                        .Include(s => s.MediaFiles)
                        .FirstOrDefaultAsync(s => s.CategoryId == entityId);
                    break;
                // Add more cases as needed for future entities
                default:
                    return (false, 0, $"Unsupported entity type: {entityType}");
            }
            
            if (mediaSection == null)
            {
                // No section found - not an error, just nothing to delete
                return (true, 0, null);
            }

            var basePath = _environment.WebRootPath;
            var deletedFileCount = 0;
            
            // Delete all media files in the section
            foreach (var mediaFile in mediaSection.MediaFiles.ToList())
            {
                // Delete physical main file
                var mainFilePath = Path.Combine(basePath, mediaFile.FilePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
                if (File.Exists(mainFilePath))
                {
                    File.Delete(mainFilePath);
                    _logger.LogInformation("Deleted physical file: {FilePath}", mainFilePath);
                }

                // Delete physical thumbnail
                if (!string.IsNullOrEmpty(mediaFile.ThumbnailPath))
                {
                    var thumbnailPath = Path.Combine(basePath, mediaFile.ThumbnailPath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
                    if (File.Exists(thumbnailPath))
                    {
                        File.Delete(thumbnailPath);
                        _logger.LogInformation("Deleted thumbnail: {ThumbnailPath}", thumbnailPath);
                    }
                }

                // Delete database record
                _context.MediaFiles.Remove(mediaFile);
                deletedFileCount++;
            }

            // Delete the section folder
            var sectionFolderName = string.Concat(mediaSection.Name.Where(c => char.IsLetterOrDigit(c) || c == '-' || c == '_' || c == ' '))
                .Trim()
                .Replace(" ", "_");
            var sectionFolderPath = Path.Combine(basePath, "uploads", "media", sectionFolderName);
            
            if (Directory.Exists(sectionFolderPath))
            {
                try
                {
                    Directory.Delete(sectionFolderPath, recursive: true);
                    _logger.LogInformation("Deleted section folder: {FolderPath}", sectionFolderPath);
                }
                catch (Exception folderEx)
                {
                    _logger.LogWarning(folderEx, "Could not delete section folder: {FolderPath}", sectionFolderPath);
                }
            }

            // Delete the media section
            _context.MediaSections.Remove(mediaSection);
            await _context.SaveChangesAsync();
            
            _logger.LogInformation("Deleted media section {SectionId} for {EntityType} {EntityId} with {FileCount} files", 
                mediaSection.Id, entityType, entityId, deletedFileCount);

            return (true, deletedFileCount, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting media section for {EntityType} {EntityId}", entityType, entityId);
            return (false, 0, "An error occurred while deleting the media section");
        }
    }
}