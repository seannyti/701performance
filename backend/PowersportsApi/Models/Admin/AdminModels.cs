using System.ComponentModel.DataAnnotations;

namespace PowersportsApi.Models.Admin;

/// <summary>
/// Request model for creating a new product
/// </summary>
public class CreateProductRequest
{
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(1000)]
    public string Description { get; set; } = string.Empty;
    
    [Required]
    [Range(0.01, 999999.99)]
    public decimal Price { get; set; }
    
    [Required]
    public int CategoryId { get; set; }
    
    [MaxLength(500)]
    public string? ImageUrl { get; set; }
    
    public bool IsFeatured { get; set; } = false;
    
    public bool IsActive { get; set; } = true;

    [MaxLength(100)]
    public string? Sku { get; set; }

    [Range(0, int.MaxValue)]
    public int StockQuantity { get; set; } = 0;

    [Range(0, int.MaxValue)]
    public int LowStockThreshold { get; set; } = 5;

    [Range(0, 999999.99)]
    public decimal? CostPrice { get; set; }

    public string? Specifications { get; set; }
}

/// <summary>
/// Request model for updating an existing product
/// </summary>
public class UpdateProductRequest
{
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(1000)]
    public string Description { get; set; } = string.Empty;
    
    [Required]
    [Range(0.01, 999999.99)]
    public decimal Price { get; set; }
    
    [Required]
    public int CategoryId { get; set; }
    
    [MaxLength(500)]
    public string? ImageUrl { get; set; }
    
    public bool IsFeatured { get; set; }
    
    public bool IsActive { get; set; }

    [MaxLength(100)]
    public string? Sku { get; set; }

    [Range(0, int.MaxValue)]
    public int StockQuantity { get; set; } = 0;

    [Range(0, int.MaxValue)]
    public int LowStockThreshold { get; set; } = 5;

    [Range(0, 999999.99)]
    public decimal? CostPrice { get; set; }

    public string? Specifications { get; set; }
}

/// <summary>
/// Request for adjusting stock quantity via the inventory tab
/// </summary>
public class AdjustStockRequest
{
    [Required]
    [Range(0, int.MaxValue)]
    public int StockQuantity { get; set; }

    [Range(0, int.MaxValue)]
    public int LowStockThreshold { get; set; } = 5;

    [MaxLength(100)]
    public string? Sku { get; set; }

    [Range(0, 999999.99)]
    public decimal? CostPrice { get; set; }
}

/// <summary>
/// Request model for creating a new category
/// </summary>
public class CreateCategoryRequest
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;
    
    [MaxLength(500)]
    public string? ImageUrl { get; set; }
    
    public bool IsActive { get; set; } = true;
}

/// <summary>
/// Request model for updating an existing category
/// </summary>
public class UpdateCategoryRequest
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;
    
    [MaxLength(500)]
    public string? ImageUrl { get; set; }
    
    public bool IsActive { get; set; }
}

/// <summary>
/// Request model for updating site settings
/// </summary>
public class UpdateSiteSettingRequest
{
    [Required]
    public string Value { get; set; } = string.Empty;
}

/// <summary>
/// Request model for creating a new site setting
/// </summary>
public class CreateSiteSettingRequest
{
    [Required]
    [MaxLength(100)]
    public string Key { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(200)]
    public string DisplayName { get; set; } = string.Empty;
    
    [Required]
    public string Value { get; set; } = string.Empty;
    
    [MaxLength(500)]
    public string? Description { get; set; }
    
    public SettingType Type { get; set; } = SettingType.Text;
    
    [MaxLength(50)]
    public string Category { get; set; } = "General";
    
    public int SortOrder { get; set; } = 0;
    
    public bool IsRequired { get; set; } = false;
    
    public bool IsActive { get; set; } = true;
}

/// <summary>
/// Request model for updating user roles (SuperAdmin only)
/// </summary>
public class UpdateUserRoleRequest
{
    [Required]
    public UserRole Role { get; set; }
}

/// <summary>
/// Request model for creating a user from the admin panel (SuperAdmin only)
/// </summary>
public class CreateAdminUserRequest
{
    [Required]
    [MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    [Required]
    [MaxLength(255)]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MinLength(8)]
    public string Password { get; set; } = string.Empty;

    [MaxLength(20)]
    public string? Phone { get; set; }

    public UserRole Role { get; set; } = UserRole.User;
}

/// <summary>
/// Request model for admin updating a user's profile information (Admin+ only)
/// </summary>
public class UpdateAdminUserInfoRequest
{
    [Required]
    [MaxLength(50)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string LastName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [MaxLength(100)]
    public string Email { get; set; } = string.Empty;

    [MaxLength(20)]
    public string? Phone { get; set; }

    public bool SubscribeNewsletter { get; set; } = false;
    
    public bool IsEmailVerified { get; set; } = false;
}

/// <summary>
/// Request model for admin resetting a user's password (SuperAdmin only)
/// </summary>
public class ResetUserPasswordRequest
{
    [Required]
    [MinLength(8)]
    public string NewPassword { get; set; } = string.Empty;
}

/// <summary>
/// Admin response with operation result
/// </summary>
public class AdminOperationResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public object? Data { get; set; }

    public static AdminOperationResult SuccessResult(string message, object? data = null)
    {
        return new AdminOperationResult { Success = true, Message = message, Data = data };
    }
    
    public static AdminOperationResult ErrorResult(string message)
    {
        return new AdminOperationResult { Success = false, Message = message };
    }
}

/// <summary>
/// Request model for updating a contact submission
/// </summary>
public class UpdateContactSubmissionRequest
{
    [Required]
    public ContactStatus Status { get; set; }
    
    public string? AdminNotes { get; set; }
    
    public int? AssignedToUserId { get; set; }
}

/// <summary>
/// Request model for creating a manual order (admin)
/// </summary>
public class CreateOrderRequest
{
    public int? UserId { get; set; }
    
    [Required]
    [MaxLength(200)]
    public string CustomerName { get; set; } = string.Empty;
    
    [Required]
    [EmailAddress]
    [MaxLength(255)]
    public string CustomerEmail { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(20)]
    public string CustomerPhone { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(500)]
    public string ShippingAddress { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(100)]
    public string ShippingCity { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(50)]
    public string ShippingState { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(20)]
    public string ShippingZipCode { get; set; } = string.Empty;
    
    [MaxLength(100)]
    public string? ShippingCountry { get; set; } = "USA";
    
    [Required]
    public List<CreateOrderItemRequest> Items { get; set; } = new();
    
    [Range(0, 999999.99)]
    public decimal TaxAmount { get; set; }
    
    [Range(0, 999999.99)]
    public decimal ShippingCost { get; set; }
    
    [Required]
    public PaymentMethod PaymentMethod { get; set; }
    
    public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;
    
    public string? PaymentNotes { get; set; }
    
    public string? CustomerNotes { get; set; }
    
    public string? AdminNotes { get; set; }
}

/// <summary>
/// Request model for order item within CreateOrderRequest
/// </summary>
public class CreateOrderItemRequest
{
    [Required]
    public int ProductId { get; set; }
    
    [Required]
    [Range(1, int.MaxValue)]
    public int Quantity { get; set; }
    
    [Range(0.01, 999999.99)]
    public decimal? UnitPriceOverride { get; set; } // Allow admin to override price
}

/// <summary>
/// Request model for updating an existing order
/// </summary>
public class UpdateOrderRequest
{
    [MaxLength(200)]
    public string? CustomerName { get; set; }
    
    [EmailAddress]
    [MaxLength(255)]
    public string? CustomerEmail { get; set; }
    
    [MaxLength(20)]
    public string? CustomerPhone { get; set; }
    
    [MaxLength(500)]
    public string? ShippingAddress { get; set; }
    
    [MaxLength(100)]
    public string? ShippingCity { get; set; }
    
    [MaxLength(50)]
    public string? ShippingState { get; set; }
    
    [MaxLength(20)]
    public string? ShippingZipCode { get; set; }
    
    [MaxLength(100)]
    public string? ShippingCountry { get; set; }
    
    public OrderStatus? OrderStatus { get; set; }
    
    public PaymentStatus? PaymentStatus { get; set; }
    
    public PaymentMethod? PaymentMethod { get; set; }
    
    public DateTime? PaymentReceivedDate { get; set; }
    
    public string? PaymentNotes { get; set; }
    
    [MaxLength(200)]
    public string? TrackingNumber { get; set; }
    
    [MaxLength(100)]
    public string? ShippingCarrier { get; set; }
    
    public DateTime? ShippedDate { get; set; }
    
    public DateTime? DeliveredDate { get; set; }
    
    public string? CustomerNotes { get; set; }
    
    public string? AdminNotes { get; set; }
}

/// <summary>
/// Request model for updating media file metadata
/// </summary>
public class UpdateMediaFileRequest
{
    [MaxLength(500)]
    public string? AltText { get; set; }
    
    [MaxLength(1000)]
    public string? Caption { get; set; }
    
    [MaxLength(500)]
    public string? Tags { get; set; }
}
