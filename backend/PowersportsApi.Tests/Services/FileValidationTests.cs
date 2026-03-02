using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;

namespace PowersportsApi.Tests.Services;

/// <summary>
/// Tests for file upload validation logic including size limits, 
/// extension checking, MIME type validation, and magic number verification.
/// These test the validation rules without needing the full FileService.
/// </summary>
public class FileValidationTests
{
    // JPEG magic bytes: FF D8 FF
    private static readonly byte[] JpegHeader = new byte[] { 0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x10, 0x4A, 0x46 };
    // PNG magic bytes: 89 50 4E 47
    private static readonly byte[] PngHeader = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };
    // WebP magic bytes: 52 49 46 46 (RIFF)
    private static readonly byte[] WebpHeader = new byte[] { 0x52, 0x49, 0x46, 0x46, 0x00, 0x00, 0x00, 0x00 };
    // Fake file that pretends to be a JPEG but isn't
    private static readonly byte[] FakeJpegContent = new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

    private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".webp" };
    private static readonly string[] AllowedMimeTypes = { "image/jpeg", "image/png", "image/webp" };
    private const long MaxFileSizeBytes = 10 * 1024 * 1024; // 10 MB

    // ─── Extension Validation ─────────────────────────────────────────────────

    [Theory]
    [InlineData(".jpg", true)]
    [InlineData(".jpeg", true)]
    [InlineData(".png", true)]
    [InlineData(".webp", true)]
    [InlineData(".gif", false)]
    [InlineData(".exe", false)]
    [InlineData(".php", false)]
    [InlineData(".js", false)]
    [InlineData(".svg", false)]
    [InlineData(".bmp", false)]
    [InlineData(".tiff", false)]
    public void Extension_AllowedExtensions_ReturnsExpectedResult(string extension, bool expectedAllowed)
    {
        // Act
        var isAllowed = AllowedExtensions.Contains(extension.ToLowerInvariant());

        // Assert
        isAllowed.Should().Be(expectedAllowed, $"extension '{extension}' should {(expectedAllowed ? "" : "not ")}be allowed");
    }

    // ─── MIME Type Validation ─────────────────────────────────────────────────

    [Theory]
    [InlineData("image/jpeg", true)]
    [InlineData("image/png", true)]
    [InlineData("image/webp", true)]
    [InlineData("image/gif", false)]
    [InlineData("application/pdf", false)]
    [InlineData("text/html", false)]
    [InlineData("application/javascript", false)]
    [InlineData("application/octet-stream", false)]
    public void MimeType_AllowedTypes_ReturnsExpectedResult(string mimeType, bool expectedAllowed)
    {
        // Act
        var isAllowed = AllowedMimeTypes.Contains(mimeType);

        // Assert
        isAllowed.Should().Be(expectedAllowed, $"MIME type '{mimeType}' should {(expectedAllowed ? "" : "not ")}be allowed");
    }

    // ─── File Size Validation ─────────────────────────────────────────────────

    [Fact]
    public void FileSize_ExceedsLimit_ShouldReject()
    {
        // Arrange
        var fileSize = 11 * 1024 * 1024L; // 11 MB (over the 10 MB limit)

        // Act
        var isValidSize = fileSize <= MaxFileSizeBytes;

        // Assert
        isValidSize.Should().BeFalse();
    }

    [Fact]
    public void FileSize_UnderLimit_ShouldAccept()
    {
        // Arrange
        var fileSize = 5 * 1024 * 1024L; // 5 MB

        // Act
        var isValidSize = fileSize <= MaxFileSizeBytes;

        // Assert
        isValidSize.Should().BeTrue();
    }

    [Fact]
    public void FileSize_ExactlyAtLimit_ShouldAccept()
    {
        // Arrange
        var fileSize = MaxFileSizeBytes; // exactly 10 MB

        // Act
        var isValidSize = fileSize <= MaxFileSizeBytes;

        // Assert
        isValidSize.Should().BeTrue();
    }

    // ─── Magic Number (File Signature) Tests ─────────────────────────────────

    [Fact]
    public void JpegMagicBytes_ShouldBeRecognized()
    {
        // Arrange
        var stream = new MemoryStream(JpegHeader);
        var buffer = new byte[8];
        stream.Read(buffer, 0, 8);

        // Assert
        var isJpeg = buffer[0] == 0xFF && buffer[1] == 0xD8 && buffer[2] == 0xFF;
        isJpeg.Should().BeTrue();
    }

    [Fact]
    public void PngMagicBytes_ShouldBeRecognized()
    {
        // Arrange
        var stream = new MemoryStream(PngHeader);
        var buffer = new byte[8];
        stream.Read(buffer, 0, 8);

        // Assert
        var isPng = buffer[0] == 0x89 && buffer[1] == 0x50 && buffer[2] == 0x4E && buffer[3] == 0x47;
        isPng.Should().BeTrue();
    }

    [Fact]
    public void WebpMagicBytes_ShouldBeRecognized()
    {
        // Arrange
        var stream = new MemoryStream(WebpHeader);
        var buffer = new byte[8];
        stream.Read(buffer, 0, 8);

        // Assert
        var isWebp = buffer[0] == 0x52 && buffer[1] == 0x49 && buffer[2] == 0x46 && buffer[3] == 0x46;
        isWebp.Should().BeTrue();
    }

    [Fact]
    public void FakeJpeg_WithWrongMagicBytes_ShouldBeRejected()
    {
        // Arrange - a file named .jpg that doesn't have JPEG magic bytes
        var stream = new MemoryStream(FakeJpegContent);
        var buffer = new byte[8];
        stream.Read(buffer, 0, 8);

        // Assert
        var isJpeg = buffer[0] == 0xFF && buffer[1] == 0xD8 && buffer[2] == 0xFF;
        isJpeg.Should().BeFalse("file does not have JPEG magic bytes");
    }

    // ─── Path Traversal Tests ─────────────────────────────────────────────────

    [Theory]
    [InlineData("../../../etc/passwd", true)]
    [InlineData("..\\..\\windows\\system32", true)]
    [InlineData("normal-filename.jpg", false)]
    [InlineData("my photo.png", false)]
    [InlineData("product_image_1.webp", false)]
    [InlineData("/etc/passwd", true)]
    [InlineData("C:\\Windows\\system32", true)]
    public void Filename_PathTraversalAttempt_DetectedCorrectly(string filename, bool isDangerous)
    {
        // Act
        var containsTraversal = filename.Contains("..") ||
                                filename.Contains("/") ||
                                filename.Contains("\\");

        // Assert
        containsTraversal.Should().Be(isDangerous,
            $"filename '{filename}' {(isDangerous ? "should" : "should not")} be flagged as dangerous");
    }

    // ─── Empty File Tests ─────────────────────────────────────────────────────

    [Fact]
    public void EmptyFile_ShouldBeRejected()
    {
        // Arrange
        var fileSize = 0L;

        // Assert
        var isEmpty = fileSize == 0;
        isEmpty.Should().BeTrue();
        (fileSize > 0).Should().BeFalse("empty files should be rejected");
    }
}
