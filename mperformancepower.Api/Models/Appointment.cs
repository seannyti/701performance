namespace mperformancepower.Api.Models;

public class Appointment
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;

    // Optional user link (for future customer profile association)
    public int? UserId { get; set; }
    public AppUser? User { get; set; }

    // Optional vehicle link
    public int? VehicleId { get; set; }
    public Vehicle? Vehicle { get; set; }

    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }

    // Scheduled, Completed, Cancelled, NoShow
    public string Status { get; set; } = "Scheduled";

    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
