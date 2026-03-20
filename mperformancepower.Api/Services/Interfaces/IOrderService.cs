using mperformancepower.Api.DTOs.Common;
using mperformancepower.Api.DTOs.Finance;
using mperformancepower.Api.DTOs.Order;

namespace mperformancepower.Api.Services.Interfaces;

public interface IOrderService
{
    Task<PagedResultDto<OrderListItemDto>> GetOrdersAsync(int page, int pageSize, string? status, string? search);
    Task<OrderDto?> GetOrderAsync(int id);
    Task<OrderDto> CreateOrderAsync(CreateOrderDto dto);
    Task<OrderDto?> UpdateOrderAsync(int id, UpdateOrderDto dto);
    Task<bool> DeleteOrderAsync(int id);
    Task<FinanceStatsDto> GetFinanceStatsAsync();
}
