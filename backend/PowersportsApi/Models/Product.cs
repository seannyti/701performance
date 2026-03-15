using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PowersportsApi.Models;

/// <summary>
/// Represents a powersports product including ATVs, dirt bikes, UTVs, snowmobiles, and gear
/// </summary>
public class Product
{
    public int Id { get; set; }
    
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(1000)]
    public string Description { get; set; } = string.Empty;
    
    public decimal Price { get; set; }
    
    public int CategoryId { get; set; }
    
    [MaxLength(500)]
    public string ImageUrl { get; set; } = string.Empty; // Legacy - will be replaced by ProductImages
    
    public bool IsActive { get; set; } = true;
    
    /// <summary>
    /// Whether this product is featured on the homepage
    /// </summary>
    public bool IsFeatured { get; set; } = false;

    // --- Inventory fields ---

    /// <summary>Stock Keeping Unit — unique identifier for this product</summary>
    [MaxLength(100)]
    public string? Sku { get; set; }

    /// <summary>Current number of units on hand</summary>
    public int StockQuantity { get; set; } = 0;

    /// <summary>Warn when stock falls to or below this value</summary>
    public int LowStockThreshold { get; set; } = 5;

    /// <summary>Purchase / cost price (used to calculate margin)</summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal? CostPrice { get; set; }

    /// <summary>Product specifications stored as JSON (e.g., Engine, Power, Weight)</summary>
    [Column(TypeName = "nvarchar(max)")]
    public string? Specifications { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public Category Category { get; set; } = null!;
    public ICollection<ProductImage> ProductImages { get; set; } = new List<ProductImage>();
    
    // Computed property for main image
    public ProductImage? MainImage => ProductImages.FirstOrDefault(pi => pi.IsMain);
}