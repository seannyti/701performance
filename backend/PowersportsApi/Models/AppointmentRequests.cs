using System;
using System.ComponentModel.DataAnnotations;

namespace PowersportsApi.Models;

public class CreateAppointmentRequest
{
    [Required]
    public DateTime StartTime { get; set; }

    [Required]
    public DateTime EndTime { get; set; }

    [Required]
    [MaxLength(200)]
    public string CustomerName { get; set; } = string.Empty;

    [MaxLength(100)]
    [EmailAddress]
    public string? CustomerEmail { get; set; }

    [MaxLength(20)]
    public string? CustomerPhone { get; set; }

    [MaxLength(500)]
    public string? ServiceType { get; set; }

    [MaxLength(1000)]
    public string? Notes { get; set; }

    public int? UserId { get; set; }
}

public class UpdateAppointmentRequest
{
    [Required]
    public DateTime StartTime { get; set; }

    [Required]
    public DateTime EndTime { get; set; }

    [Required]
    [MaxLength(200)]
    public string CustomerName { get; set; } = string.Empty;

    [MaxLength(100)]
    [EmailAddress]
    public string? CustomerEmail { get; set; }

    [MaxLength(20)]
    public string? CustomerPhone { get; set; }

    [MaxLength(500)]
    public string? ServiceType { get; set; }

    [MaxLength(1000)]
    public string? Notes { get; set; }

    [Required]
    [MaxLength(50)]
    public string Status { get; set; } = "Scheduled";

    public int? UserId { get; set; }
}

public class UpdateAppointmentStatusRequest
{
    [Required]
    [MaxLength(50)]
    public string Status { get; set; } = string.Empty;
}
