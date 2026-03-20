namespace mperformancepower.Api.Models;

public class Order
{
    public int Id { get; set; }

    // Vehicle
    public int VehicleId { get; set; }
    public Vehicle Vehicle { get; set; } = null!;

    // Linked inquiry (optional)
    public int? InquiryId { get; set; }
    public Inquiry? Inquiry { get; set; }

    // Customer info
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;

    // Sale details
    public decimal SalePrice { get; set; }
    public string PaymentMethod { get; set; } = "Cash"; // Cash, Financed, TradeIn

    // Finance details (if Financed)
    public decimal? DownPayment { get; set; }
    public decimal? LoanAmount { get; set; }
    public int? LoanTermMonths { get; set; }
    public decimal? APR { get; set; }
    public string? LenderName { get; set; }

    // Status
    public string Status { get; set; } = "Pending"; // Pending, Completed, Delivered, Cancelled

    public string? Notes { get; set; }
    public string? TrackingNumber { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? DeliveredAt { get; set; }
}
