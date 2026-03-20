namespace mperformancepower.Api.Services.Interfaces;

public interface IImageService
{
    Task<string> SaveImageAsync(IFormFile file, string folderSlug);
    void DeleteImage(string fileName);
    void DeleteFolder(string folder);
}
