using System.ComponentModel.DataAnnotations;
using mperformancepower.Api.Enums;

namespace mperformancepower.Api.DTOs.Vehicle;

public class UpdateVehicleDto
{
    [Required, MaxLength(100)]
    public string Make { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string Model { get; set; } = string.Empty;

    [Range(1900, 2100)]
    public int Year { get; set; }

    [Range(1, int.MaxValue)]
    public int CategoryId { get; set; }

    [Range(0, 9999999)]
    public decimal Price { get; set; }

    public int? Mileage { get; set; }
    public VehicleCondition Condition { get; set; }
    [MaxLength(5000)]
    public string Description { get; set; } = string.Empty;

    [Range(0, 9999)]
    public int Stock { get; set; }

    public bool Featured { get; set; }
    public List<VehicleSpecDto>? Specs { get; set; }
}
