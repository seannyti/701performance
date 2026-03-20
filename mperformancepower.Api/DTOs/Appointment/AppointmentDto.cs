namespace mperformancepower.Api.DTOs.Appointment;

public class AppointmentDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public int? UserId { get; set; }
    public string? UserName { get; set; }
    public int? VehicleId { get; set; }
    public string? VehicleName { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string Status { get; set; } = "Scheduled";
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
}
