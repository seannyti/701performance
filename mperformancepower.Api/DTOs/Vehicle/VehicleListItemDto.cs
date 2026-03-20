namespace mperformancepower.Api.DTOs.Vehicle;

public class VehicleListItemDto
{
    public int Id { get; set; }
    public string Make { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public int Year { get; set; }
    public int CategoryId { get; set; }
    public string Category { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int? Mileage { get; set; }
    public string Condition { get; set; } = string.Empty;
    public int Stock { get; set; }
    public bool Featured { get; set; }
    public string? PrimaryImage { get; set; }
}
