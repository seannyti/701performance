using Microsoft.EntityFrameworkCore;
using mperformancepower.Api.Data;
using mperformancepower.Api.DTOs.Appointment;
using mperformancepower.Api.Models;
using mperformancepower.Api.Services.Interfaces;

namespace mperformancepower.Api.Services;

public class AppointmentService(AppDbContext db) : IAppointmentService
{
    public async Task<List<AppointmentDto>> GetAppointmentsAsync(DateTime from, DateTime to)
    {
        return await db.Appointments
            .Include(a => a.Vehicle)
            .Include(a => a.User)
            .Where(a => a.StartTime < to && a.EndTime > from)
            .OrderBy(a => a.StartTime)
            .Select(a => ToDto(a))
            .ToListAsync();
    }

    public async Task<AppointmentDto?> GetAppointmentAsync(int id)
    {
        var a = await db.Appointments.Include(a => a.Vehicle).Include(a => a.User).FirstOrDefaultAsync(a => a.Id == id);
        return a is null ? null : ToDto(a);
    }

    public async Task<(AppointmentDto? appointment, string? error)> CreateAppointmentAsync(CreateAppointmentDto dto)
    {
        if (dto.EndTime <= dto.StartTime)
            return (null, "End time must be after start time.");

        var overlap = await HasOverlapAsync(dto.StartTime, dto.EndTime, excludeId: null);
        if (overlap)
            return (null, "This time slot overlaps with an existing appointment.");

        var appt = new Appointment
        {
            Title = dto.Title,
            CustomerName = dto.CustomerName,
            CustomerEmail = dto.CustomerEmail,
            CustomerPhone = dto.CustomerPhone,
            UserId = dto.UserId,
            VehicleId = dto.VehicleId,
            StartTime = dto.StartTime,
            EndTime = dto.EndTime,
            Notes = dto.Notes,
        };

        db.Appointments.Add(appt);
        await db.SaveChangesAsync();

        await db.Entry(appt).Reference(a => a.Vehicle).LoadAsync();
        await db.Entry(appt).Reference(a => a.User).LoadAsync();
        return (ToDto(appt), null);
    }

    public async Task<(AppointmentDto? appointment, string? error)> UpdateAppointmentAsync(int id, UpdateAppointmentDto dto)
    {
        var appt = await db.Appointments.Include(a => a.Vehicle).FirstOrDefaultAsync(a => a.Id == id);
        if (appt is null) return (null, null);

        if (dto.EndTime <= dto.StartTime)
            return (null, "End time must be after start time.");

        var overlap = await HasOverlapAsync(dto.StartTime, dto.EndTime, excludeId: id);
        if (overlap)
            return (null, "This time slot overlaps with an existing appointment.");

        appt.Title = dto.Title;
        appt.CustomerName = dto.CustomerName;
        appt.CustomerEmail = dto.CustomerEmail;
        appt.CustomerPhone = dto.CustomerPhone;
        appt.UserId = dto.UserId;
        appt.VehicleId = dto.VehicleId;
        appt.StartTime = dto.StartTime;
        appt.EndTime = dto.EndTime;
        appt.Status = dto.Status;
        appt.Notes = dto.Notes;

        await db.SaveChangesAsync();
        await db.Entry(appt).Reference(a => a.Vehicle).LoadAsync();
        await db.Entry(appt).Reference(a => a.User).LoadAsync();
        return (ToDto(appt), null);
    }

    public async Task<bool> DeleteAppointmentAsync(int id)
    {
        var appt = await db.Appointments.FindAsync(id);
        if (appt is null) return false;
        db.Appointments.Remove(appt);
        await db.SaveChangesAsync();
        return true;
    }

    private async Task<bool> HasOverlapAsync(DateTime start, DateTime end, int? excludeId)
    {
        var query = db.Appointments
            .Where(a => a.Status != "Cancelled" && a.StartTime < end && a.EndTime > start);

        if (excludeId.HasValue)
            query = query.Where(a => a.Id != excludeId.Value);

        return await query.AnyAsync();
    }

    private static AppointmentDto ToDto(Appointment a) => new()
    {
        Id = a.Id,
        Title = a.Title,
        CustomerName = a.CustomerName,
        CustomerEmail = a.CustomerEmail,
        CustomerPhone = a.CustomerPhone,
        UserId = a.UserId,
        UserName = a.User is null ? null : $"{a.User.FirstName} {a.User.LastName}".Trim().Length > 0
            ? $"{a.User.FirstName} {a.User.LastName}".Trim()
            : a.User.Email,
        VehicleId = a.VehicleId,
        VehicleName = a.Vehicle is null ? null : $"{a.Vehicle.Year} {a.Vehicle.Make} {a.Vehicle.Model}",
        StartTime = a.StartTime,
        EndTime = a.EndTime,
        Status = a.Status,
        Notes = a.Notes,
        CreatedAt = a.CreatedAt,
    };
}
