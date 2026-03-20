using Microsoft.EntityFrameworkCore;
using mperformancepower.Api.Data;
using mperformancepower.Api.DTOs.Common;
using mperformancepower.Api.DTOs.Finance;
using mperformancepower.Api.DTOs.Order;
using mperformancepower.Api.Models;
using mperformancepower.Api.Services.Interfaces;

namespace mperformancepower.Api.Services;

public class OrderService(AppDbContext db) : IOrderService
{
    public async Task<PagedResultDto<OrderListItemDto>> GetOrdersAsync(
        int page, int pageSize, string? status, string? search)
    {
        var query = db.Orders.Include(o => o.Vehicle).ThenInclude(v => v.Category).AsQueryable();

        if (!string.IsNullOrWhiteSpace(status) && status != "All")
            query = query.Where(o => o.Status == status);

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(o =>
                o.CustomerName.Contains(search) ||
                o.CustomerEmail.Contains(search) ||
                o.Vehicle.Make.Contains(search) ||
                o.Vehicle.Model.Contains(search));

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(o => o.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(o => new OrderListItemDto
            {
                Id = o.Id,
                VehicleId = o.VehicleId,
                VehicleName = $"{o.Vehicle.Year} {o.Vehicle.Make} {o.Vehicle.Model}",
                CustomerName = o.CustomerName,
                CustomerEmail = o.CustomerEmail,
                SalePrice = o.SalePrice,
                PaymentMethod = o.PaymentMethod,
                Status = o.Status,
                CreatedAt = o.CreatedAt
            })
            .ToListAsync();

        return new PagedResultDto<OrderListItemDto>
        {
            Items = items,
            TotalCount = total,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<OrderDto?> GetOrderAsync(int id)
    {
        var o = await db.Orders
            .Include(o => o.Vehicle).ThenInclude(v => v.Category)
            .FirstOrDefaultAsync(o => o.Id == id);
        return o is null ? null : MapToDto(o);
    }

    public async Task<OrderDto> CreateOrderAsync(CreateOrderDto dto)
    {
        var order = new Order
        {
            VehicleId = dto.VehicleId,
            InquiryId = dto.InquiryId,
            CustomerName = dto.CustomerName,
            CustomerEmail = dto.CustomerEmail,
            CustomerPhone = dto.CustomerPhone,
            SalePrice = dto.SalePrice,
            PaymentMethod = dto.PaymentMethod,
            DownPayment = dto.DownPayment,
            LoanAmount = dto.LoanAmount,
            LoanTermMonths = dto.LoanTermMonths,
            APR = dto.APR,
            LenderName = dto.LenderName,
            Status = dto.Status,
            Notes = dto.Notes
        };

        db.Orders.Add(order);

        if (dto.Status == "Completed")
            await DecrementStock(dto.VehicleId);

        await db.SaveChangesAsync();
        await db.Entry(order).Reference(o => o.Vehicle).LoadAsync();
        await db.Entry(order.Vehicle).Reference(v => v.Category).LoadAsync();
        return MapToDto(order);
    }

    public async Task<OrderDto?> UpdateOrderAsync(int id, UpdateOrderDto dto)
    {
        var order = await db.Orders
            .Include(o => o.Vehicle).ThenInclude(v => v.Category)
            .FirstOrDefaultAsync(o => o.Id == id);
        if (order is null) return null;

        var previousStatus = order.Status;

        order.CustomerName = dto.CustomerName;
        order.CustomerEmail = dto.CustomerEmail;
        order.CustomerPhone = dto.CustomerPhone;
        order.SalePrice = dto.SalePrice;
        order.PaymentMethod = dto.PaymentMethod;
        order.DownPayment = dto.DownPayment;
        order.LoanAmount = dto.LoanAmount;
        order.LoanTermMonths = dto.LoanTermMonths;
        order.APR = dto.APR;
        order.LenderName = dto.LenderName;
        order.Status = dto.Status;
        order.Notes = dto.Notes;
        order.TrackingNumber = dto.TrackingNumber;

        if (dto.Status == "Delivered" && order.DeliveredAt is null)
            order.DeliveredAt = DateTime.UtcNow;

        // Stock management on status transitions
        if (previousStatus != "Completed" && dto.Status == "Completed")
            await DecrementStock(order.VehicleId);
        else if (previousStatus == "Completed" && dto.Status == "Cancelled")
            await IncrementStock(order.VehicleId);

        await db.SaveChangesAsync();
        return MapToDto(order);
    }

    public async Task<bool> DeleteOrderAsync(int id)
    {
        var order = await db.Orders.FindAsync(id);
        if (order is null) return false;

        if (order.Status == "Completed" || order.Status == "Delivered")
            await IncrementStock(order.VehicleId);

        db.Orders.Remove(order);
        await db.SaveChangesAsync();
        return true;
    }

    public async Task<FinanceStatsDto> GetFinanceStatsAsync()
    {
        var orders = await db.Orders
            .Include(o => o.Vehicle).ThenInclude(v => v.Category)
            .Where(o => o.Status == "Completed" || o.Status == "Delivered")
            .ToListAsync();

        var now = DateTime.UtcNow;
        var thisMonth = orders.Where(o => o.CreatedAt.Year == now.Year && o.CreatedAt.Month == now.Month).ToList();

        var revenueByCategory = orders
            .GroupBy(o => o.Vehicle.Category.Name)
            .Select(g => new CategoryRevenueDto
            {
                Category = g.Key,
                Revenue = g.Sum(o => o.SalePrice),
                Units = g.Count()
            })
            .OrderByDescending(x => x.Revenue)
            .ToList();

        var paymentBreakdown = orders
            .GroupBy(o => o.PaymentMethod)
            .Select(g => new PaymentMethodBreakdownDto
            {
                Method = g.Key,
                Count = g.Count(),
                Revenue = g.Sum(o => o.SalePrice)
            })
            .OrderByDescending(x => x.Count)
            .ToList();

        var topLenders = orders
            .Where(o => o.PaymentMethod == "Financed" && !string.IsNullOrWhiteSpace(o.LenderName))
            .GroupBy(o => o.LenderName!)
            .Select(g => new LenderBreakdownDto
            {
                Lender = g.Key,
                Count = g.Count(),
                TotalFinanced = g.Sum(o => o.LoanAmount ?? 0)
            })
            .OrderByDescending(x => x.Count)
            .Take(5)
            .ToList();

        // Last 12 months
        var monthlyRevenue = Enumerable.Range(0, 12)
            .Select(i => now.AddMonths(-i))
            .Select(d => new MonthlyRevenueDto
            {
                Year = d.Year,
                Month = d.Month,
                Label = d.ToString("MMM yyyy"),
                Revenue = orders.Where(o => o.CreatedAt.Year == d.Year && o.CreatedAt.Month == d.Month).Sum(o => o.SalePrice),
                Units = orders.Count(o => o.CreatedAt.Year == d.Year && o.CreatedAt.Month == d.Month)
            })
            .Reverse()
            .ToList();

        return new FinanceStatsDto
        {
            TotalRevenue = orders.Sum(o => o.SalePrice),
            TotalUnitsSold = orders.Count,
            AvgSalePrice = orders.Count > 0 ? orders.Average(o => o.SalePrice) : 0,
            ThisMonthRevenue = thisMonth.Sum(o => o.SalePrice),
            ThisMonthUnits = thisMonth.Count,
            RevenueByCategory = revenueByCategory,
            PaymentMethodBreakdown = paymentBreakdown,
            MonthlyRevenue = monthlyRevenue,
            TopLenders = topLenders
        };
    }

    private async Task DecrementStock(int vehicleId)
    {
        var vehicle = await db.Vehicles.FindAsync(vehicleId);
        if (vehicle is not null && vehicle.Stock > 0)
            vehicle.Stock--;
    }

    private async Task IncrementStock(int vehicleId)
    {
        var vehicle = await db.Vehicles.FindAsync(vehicleId);
        if (vehicle is not null)
            vehicle.Stock++;
    }

    private static OrderDto MapToDto(Order o) => new()
    {
        Id = o.Id,
        VehicleId = o.VehicleId,
        VehicleName = $"{o.Vehicle.Year} {o.Vehicle.Make} {o.Vehicle.Model}",
        InquiryId = o.InquiryId,
        CustomerName = o.CustomerName,
        CustomerEmail = o.CustomerEmail,
        CustomerPhone = o.CustomerPhone,
        SalePrice = o.SalePrice,
        PaymentMethod = o.PaymentMethod,
        DownPayment = o.DownPayment,
        LoanAmount = o.LoanAmount,
        LoanTermMonths = o.LoanTermMonths,
        APR = o.APR,
        LenderName = o.LenderName,
        Status = o.Status,
        Notes = o.Notes,
        TrackingNumber = o.TrackingNumber,
        CreatedAt = o.CreatedAt,
        DeliveredAt = o.DeliveredAt
    };
}
