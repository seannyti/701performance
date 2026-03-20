namespace mperformancepower.Api.DTOs.Order;

public class OrderDto
{
    public int Id { get; set; }
    public int VehicleId { get; set; }
    public string VehicleName { get; set; } = string.Empty;
    public int? InquiryId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public decimal SalePrice { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public decimal? DownPayment { get; set; }
    public decimal? LoanAmount { get; set; }
    public int? LoanTermMonths { get; set; }
    public decimal? APR { get; set; }
    public string? LenderName { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public string? TrackingNumber { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? DeliveredAt { get; set; }
}
