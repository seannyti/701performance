using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using mperformancepower.Api.Data;

namespace mperformancepower.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ChatController(AppDbContext db) : ControllerBase
{
    [HttpGet("sessions")]
    public async Task<IActionResult> GetSessions()
    {
        var sessions = await db.ChatSessions
            .OrderByDescending(s => s.LastMessageAt)
            .Select(s => new
            {
                s.Id,
                s.VisitorName,
                s.VisitorEmail,
                s.Status,
                s.StartedAt,
                s.LastMessageAt,
                UnreadCount = s.Messages.Count(m => m.SenderType == "Visitor"),
            })
            .ToListAsync();

        return Ok(sessions);
    }

    [HttpGet("sessions/{sessionId}/messages")]
    public async Task<IActionResult> GetMessages(string sessionId)
    {
        var session = await db.ChatSessions.FindAsync(sessionId);
        if (session is null) return NotFound();

        var messages = await db.ChatMessages
            .Where(m => m.SessionId == sessionId)
            .OrderBy(m => m.SentAt)
            .Select(m => new
            {
                m.Id,
                m.SessionId,
                m.SenderType,
                m.SenderName,
                m.Content,
                m.SentAt,
            })
            .ToListAsync();

        return Ok(new
        {
            session = new
            {
                session.Id,
                session.VisitorName,
                session.VisitorEmail,
                session.Status,
                session.StartedAt,
                session.LastMessageAt,
            },
            messages,
        });
    }

    [HttpPatch("sessions/{sessionId}/close")]
    public async Task<IActionResult> CloseSession(string sessionId)
    {
        var session = await db.ChatSessions.FindAsync(sessionId);
        if (session is null) return NotFound();
        session.Status = "Closed";
        await db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("sessions/{sessionId}")]
    public async Task<IActionResult> DeleteSession(string sessionId)
    {
        var session = await db.ChatSessions.FindAsync(sessionId);
        if (session is null) return NotFound();
        db.ChatSessions.Remove(session);
        await db.SaveChangesAsync();
        return NoContent();
    }
}
