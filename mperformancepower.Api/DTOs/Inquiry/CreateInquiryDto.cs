using System.ComponentModel.DataAnnotations;

namespace mperformancepower.Api.DTOs.Inquiry;

public class CreateInquiryDto
{
    public int? VehicleId { get; set; }

    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required, EmailAddress, MaxLength(200)]
    public string Email { get; set; } = string.Empty;

    [MaxLength(30)]
    public string Phone { get; set; } = string.Empty;

    [Required, MaxLength(2000)]
    public string Message { get; set; } = string.Empty;
}
