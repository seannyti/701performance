namespace mperformancepower.Api.Models;

public class VehicleImage
{
    public int Id { get; set; }
    public int VehicleId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public bool IsPrimary { get; set; }
    public int DisplayOrder { get; set; }

    public Vehicle Vehicle { get; set; } = null!;
}
