namespace PowersportsApi.Models;

public record ProductImageDto(
    int Id,
    int ProductId,
    int? MediaFileId,
    bool IsMain,
    int SortOrder,
    string Url,
    string? ThumbnailUrl
);

public record ProductDto(
    int Id,
    string Name,
    string Description,
    decimal Price,
    string Category,
    int CategoryId,
    string? ImageUrl,
    bool IsFeatured,
    bool IsActive,
    string? Sku,
    int StockQuantity,
    int LowStockThreshold,
    decimal? CostPrice,
    string? Specifications,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    List<ProductImageDto> ProductImages
);

public static class ProductExtensions
{
    public static ProductDto ToProductDto(this Product p) => new(
        p.Id,
        p.Name,
        p.Description,
        p.Price,
        p.Category?.Name ?? "",
        p.CategoryId,
        p.ImageUrl,
        p.IsFeatured,
        p.IsActive,
        p.Sku,
        p.StockQuantity,
        p.LowStockThreshold,
        p.CostPrice,
        p.Specifications,
        p.CreatedAt,
        p.UpdatedAt,
        p.ProductImages?
            .OrderBy(pi => pi.SortOrder)
            .Select(pi => new ProductImageDto(
                pi.Id,
                pi.ProductId,
                pi.MediaFileId,
                pi.IsMain,
                pi.SortOrder,
                pi.MediaFile?.FilePath ?? "",
                pi.MediaFile?.ThumbnailPath
            )).ToList() ?? []
    );
}
