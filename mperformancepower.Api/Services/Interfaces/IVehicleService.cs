using mperformancepower.Api.DTOs.Common;
using mperformancepower.Api.DTOs.Vehicle;
using mperformancepower.Api.Enums;

namespace mperformancepower.Api.Services.Interfaces;

public interface IVehicleService
{
    Task<PagedResultDto<VehicleListItemDto>> GetVehiclesAsync(
        int page, int pageSize, int? categoryId,
        VehicleCondition? condition, bool? featured, string? search);
    Task<VehicleDto?> GetVehicleAsync(int id);
    Task<VehicleDto> CreateVehicleAsync(CreateVehicleDto dto);
    Task<VehicleDto?> UpdateVehicleAsync(int id, UpdateVehicleDto dto);
    Task<bool> DeleteVehicleAsync(int id);
    Task<List<VehicleImageDto>> UploadImagesAsync(int vehicleId, IFormFileCollection files);
    Task<bool> DeleteImageAsync(int imageId);
    Task<bool> SetPrimaryImageAsync(int imageId);
    Task ReorderImagesAsync(List<(int Id, int Order)> reorders);
}
