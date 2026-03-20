using System.ComponentModel.DataAnnotations;

namespace mperformancepower.Api.DTOs.Order;

public class UpdateOrderDto
{
    [Required, MaxLength(100)]
    public string CustomerName { get; set; } = string.Empty;

    [Required, EmailAddress, MaxLength(200)]
    public string CustomerEmail { get; set; } = string.Empty;

    [MaxLength(30)]
    public string CustomerPhone { get; set; } = string.Empty;

    [Range(0, 9999999)]
    public decimal SalePrice { get; set; }

    [Required]
    public string PaymentMethod { get; set; } = "Cash";

    public decimal? DownPayment { get; set; }
    public decimal? LoanAmount { get; set; }
    public int? LoanTermMonths { get; set; }
    public decimal? APR { get; set; }

    [MaxLength(200)]
    public string? LenderName { get; set; }

    [Required]
    public string Status { get; set; } = "Pending";

    [MaxLength(2000)]
    public string? Notes { get; set; }

    [MaxLength(200)]
    public string? TrackingNumber { get; set; }
}
