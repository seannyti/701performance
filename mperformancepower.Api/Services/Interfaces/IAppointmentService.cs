using mperformancepower.Api.DTOs.Appointment;

namespace mperformancepower.Api.Services.Interfaces;

public interface IAppointmentService
{
    Task<List<AppointmentDto>> GetAppointmentsAsync(DateTime from, DateTime to);
    Task<AppointmentDto?> GetAppointmentAsync(int id);
    Task<(AppointmentDto? appointment, string? error)> CreateAppointmentAsync(CreateAppointmentDto dto);
    Task<(AppointmentDto? appointment, string? error)> UpdateAppointmentAsync(int id, UpdateAppointmentDto dto);
    Task<bool> DeleteAppointmentAsync(int id);
}
