namespace mperformancepower.Api.DTOs.Vehicle;

public class VehicleImageDto
{
    public int Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public bool IsPrimary { get; set; }
    public int DisplayOrder { get; set; }
}
