using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats.Webp;
using mperformancepower.Api.Services.Interfaces;

namespace mperformancepower.Api.Services;

public class ImageService(IWebHostEnvironment env) : IImageService
{
    private const int MaxWidth = 1200;

    public async Task<string> SaveImageAsync(IFormFile file, string folderSlug)
    {
        var uploadDir = Path.Combine(env.WebRootPath, "uploads", folderSlug);
        Directory.CreateDirectory(uploadDir);

        var fileName = $"{folderSlug}/{Guid.NewGuid()}.webp";
        var fullPath = Path.Combine(env.WebRootPath, "uploads", folderSlug, Path.GetFileName(fileName));

        using var image = await Image.LoadAsync(file.OpenReadStream());

        if (image.Width > MaxWidth)
            image.Mutate(x => x.Resize(MaxWidth, 0));

        await image.SaveAsWebpAsync(fullPath, new WebpEncoder { Quality = 82 });

        return fileName;
    }

    public void DeleteImage(string fileName)
    {
        var uploadsRoot = Path.GetFullPath(Path.Combine(env.WebRootPath, "uploads"));
        var fullPath = Path.GetFullPath(Path.Combine(uploadsRoot, fileName));
        if (!fullPath.StartsWith(uploadsRoot + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
            return;
        if (File.Exists(fullPath))
            File.Delete(fullPath);
    }

    public void DeleteFolder(string folder)
    {
        var uploadsRoot = Path.GetFullPath(Path.Combine(env.WebRootPath, "uploads"));
        var fullPath = Path.GetFullPath(Path.Combine(uploadsRoot, folder));
        if (!fullPath.StartsWith(uploadsRoot + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
            return;
        if (Directory.Exists(fullPath))
            Directory.Delete(fullPath, recursive: true);
    }
}
