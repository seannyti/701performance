using System.ComponentModel.DataAnnotations;

namespace mperformancepower.Api.DTOs.Appointment;

public class CreateAppointmentDto
{
    [Required, MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required, MaxLength(200)]
    public string CustomerName { get; set; } = string.Empty;

    [MaxLength(200)]
    public string CustomerEmail { get; set; } = string.Empty;

    [MaxLength(50)]
    public string CustomerPhone { get; set; } = string.Empty;

    public int? UserId { get; set; }
    public int? VehicleId { get; set; }

    [Required]
    public DateTime StartTime { get; set; }

    [Required]
    public DateTime EndTime { get; set; }

    [MaxLength(2000)]
    public string? Notes { get; set; }
}
