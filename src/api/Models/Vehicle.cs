using System.Text.Json.Serialization;

namespace PerformancePower.Api.Models;

public class Vehicle
{
    public int Id { get; set; }
    public string StockNumber { get; set; } = string.Empty;
    public string? Vin { get; set; }
    public int Year { get; set; }
    public string Make { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public string? Trim { get; set; }
    public string Type { get; set; } = string.Empty; // atv|utv|motorcycle|dirtbike|snowmobile|other
    public string Condition { get; set; } = "new"; // new|used|consignment
    public string? Color { get; set; }
    public int? Mileage { get; set; }
    public decimal Cost { get; set; }
    public decimal Msrp { get; set; }
    public decimal SalePrice { get; set; }
    public string Status { get; set; } = "available"; // available|pending|sold|hold|trade
    public bool IsFeatured { get; set; } = false;
    public string? Description { get; set; }
    public string? SpecSheetUrl { get; set; }
    public string? Specs { get; set; } // JSON: [{ "label": "Engine", "value": "449cc 4-Stroke" }, ...]

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? SoldAt { get; set; }

    public ICollection<VehicleImage> Images { get; set; } = new List<VehicleImage>();
    public ICollection<VehicleHistory> History { get; set; } = new List<VehicleHistory>();
}

public class VehicleImage
{
    public int Id { get; set; }
    public int VehicleId { get; set; }
    public Vehicle Vehicle { get; set; } = null!;
    public string Url { get; set; } = string.Empty;
    public string ThumbnailUrl { get; set; } = string.Empty;
    public int SortOrder { get; set; } = 0;
    public bool IsPrimary { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class VehicleHistory
{
    public int Id { get; set; }
    public int VehicleId { get; set; }
    [JsonIgnore]
    public Vehicle Vehicle { get; set; } = null!;
    public int? UserId { get; set; }
    public User? User { get; set; }
    public string Action { get; set; } = string.Empty;
    public string? OldValues { get; set; } // JSON
    public string? NewValues { get; set; } // JSON
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
