using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using mperformancepower.Api.DTOs.Appointment;
using mperformancepower.Api.Services.Interfaces;

namespace mperformancepower.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class AppointmentsController(IAppointmentService appointmentService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] DateTime from,
        [FromQuery] DateTime to)
    {
        var results = await appointmentService.GetAppointmentsAsync(from, to);
        return Ok(results);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id)
    {
        var appt = await appointmentService.GetAppointmentAsync(id);
        return appt is null ? NotFound() : Ok(appt);
    }

    [HttpPost, AllowAnonymous]
    public async Task<IActionResult> Create([FromBody] CreateAppointmentDto dto)
    {
        var (appt, error) = await appointmentService.CreateAppointmentAsync(dto);
        if (error is not null) return Conflict(new { message = error });
        return CreatedAtAction(nameof(Get), new { id = appt!.Id }, appt);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateAppointmentDto dto)
    {
        var (appt, error) = await appointmentService.UpdateAppointmentAsync(id, dto);
        if (error is not null) return Conflict(new { message = error });
        return appt is null ? NotFound() : Ok(appt);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var success = await appointmentService.DeleteAppointmentAsync(id);
        return success ? NoContent() : NotFound();
    }
}
