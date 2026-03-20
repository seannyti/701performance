namespace mperformancepower.Api.Models;

public class Inquiry
{
    public int Id { get; set; }
    public int? VehicleId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string Status { get; set; } = "New";
    public DateTime? RespondedAt { get; set; }

    public Vehicle? Vehicle { get; set; }
}
