using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using mperformancepower.Api.Data;
using mperformancepower.Api.DTOs.Appointment;
using mperformancepower.Api.DTOs.Order;
using SixLabors.ImageSharp;

namespace mperformancepower.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProfileController(AppDbContext db, IWebHostEnvironment env) : ControllerBase
{
    private int CurrentUserId =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    private string CurrentUserEmail =>
        User.FindFirstValue(ClaimTypes.Email) ?? string.Empty;

    // ── GET /api/profile ─────────────────────────────────────────
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var user = await db.AppUsers.FindAsync(CurrentUserId);
        if (user is null) return NotFound();

        return Ok(new
        {
            user.Id,
            user.Email,
            user.FirstName,
            user.LastName,
            user.Phone,
            user.AvatarPath,
            user.CreatedAt,
        });
    }

    // ── PUT /api/profile ─────────────────────────────────────────
    [HttpPut]
    public async Task<IActionResult> Update([FromBody] UpdateProfileDto dto)
    {
        var user = await db.AppUsers.FindAsync(CurrentUserId);
        if (user is null) return NotFound();

        user.FirstName = dto.FirstName?.Trim();
        user.LastName = dto.LastName?.Trim();
        user.Phone = dto.Phone?.Trim();
        await db.SaveChangesAsync();

        return Ok(new { user.FirstName, user.LastName, user.Phone });
    }

    // ── POST /api/profile/change-password ────────────────────────
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
    {
        var user = await db.AppUsers.FindAsync(CurrentUserId);
        if (user is null) return NotFound();

        if (!BCrypt.Net.BCrypt.Verify(dto.CurrentPassword, user.PasswordHash))
            return BadRequest(new { message = "Current password is incorrect." });

        if (dto.NewPassword.Length < 8)
            return BadRequest(new { message = "New password must be at least 8 characters." });

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
        await db.SaveChangesAsync();

        return Ok(new { message = "Password updated successfully." });
    }

    // ── POST /api/profile/avatar ──────────────────────────────────
    [HttpPost("avatar")]
    [RequestSizeLimit(6 * 1024 * 1024)] // 6 MB request limit
    public async Task<IActionResult> UploadAvatar(IFormFile file)
    {
        if (file is null || file.Length == 0)
            return BadRequest(new { message = "No file provided." });

        if (file.Length > 5 * 1024 * 1024)
            return BadRequest(new { message = "Avatar must be under 5 MB." });

        var imageInfo = await Image.IdentifyAsync(file.OpenReadStream());
        if (imageInfo is null)
            return BadRequest(new { message = "Uploaded file is not a valid image." });

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (ext is not (".jpg" or ".jpeg" or ".png" or ".webp"))
            ext = ".jpg"; // fall back to a safe extension if missing

        var uploadsDir = Path.Combine(env.WebRootPath, "uploads", "avatars");
        Directory.CreateDirectory(uploadsDir);

        var fileName = $"avatar_{CurrentUserId}_{Guid.NewGuid():N}{ext}";
        var filePath = Path.Combine(uploadsDir, fileName);

        await using (var stream = System.IO.File.Create(filePath))
            await file.CopyToAsync(stream);

        var user = await db.AppUsers.FindAsync(CurrentUserId);
        if (user is null) return NotFound();

        // Delete old avatar file if exists
        if (!string.IsNullOrWhiteSpace(user.AvatarPath))
        {
            var oldPath = Path.Combine(env.WebRootPath, "uploads", "avatars",
                Path.GetFileName(user.AvatarPath));
            if (System.IO.File.Exists(oldPath))
                System.IO.File.Delete(oldPath);
        }

        user.AvatarPath = $"avatars/{fileName}";
        await db.SaveChangesAsync();

        return Ok(new { avatarPath = user.AvatarPath });
    }

    // ── DELETE /api/profile/avatar ───────────────────────────────
    [HttpDelete("avatar")]
    public async Task<IActionResult> DeleteAvatar()
    {
        var user = await db.AppUsers.FindAsync(CurrentUserId);
        if (user is null) return NotFound();

        if (!string.IsNullOrWhiteSpace(user.AvatarPath))
        {
            var oldPath = Path.Combine(env.WebRootPath, "uploads", "avatars",
                Path.GetFileName(user.AvatarPath));
            if (System.IO.File.Exists(oldPath))
                System.IO.File.Delete(oldPath);
            user.AvatarPath = null;
            await db.SaveChangesAsync();
        }

        return Ok(new { avatarPath = (string?)null });
    }

    // ── GET /api/profile/appointments ────────────────────────────
    [HttpGet("appointments")]
    public async Task<IActionResult> GetAppointments()
    {
        var email = CurrentUserEmail;
        var userId = CurrentUserId;
        var now = DateTime.UtcNow;

        var appts = await db.Appointments
            .Include(a => a.Vehicle)
            .Where(a => a.StartTime >= now &&
                        a.Status == "Scheduled" &&
                        (a.UserId == userId || a.CustomerEmail == email))
            .OrderBy(a => a.StartTime)
            .Select(a => new AppointmentDto
            {
                Id = a.Id,
                Title = a.Title,
                CustomerName = a.CustomerName,
                CustomerEmail = a.CustomerEmail,
                CustomerPhone = a.CustomerPhone,
                UserId = a.UserId,
                VehicleId = a.VehicleId,
                VehicleName = a.Vehicle != null ? $"{a.Vehicle.Year} {a.Vehicle.Make} {a.Vehicle.Model}" : null,
                StartTime = a.StartTime,
                EndTime = a.EndTime,
                Status = a.Status,
                Notes = a.Notes,
                CreatedAt = a.CreatedAt,
            })
            .ToListAsync();

        return Ok(appts);
    }

    // ── GET /api/profile/orders ───────────────────────────────────
    [HttpGet("orders")]
    public async Task<IActionResult> GetOrders()
    {
        var email = CurrentUserEmail;

        var orders = await db.Orders
            .Include(o => o.Vehicle)
            .Where(o => o.CustomerEmail == email)
            .OrderByDescending(o => o.CreatedAt)
            .Select(o => new OrderDto
            {
                Id = o.Id,
                VehicleId = o.VehicleId,
                VehicleName = $"{o.Vehicle.Year} {o.Vehicle.Make} {o.Vehicle.Model}",
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
                DeliveredAt = o.DeliveredAt,
            })
            .ToListAsync();

        return Ok(orders);
    }
}

public record UpdateProfileDto(string? FirstName, string? LastName, string? Phone);
public record ChangePasswordDto(string CurrentPassword, string NewPassword);
