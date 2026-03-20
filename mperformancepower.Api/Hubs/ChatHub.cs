using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using mperformancepower.Api.Data;
using mperformancepower.Api.Models;

namespace mperformancepower.Api.Hubs;

public class ChatHub(AppDbContext db) : Hub
{
    // ── Visitor: start or rejoin a session ──────────────────────────
    public async Task<object> StartSession(string visitorName, string visitorEmail)
    {
        var session = new ChatSession
        {
            VisitorName = visitorName,
            VisitorEmail = visitorEmail,
        };
        db.ChatSessions.Add(session);
        await db.SaveChangesAsync();

        await Groups.AddToGroupAsync(Context.ConnectionId, SessionGroup(session.Id));

        // Notify admins a new session started
        await Clients.Group("Admins").SendAsync("SessionStarted", new
        {
            id = session.Id,
            visitorName = session.VisitorName,
            visitorEmail = session.VisitorEmail,
            startedAt = session.StartedAt,
            lastMessageAt = session.LastMessageAt,
            status = session.Status,
        });

        return new { sessionId = session.Id };
    }

    // ── Visitor: rejoin existing session (on reconnect) ─────────────
    public async Task RejoinSession(string sessionId)
    {
        var session = await db.ChatSessions.FindAsync(sessionId);
        if (session is null) return;
        await Groups.AddToGroupAsync(Context.ConnectionId, SessionGroup(sessionId));
    }

    // ── Visitor: send a message ──────────────────────────────────────
    public async Task SendMessage(string sessionId, string content)
    {
        var session = await db.ChatSessions.FindAsync(sessionId);
        if (session is null || session.Status == "Closed") return;

        var msg = new ChatMessage
        {
            SessionId = sessionId,
            SenderType = "Visitor",
            SenderName = session.VisitorName,
            Content = content,
        };
        db.ChatMessages.Add(msg);
        session.LastMessageAt = DateTime.UtcNow;
        await db.SaveChangesAsync();

        var payload = MessagePayload(msg);

        // Deliver to everyone in the session group EXCEPT the sender
        // (visitor keeps their optimistic update; admins viewing this session get it once)
        await Clients.OthersInGroup(SessionGroup(sessionId)).SendAsync("ReceiveMessage", payload);
        // Notify ALL admins so they can increment unread and re-sort the session list
        await Clients.Group("Admins").SendAsync("NewVisitorMessage", new { sessionId, lastMessageAt = session.LastMessageAt });
    }

    // ── Admin: join admin group ──────────────────────────────────────
    public async Task AdminJoin()
    {
        if (Context.User?.IsInRole("Admin") != true) return;
        await Groups.AddToGroupAsync(Context.ConnectionId, "Admins");
    }

    // ── Admin: join a session group to receive its messages ─────────
    public async Task AdminJoinSession(string sessionId)
    {
        if (Context.User?.IsInRole("Admin") != true) return;
        await Groups.AddToGroupAsync(Context.ConnectionId, SessionGroup(sessionId));
    }

    // ── Admin: reply to a visitor ────────────────────────────────────
    public async Task AdminReply(string sessionId, string content)
    {
        if (Context.User?.IsInRole("Admin") != true) return;

        var session = await db.ChatSessions.FindAsync(sessionId);
        if (session is null || session.Status == "Closed") return;

        var adminName = Context.User?.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value
                      ?? Context.User?.FindFirst("email")?.Value
                      ?? "Admin";
        var msg = new ChatMessage
        {
            SessionId = sessionId,
            SenderType = "Admin",
            SenderName = adminName,
            Content = content,
        };
        db.ChatMessages.Add(msg);
        session.LastMessageAt = DateTime.UtcNow;
        await db.SaveChangesAsync();

        var payload = MessagePayload(msg);

        // Deliver to session group (visitor + any other admin viewers)
        // The replying admin is also in this group so they get the echo once
        await Clients.Group(SessionGroup(sessionId)).SendAsync("ReceiveMessage", payload);
        // Update session ordering for all admins
        await Clients.Group("Admins").SendAsync("SessionUpdated", new { sessionId, lastMessageAt = session.LastMessageAt });
    }

    // ── Admin: close a session ───────────────────────────────────────
    public async Task AdminCloseSession(string sessionId)
    {
        if (Context.User?.IsInRole("Admin") != true) return;

        var session = await db.ChatSessions.FindAsync(sessionId);
        if (session is null) return;

        session.Status = "Closed";
        await db.SaveChangesAsync();

        await Clients.Group(SessionGroup(sessionId)).SendAsync("SessionClosed", sessionId);
        await Clients.Group("Admins").SendAsync("SessionClosed", sessionId);
    }

    // ── Helpers ──────────────────────────────────────────────────────
    private static string SessionGroup(string sessionId) => $"session_{sessionId}";

    private static object MessagePayload(ChatMessage m) => new
    {
        id = m.Id,
        sessionId = m.SessionId,
        senderType = m.SenderType,
        senderName = m.SenderName,
        content = m.Content,
        sentAt = m.SentAt,
    };
}
