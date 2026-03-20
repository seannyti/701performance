namespace mperformancepower.Api.Models;

public class SiteSetting
{
    public string Section { get; set; } = string.Empty;
    public string DataJson { get; set; } = "{}";
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
