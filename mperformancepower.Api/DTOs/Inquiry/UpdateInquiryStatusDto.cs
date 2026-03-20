using System.ComponentModel.DataAnnotations;

namespace mperformancepower.Api.DTOs.Inquiry;

public class UpdateInquiryStatusDto
{
    [Required]
    public string Status { get; set; } = string.Empty;
}
