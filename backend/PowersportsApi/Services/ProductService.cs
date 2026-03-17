using Microsoft.EntityFrameworkCore;
using PowersportsApi.Data;
using PowersportsApi.Models;

namespace PowersportsApi.Services;

/// <summary>
/// Service for managing product data with fallback support for in-memory and database storage.
/// </summary>
public class ProductService
{
    private readonly PowersportsDbContext _context;
    private readonly bool _useDatabase;
    private readonly ILogger<ProductService> _logger;

    // Fallback in-memory data if database is not available
    private static readonly List<Product> _fallbackProducts = new()
    {
        new Product 
        { 
            Id = 1, 
            Name = "Yamaha Grizzly 700", 
            Description = "Powerful 4WD ATV perfect for trail riding and utility work. Features a 686cc engine with reliable Ultramatic transmission.", 
            Price = 12999.99m, 
            CategoryId = 1, // ATV
            ImageUrl = "https://images.unsplash.com/photo-1558618047-6c0c841469ed?w=400&h=300&fit=crop"
        },
        new Product 
        { 
            Id = 2, 
            Name = "Honda CRF450R", 
            Description = "Championship-winning motocross bike with advanced suspension and lightweight aluminum frame. Built for competition.", 
            Price = 9899.99m, 
            CategoryId = 2, // Dirtbike
            ImageUrl = "https://images.unsplash.com/photo-1558577452-838b5e2b6b75?w=400&h=300&fit=crop"
        },
        new Product 
        { 
            Id = 3, 
            Name = "Polaris RZR XP 1000", 
            Description = "High-performance side-by-side UTV with sport suspension and powerful ProStar engine. Ready for extreme terrain.", 
            Price = 21999.99m, 
            CategoryId = 3, // UTV
            ImageUrl = "https://images.unsplash.com/photo-1571068316344-75bc76f77890?w=400&h=300&fit=crop"
        },
        new Product 
        { 
            Id = 4, 
            Name = "Ski-Doo MXZ X-RS", 
            Description = "Premium racing snowmobile with 850 E-TEC engine and advanced RAS X suspension for precise handling on trails.", 
            Price = 16499.99m, 
            CategoryId = 4, // Snowmobile
            ImageUrl = "https://images.unsplash.com/photo-1578662996442-48f60103fc96?w=400&h=300&fit=crop"
        },
        new Product 
        { 
            Id = 5, 
            Name = "Fox Racing V3 Helmet", 
            Description = "Professional-grade motocross helmet with MIPS technology and superior ventilation. DOT and ECE certified.", 
            Price = 649.99m, 
            CategoryId = 5, // Gear
            ImageUrl = "https://images.unsplash.com/photo-1558618542-b3d9f7c05de4?w=400&h=300&fit=crop"
        },
        new Product 
        { 
            Id = 6, 
            Name = "Can-Am Maverick X3", 
            Description = "Turbocharged side-by-side with industry-leading power and innovative Intelligent Throttle Control.", 
            Price = 24999.99m, 
            CategoryId = 3, // UTV
            ImageUrl = "https://images.unsplash.com/photo-1591737622611-b6c5ae78542d?w=400&h=300&fit=crop"
        },
        new Product 
        { 
            Id = 7, 
            Name = "KTM 250 SX-F", 
            Description = "Lightweight 4-stroke motocross bike with championship-proven performance and advanced WP XACT suspension.", 
            Price = 8999.99m, 
            CategoryId = 2, // Dirtbike
            ImageUrl = "https://images.unsplash.com/photo-1606768666853-403c90a981ad?w=400&h=300&fit=crop"
        },
        new Product 
        { 
            Id = 8, 
            Name = "Arctic Cat Alpha One", 
            Description = "Mountain snowmobile with revolutionary Alpha rear suspension and powerful 800 H.O. engine for deep snow performance.", 
            Price = 15299.99m, 
            CategoryId = 4, // Snowmobile
            ImageUrl = "https://images.unsplash.com/photo-1578662996442-48f60103fc96?w=400&h=300&fit=crop"
        },
        new Product 
        { 
            Id = 9, 
            Name = "Alpinestars Tech 10 Boots", 
            Description = "Premium motocross boots with advanced protection system and lightweight microfiber construction.", 
            Price = 599.99m, 
            CategoryId = 5, // Gear
            ImageUrl = "https://images.unsplash.com/photo-1544966503-7cc5ac882d5b?w=400&h=300&fit=crop"
        },
        new Product 
        { 
            Id = 10, 
            Name = "Honda Foreman 520", 
            Description = "Versatile utility ATV with electric power steering and independent rear suspension. Perfect for work and recreation.", 
            Price = 8999.99m, 
            CategoryId = 1, // ATV
            ImageUrl = "https://images.unsplash.com/photo-1558618047-6c0c841469ed?w=400&h=300&fit=crop"
        }
    };

    public ProductService(PowersportsDbContext context, IConfiguration configuration, ILogger<ProductService> logger)
    {
        _context = context;
        _useDatabase = configuration.GetValue<string>("DatabaseProvider") != "InMemory";
        _logger = logger;
    }

    /// <summary>
    /// Retrieves all products from the database, including their images. Falls back to in-memory data if database is unavailable.
    /// </summary>
    /// <returns>A list of all products with their main image URLs populated.</returns>
    public async Task<List<Product>> GetAllProductsAsync()
    {
        try
        {
            if (_useDatabase && await IsDatabaseAvailableAsync())
            {
                var products = await _context.Products
                    .Include(p => p.Category)
                    .Include(p => p.ProductImages)
                        .ThenInclude(pi => pi.MediaFile)
                    .ToListAsync();
                
                // Set ImageUrl from main product image if available
                foreach (var product in products)
                {
                    var mainImage = product.ProductImages?.FirstOrDefault(pi => pi.IsMain);
                    if (mainImage?.MediaFile != null)
                    {
                        product.ImageUrl = mainImage.MediaFile.ThumbnailPath ?? string.Empty;
                    }
                }
                
                return products;
            }
            else
            {
                return await Task.FromResult(_fallbackProducts);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Database unavailable or error in GetAllProductsAsync, returning fallback products");
            return _fallbackProducts;
        }
    }

    /// <summary>
    /// Retrieves a specific product by its ID, including associated images. Falls back to in-memory data if database is unavailable.
    /// </summary>
    /// <param name="id">The unique identifier of the product to retrieve.</param>
    /// <returns>The product with the specified ID, or null if not found.</returns>
    public async Task<Product?> GetProductByIdAsync(int id)
    {
        try
        {
            if (_useDatabase && await IsDatabaseAvailableAsync())
            {
                var product = await _context.Products
                    .Include(p => p.Category)
                    .Include(p => p.ProductImages)
                        .ThenInclude(pi => pi.MediaFile)
                    .FirstOrDefaultAsync(p => p.Id == id);
                
                // Set ImageUrl from main product image if available
                if (product != null)
                {
                    var mainImage = product.ProductImages?.FirstOrDefault(pi => pi.IsMain);
                    if (mainImage?.MediaFile != null)
                    {
                        product.ImageUrl = mainImage.MediaFile.ThumbnailPath ?? string.Empty;
                    }
                }
                
                return product;
            }
            else
            {
                var product = _fallbackProducts.FirstOrDefault(p => p.Id == id);
                return await Task.FromResult(product);
            }
        }
        catch (Exception)
        {
            // If database fails, fall back to in-memory data
            var product = _fallbackProducts.FirstOrDefault(p => p.Id == id);
            return await Task.FromResult(product);
        }
    }

    /// <summary>
    /// Retrieves all products belonging to a specific category. Falls back to in-memory data if database is unavailable.
    /// </summary>
    /// <param name="category">The name of the category (e.g., "ATV", "Dirtbike", "UTV").</param>
    /// <returns>A list of products in the specified category.</returns>
    public async Task<List<Product>> GetProductsByCategoryAsync(string category)
    {
        try
        {
            if (_useDatabase && await IsDatabaseAvailableAsync())
            {
                return await _context.Products
                    .Include(p => p.Category)
                    .Where(p => p.Category.Name.ToLower() == category.ToLower())
                    .ToListAsync();
            }
            else
            {
                // Map category names to IDs for in-memory lookup
                var categoryId = GetCategoryIdByName(category);
                var products = _fallbackProducts.Where(p => p.CategoryId == categoryId).ToList();
                return await Task.FromResult(products);
            }
        }
        catch (Exception)
        {
            // If database fails, fall back to in-memory data
            var categoryId = GetCategoryIdByName(category);
            var products = _fallbackProducts.Where(p => p.CategoryId == categoryId).ToList();
            return await Task.FromResult(products);
        }
    }

    /// <summary>
    /// Retrieves the first 3 products for display on the home page. Falls back to in-memory data if database is unavailable.
    /// </summary>
    /// <returns>A list of up to 3 featured products with their main image URLs populated.</returns>
    public async Task<List<Product>> GetFeaturedProductsAsync()
    {
        try
        {
            if (_useDatabase && await IsDatabaseAvailableAsync())
            {
                var products = await _context.Products
                    .Include(p => p.Category)
                    .Include(p => p.ProductImages)
                        .ThenInclude(pi => pi.MediaFile)
                    .Take(3)
                    .ToListAsync();
                
                // Set ImageUrl from main product image if available
                foreach (var product in products)
                {
                    var mainImage = product.ProductImages?.FirstOrDefault(pi => pi.IsMain);
                    if (mainImage?.MediaFile != null)
                    {
                        product.ImageUrl = mainImage.MediaFile.ThumbnailPath ?? string.Empty;
                    }
                }
                
                return products;
            }
            else
            {
                var featured = _fallbackProducts.Take(3).ToList();
                return await Task.FromResult(featured);
            }
        }
        catch (Exception)
        {
            // If database fails, fall back to in-memory data
            var featured = _fallbackProducts.Take(3).ToList();
            return await Task.FromResult(featured);
        }
    }

    /// <summary>
    /// Checks if the database connection is available and can be connected to.
    /// </summary>
    /// <returns>True if the database is available, false otherwise.</returns>
    private async Task<bool> IsDatabaseAvailableAsync()
    {
        try
        {
            return await _context.Database.CanConnectAsync();
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Maps category names to their corresponding IDs for in-memory data operations.
    /// </summary>
    /// <param name="categoryName">The name of the category (case-insensitive).</param>
    /// <returns>The category ID, or 0 if the category is not recognized.</returns>
    private static int GetCategoryIdByName(string categoryName)
    {
        return categoryName.ToUpper() switch
        {
            "ATV" => 1,
            "DIRTBIKE" => 2,
            "UTV" => 3,
            "SNOWMOBILE" => 4,
            "GEAR" => 5,
            _ => 0 // Unknown category
        };
    }
}