using mperformancepower.Api.Enums;

namespace mperformancepower.Api.Models;

public class Vehicle
{
    public int Id { get; set; }
    public string Make { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public int Year { get; set; }
    public int CategoryId { get; set; }
    public Category Category { get; set; } = null!;
    public decimal Price { get; set; }
    public int? Mileage { get; set; }
    public VehicleCondition Condition { get; set; }
    public string Description { get; set; } = string.Empty;
    public int Stock { get; set; }
    public bool Featured { get; set; }
    public string Specs { get; set; } = "[]";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<VehicleImage> Images { get; set; } = [];
    public ICollection<Inquiry> Inquiries { get; set; } = [];
}
