namespace PowersportsApi.Models;

public class Order
{
    public int Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty; // e.g., "ORD-20260302-0001"
    
    // Customer Information
    public int? UserId { get; set; }
    public User? User { get; set; }
    public required string CustomerName { get; set; }
    public required string CustomerEmail { get; set; }
    public required string CustomerPhone { get; set; }
    
    // Shipping Address
    public required string ShippingAddress { get; set; }
    public required string ShippingCity { get; set; }
    public required string ShippingState { get; set; }
    public required string ShippingZipCode { get; set; }
    public string? ShippingCountry { get; set; } = "USA";
    
    // Order Details
    public List<OrderItem> Items { get; set; } = new();
    public decimal Subtotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal ShippingCost { get; set; }
    public decimal TotalAmount { get; set; }
    
    // Payment Information
    public PaymentMethod PaymentMethod { get; set; }
    public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;
    public DateTime? PaymentReceivedDate { get; set; }
    public string? PaymentNotes { get; set; }
    
    // Fulfillment Information
    public OrderStatus OrderStatus { get; set; } = OrderStatus.Pending;
    public string? TrackingNumber { get; set; }
    public string? ShippingCarrier { get; set; }
    public DateTime? ShippedDate { get; set; }
    public DateTime? DeliveredDate { get; set; }
    
    // Additional Information
    public string? CustomerNotes { get; set; }
    public string? AdminNotes { get; set; }
    
    // Timestamps
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

public class OrderItem
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public Order Order { get; set; } = null!;
    
    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;
    
    public required string ProductName { get; set; } // Snapshot at time of order
    public string? ProductSku { get; set; }
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public decimal TotalPrice { get; set; }
}

public enum OrderStatus
{
    Pending,
    Processing,
    Shipped,
    Delivered,
    Cancelled,
    Refunded
}

public enum PaymentStatus
{
    Pending,
    Received,
    PartiallyPaid,
    Failed,
    Refunded
}

public enum PaymentMethod
{
    Cash,
    Check,
    BankTransfer,
    CreditCardPhone,
    Financing,
    Other,
    CreditCard,
    DebitCard
}
