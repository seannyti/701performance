using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using mperformancepower.Api.Data;
using mperformancepower.Api.DTOs.Common;
using mperformancepower.Api.DTOs.Vehicle;
using mperformancepower.Api.Models;
using mperformancepower.Api.Enums;
using mperformancepower.Api.Services.Interfaces;

namespace mperformancepower.Api.Services;

public class VehicleService(AppDbContext db, IImageService imageService) : IVehicleService
{
    public async Task<PagedResultDto<VehicleListItemDto>> GetVehiclesAsync(
        int page, int pageSize, int? categoryId,
        VehicleCondition? condition, bool? featured, string? search)
    {
        var query = db.Vehicles.Include(v => v.Images).Include(v => v.Category).AsQueryable();

        query = query.Where(v => v.Category.IsActive);

        if (categoryId.HasValue)
            query = query.Where(v => v.CategoryId == categoryId.Value);
        if (condition.HasValue)
            query = query.Where(v => v.Condition == condition.Value);
        if (featured.HasValue)
            query = query.Where(v => v.Featured == featured.Value);
        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(v => v.Make.Contains(search) || v.Model.Contains(search));

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(v => v.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(v => new VehicleListItemDto
            {
                Id = v.Id,
                Make = v.Make,
                Model = v.Model,
                Year = v.Year,
                CategoryId = v.CategoryId,
                Category = v.Category.Name,
                Price = v.Price,
                Mileage = v.Mileage,
                Condition = v.Condition.ToString(),
                Stock = v.Stock,
                Featured = v.Featured,
                PrimaryImage = v.Images
                    .Where(i => i.IsPrimary)
                    .OrderBy(i => i.DisplayOrder)
                    .Select(i => i.FileName)
                    .FirstOrDefault()
                    ?? v.Images.OrderBy(i => i.DisplayOrder).Select(i => i.FileName).FirstOrDefault()
            })
            .ToListAsync();

        return new PagedResultDto<VehicleListItemDto>
        {
            Items = items,
            TotalCount = total,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<VehicleDto?> GetVehicleAsync(int id)
    {
        var v = await db.Vehicles
            .Include(v => v.Images.OrderBy(i => i.DisplayOrder))
            .Include(v => v.Category)
            .FirstOrDefaultAsync(v => v.Id == id);

        return v is null ? null : MapToDto(v);
    }

    public async Task<VehicleDto> CreateVehicleAsync(CreateVehicleDto dto)
    {
        var vehicle = new Vehicle
        {
            Make = dto.Make,
            Model = dto.Model,
            Year = dto.Year,
            CategoryId = dto.CategoryId,
            Price = dto.Price,
            Mileage = dto.Mileage,
            Condition = dto.Condition,
            Description = dto.Description,
            Stock = dto.Stock,
            Featured = dto.Featured,
            Specs = JsonSerializer.Serialize(dto.Specs ?? []),
        };

        db.Vehicles.Add(vehicle);
        await db.SaveChangesAsync();
        await db.Entry(vehicle).Reference(v => v.Category).LoadAsync();
        return MapToDto(vehicle);
    }

    public async Task<VehicleDto?> UpdateVehicleAsync(int id, UpdateVehicleDto dto)
    {
        var vehicle = await db.Vehicles.Include(v => v.Images).Include(v => v.Category).FirstOrDefaultAsync(v => v.Id == id);
        if (vehicle is null) return null;

        vehicle.Make = dto.Make;
        vehicle.Model = dto.Model;
        vehicle.Year = dto.Year;
        vehicle.CategoryId = dto.CategoryId;
        vehicle.Price = dto.Price;
        vehicle.Mileage = dto.Mileage;
        vehicle.Condition = dto.Condition;
        vehicle.Description = dto.Description;
        vehicle.Stock = dto.Stock;
        vehicle.Featured = dto.Featured;
        vehicle.Specs = JsonSerializer.Serialize(dto.Specs ?? []);

        await db.SaveChangesAsync();
        await db.Entry(vehicle).Reference(v => v.Category).LoadAsync();
        return MapToDto(vehicle);
    }

    public async Task<bool> DeleteVehicleAsync(int id)
    {
        var vehicle = await db.Vehicles.Include(v => v.Images).FirstOrDefaultAsync(v => v.Id == id);
        if (vehicle is null) return false;

        // Delete all image files and then the folder
        var folders = vehicle.Images
            .Select(img => Path.GetDirectoryName(img.FileName)?.Replace('\\', '/'))
            .Where(f => !string.IsNullOrWhiteSpace(f))
            .Distinct()
            .ToList();

        foreach (var img in vehicle.Images)
            imageService.DeleteImage(img.FileName);

        foreach (var folder in folders)
            imageService.DeleteFolder(folder!);

        db.Vehicles.Remove(vehicle);
        await db.SaveChangesAsync();
        return true;
    }

    public async Task<List<VehicleImageDto>> UploadImagesAsync(int vehicleId, IFormFileCollection files)
    {
        var vehicle = await db.Vehicles.Include(v => v.Images).FirstOrDefaultAsync(v => v.Id == vehicleId);
        if (vehicle is null) throw new KeyNotFoundException($"Vehicle {vehicleId} not found.");

        var currentMax = vehicle.Images.Any() ? vehicle.Images.Max(i => i.DisplayOrder) : -1;
        var noPrimary = !vehicle.Images.Any(i => i.IsPrimary);
        var result = new List<VehicleImageDto>();

        var slug = System.Text.RegularExpressions.Regex.Replace(
            $"{vehicle.Year}-{vehicle.Make}-{vehicle.Model}".ToLowerInvariant().Replace(' ', '-'),
            @"[^a-z0-9\-]", "");

        foreach (var file in files)
        {
            var fileName = await imageService.SaveImageAsync(file, slug);
            currentMax++;
            var img = new VehicleImage
            {
                VehicleId = vehicleId,
                FileName = fileName,
                DisplayOrder = currentMax,
                IsPrimary = noPrimary && currentMax == 0
            };
            db.VehicleImages.Add(img);
            await db.SaveChangesAsync();
            result.Add(new VehicleImageDto
            {
                Id = img.Id,
                FileName = img.FileName,
                IsPrimary = img.IsPrimary,
                DisplayOrder = img.DisplayOrder
            });
        }

        return result;
    }

    public async Task<bool> DeleteImageAsync(int imageId)
    {
        var img = await db.VehicleImages.FindAsync(imageId);
        if (img is null) return false;

        imageService.DeleteImage(img.FileName);
        db.VehicleImages.Remove(img);
        await db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> SetPrimaryImageAsync(int imageId)
    {
        var img = await db.VehicleImages.FindAsync(imageId);
        if (img is null) return false;

        var siblings = await db.VehicleImages
            .Where(i => i.VehicleId == img.VehicleId)
            .ToListAsync();

        foreach (var sibling in siblings)
            sibling.IsPrimary = sibling.Id == imageId;

        await db.SaveChangesAsync();
        return true;
    }

    public async Task ReorderImagesAsync(List<(int Id, int Order)> reorders)
    {
        foreach (var (id, order) in reorders)
        {
            var img = await db.VehicleImages.FindAsync(id);
            if (img is not null)
                img.DisplayOrder = order;
        }
        await db.SaveChangesAsync();
    }

    private static VehicleDto MapToDto(Vehicle v) => new()
    {
        Id = v.Id,
        Make = v.Make,
        Model = v.Model,
        Year = v.Year,
        CategoryId = v.CategoryId,
        Category = v.Category?.Name ?? string.Empty,
        Price = v.Price,
        Mileage = v.Mileage,
        Condition = v.Condition.ToString(),
        Description = v.Description,
        Stock = v.Stock,
        Featured = v.Featured,
        CreatedAt = v.CreatedAt,
        Specs = string.IsNullOrWhiteSpace(v.Specs) ? [] :
            JsonSerializer.Deserialize<List<VehicleSpecDto>>(v.Specs,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? [],
        Images = v.Images.Select(i => new VehicleImageDto
        {
            Id = i.Id,
            FileName = i.FileName,
            IsPrimary = i.IsPrimary,
            DisplayOrder = i.DisplayOrder
        }).ToList()
    };
}
