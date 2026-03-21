using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using mperformancepower.Api.Data;
using mperformancepower.Api.DTOs.Common;
using mperformancepower.Api.DTOs.Inquiry;
using mperformancepower.Api.Models;
using mperformancepower.Api.Hubs;
using mperformancepower.Api.Services.Interfaces;

namespace mperformancepower.Api.Services;

public class InquiryService(
    AppDbContext db,
    IMailService mail,
    IHubContext<NotificationHub> hub,
    IServiceScopeFactory scopeFactory) : IInquiryService
{
    public async Task<InquiryDto> CreateInquiryAsync(CreateInquiryDto dto)
    {
        var inquiry = new Inquiry
        {
            VehicleId = dto.VehicleId,
            Name = dto.Name,
            Email = dto.Email,
            Phone = dto.Phone,
            Message = dto.Message,
            Status = "New"
        };

        db.Inquiries.Add(inquiry);
        await db.SaveChangesAsync();

        string? vehicleName = null;
        if (dto.VehicleId.HasValue)
        {
            var v = await db.Vehicles.FindAsync(dto.VehicleId.Value);
            vehicleName = v is not null ? $"{v.Year} {v.Make} {v.Model}" : null;
        }

        var result = MapToDto(inquiry, vehicleName);

        _ = Task.Run(async () =>
        {
            using var scope = scopeFactory.CreateScope();
            var scopedMail = scope.ServiceProvider.GetRequiredService<IMailService>();
            await scopedMail.SendInquiryConfirmationAsync(result);
            await scopedMail.SendAdminNotificationAsync(result);
        });

        await hub.Clients.Group("Admins").SendAsync("NewInquiry", result);

        return result;
    }

    public async Task<PagedResultDto<InquiryDto>> GetInquiriesAsync(
        int page, int pageSize, string? status, string? search, DateTime? from, DateTime? to)
    {
        var query = db.Inquiries.Include(i => i.Vehicle).AsQueryable();

        if (!string.IsNullOrWhiteSpace(status) && status != "All")
            query = query.Where(i => i.Status == status);

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(i =>
                i.Name.Contains(search) ||
                i.Email.Contains(search) ||
                i.Message.Contains(search));

        if (from.HasValue)
            query = query.Where(i => i.CreatedAt >= from.Value);

        if (to.HasValue)
            query = query.Where(i => i.CreatedAt <= to.Value.AddDays(1));

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(i => i.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResultDto<InquiryDto>
        {
            Items = items.Select(i => MapToDto(i,
                i.Vehicle is not null ? $"{i.Vehicle.Year} {i.Vehicle.Make} {i.Vehicle.Model}" : null)).ToList(),
            TotalCount = total,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<InquiryDto?> GetInquiryAsync(int id)
    {
        var i = await db.Inquiries.Include(i => i.Vehicle).FirstOrDefaultAsync(i => i.Id == id);
        if (i is null) return null;
        var vehicleName = i.Vehicle is not null ? $"{i.Vehicle.Year} {i.Vehicle.Make} {i.Vehicle.Model}" : null;
        return MapToDto(i, vehicleName);
    }

    public async Task<InquiryDto?> UpdateStatusAsync(int id, string status)
    {
        var inquiry = await db.Inquiries.Include(i => i.Vehicle).FirstOrDefaultAsync(i => i.Id == id);
        if (inquiry is null) return null;

        inquiry.Status = status;

        // Record first response time when moving out of New
        if (status != "New" && inquiry.RespondedAt is null)
            inquiry.RespondedAt = DateTime.UtcNow;

        await db.SaveChangesAsync();

        var vehicleName = inquiry.Vehicle is not null
            ? $"{inquiry.Vehicle.Year} {inquiry.Vehicle.Make} {inquiry.Vehicle.Model}"
            : null;
        return MapToDto(inquiry, vehicleName);
    }

    public async Task<InquiryStatsDto> GetStatsAsync()
    {
        var all = await db.Inquiries.ToListAsync();

        var responded = all.Where(i => i.RespondedAt.HasValue).ToList();
        double? avg = responded.Count > 0
            ? responded.Average(i => (i.RespondedAt!.Value - i.CreatedAt).TotalMinutes)
            : null;

        return new InquiryStatsDto
        {
            Total = all.Count,
            New = all.Count(i => i.Status == "New"),
            InProgress = all.Count(i => i.Status == "InProgress"),
            Resolved = all.Count(i => i.Status == "Resolved"),
            AvgResponseMinutes = avg.HasValue ? Math.Round(avg.Value, 1) : null
        };
    }

    public async Task<bool> DeleteInquiryAsync(int id)
    {
        var inquiry = await db.Inquiries.FindAsync(id);
        if (inquiry is null) return false;
        db.Inquiries.Remove(inquiry);
        await db.SaveChangesAsync();
        return true;
    }

    private static InquiryDto MapToDto(Inquiry i, string? vehicleName) => new()
    {
        Id = i.Id,
        VehicleId = i.VehicleId,
        VehicleName = vehicleName,
        Name = i.Name,
        Email = i.Email,
        Phone = i.Phone,
        Message = i.Message,
        CreatedAt = i.CreatedAt,
        Status = i.Status,
        RespondedAt = i.RespondedAt
    };
}
