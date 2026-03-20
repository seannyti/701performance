using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using mperformancepower.Api.Data;
using mperformancepower.Api.DTOs.Users;
using mperformancepower.Api.Services.Interfaces;

namespace mperformancepower.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class UsersController(AppDbContext db, IMailService mailService) : ControllerBase
{
    // ── Admin: list all users ──────────────────────────────────────
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null,
        [FromQuery] string? role = null)
    {
        var query = db.AppUsers.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(u =>
                u.Email.ToLower().Contains(term) ||
                (u.FirstName != null && u.FirstName.ToLower().Contains(term)) ||
                (u.LastName != null && u.LastName.ToLower().Contains(term)));
        }

        if (!string.IsNullOrWhiteSpace(role))
            query = query.Where(u => u.Role == role);

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(u => u.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(u => new UserDto
            {
                Id = u.Id,
                Email = u.Email,
                FirstName = u.FirstName,
                LastName = u.LastName,
                Phone = u.Phone,
                Role = u.Role,
                EmailVerified = u.EmailVerified,
                IsActive = u.IsActive,
                CreatedAt = u.CreatedAt,
            })
            .ToListAsync();

        return Ok(new { items, totalCount = total, page, pageSize });
    }

    // ── Admin: get single user ────────────────────────────────────
    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id)
    {
        var u = await db.AppUsers.FindAsync(id);
        if (u is null) return NotFound();
        return Ok(MapToDto(u));
    }

    // ── Admin: update user info ───────────────────────────────────
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] AdminUpdateUserDto dto)
    {
        var user = await db.AppUsers.FindAsync(id);
        if (user is null) return NotFound();

        if (dto.Email is not null && dto.Email != user.Email)
        {
            var taken = await db.AppUsers.AnyAsync(u => u.Email == dto.Email && u.Id != id);
            if (taken) return Conflict(new { message = "Email is already in use." });
            user.Email = dto.Email;
        }

        if (dto.FirstName is not null) user.FirstName = dto.FirstName;
        if (dto.LastName is not null)  user.LastName  = dto.LastName;
        if (dto.Phone is not null)     user.Phone     = dto.Phone;
        if (dto.Role is not null && (dto.Role == "Admin" || dto.Role == "Customer"))
            user.Role = dto.Role;

        if (dto.EmailVerified is not null)
        {
            user.EmailVerified = dto.EmailVerified.Value;
            if (dto.EmailVerified.Value)
            {
                user.EmailVerificationToken = null;
                user.EmailVerificationTokenExpiry = null;
            }
        }

        await db.SaveChangesAsync();
        return Ok(MapToDto(user));
    }

    // ── Admin: force-verify email ─────────────────────────────────
    [HttpPost("{id:int}/verify")]
    public async Task<IActionResult> ForceVerify(int id)
    {
        var user = await db.AppUsers.FindAsync(id);
        if (user is null) return NotFound();

        user.EmailVerified = true;
        user.EmailVerificationToken = null;
        user.EmailVerificationTokenExpiry = null;
        await db.SaveChangesAsync();

        return Ok(new { message = "Email marked as verified." });
    }

    // ── Admin: toggle active/inactive ────────────────────────────
    [HttpPost("{id:int}/toggle-active")]
    public async Task<IActionResult> ToggleActive(int id)
    {
        var user = await db.AppUsers.FindAsync(id);
        if (user is null) return NotFound();

        user.IsActive = !user.IsActive;
        await db.SaveChangesAsync();

        return Ok(new { isActive = user.IsActive });
    }

    // ── Admin: reset password (sends temp password via email) ────────
    [HttpPost("{id:int}/reset-password")]
    public async Task<IActionResult> ResetPassword(int id)
    {
        var user = await db.AppUsers.FindAsync(id);
        if (user is null) return NotFound();

        var tempPassword = GenerateTempPassword();
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(tempPassword);
        await db.SaveChangesAsync();

        await mailService.SendTempPasswordAsync(user.Email, tempPassword);

        return NoContent();
    }

    // ── Admin: delete user ────────────────────────────────────────
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var user = await db.AppUsers.FindAsync(id);
        if (user is null) return NotFound();

        // Prevent deleting the last admin
        if (user.Role == "Admin")
        {
            var adminCount = await db.AppUsers.CountAsync(u => u.Role == "Admin");
            if (adminCount <= 1)
                return Conflict(new { message = "Cannot delete the last admin account." });
        }

        db.AppUsers.Remove(user);
        await db.SaveChangesAsync();
        return NoContent();
    }

    // ── Customer typeahead search (existing) ─────────────────────
    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string q = "")
    {
        var term = q.Trim().ToLower();

        var users = await db.AppUsers
            .Where(u => u.Role == "Customer" && (
                (u.FirstName != null && u.FirstName.ToLower().Contains(term)) ||
                (u.LastName != null && u.LastName.ToLower().Contains(term)) ||
                u.Email.ToLower().Contains(term)))
            .OrderBy(u => u.FirstName ?? u.Email)
            .Take(20)
            .Select(u => new { u.Id, u.FirstName, u.LastName, u.Email, u.Phone })
            .ToListAsync();

        return Ok(users);
    }

    private static UserDto MapToDto(mperformancepower.Api.Models.AppUser u) => new()
    {
        Id = u.Id,
        Email = u.Email,
        FirstName = u.FirstName,
        LastName = u.LastName,
        Phone = u.Phone,
        Role = u.Role,
        EmailVerified = u.EmailVerified,
        IsActive = u.IsActive,
        CreatedAt = u.CreatedAt,
    };

    private static string GenerateTempPassword()
    {
        const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghjkmnpqrstuvwxyz23456789!@#";
        return new string(System.Security.Cryptography.RandomNumberGenerator.GetItems<char>(chars, 12));
    }
}
