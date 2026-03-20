namespace mperformancepower.Api.DTOs.Inquiry;

public class InquiryDto
{
    public int Id { get; set; }
    public int? VehicleId { get; set; }
    public string? VehicleName { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string Status { get; set; } = "New";
    public DateTime? RespondedAt { get; set; }
}
