namespace mperformancepower.Api.Models;

public class HeroSettings
{
    public int Id { get; set; }
    public string HeroType { get; set; } = "none"; // "youtube" | "mp4" | "image" | "none"
    public string? YoutubeUrl { get; set; }
    public int YoutubeStartTime { get; set; } = 0;
    public string? VideoPath { get; set; }
    public string? ImagePath { get; set; }
    public double OverlayOpacity { get; set; } = 0.5;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
