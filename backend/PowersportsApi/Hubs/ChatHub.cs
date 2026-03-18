using System.Collections.Concurrent;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using PowersportsApi.Data;
using PowersportsApi.Models;

namespace PowersportsApi.Hubs;

/// <summary>
/// SignalR hub for live customer chat.
///
/// Groups:
///   session-{id}  — the customer + the assigned agent for a given session
///   agents        — all connected admin users; receives new-session and update events
///
/// Auth:
///   Customer methods — anonymous allowed; identity used when available
///   Agent methods    — require AdminOnly policy (Role = Admin | SuperAdmin)
///
/// Security:
///   - Customers must present the opaque SessionToken issued at session creation
///     before they are admitted to a session group (prevents enumeration / eavesdropping).
///   - Non-agent connections can only SendMessage to sessions they have been admitted to.
///   - Message body is capped at 2 000 characters server-side.
/// </summary>
[AllowAnonymous]
public class ChatHub : Hub
{
    private readonly PowersportsDbContext _db;
    private readonly ILogger<ChatHub> _logger;

    /// <summary>Maps SignalR connectionId → sessionId for customer connections.</summary>
    private static readonly ConcurrentDictionary<string, int> _customerSessions = new();

    public ChatHub(PowersportsDbContext db, ILogger<ChatHub> logger)
    {
        _db = db;
        _logger = logger;
    }

    // ── Customer methods ─────────────────────────────────────────────────────

    /// <summary>
    /// Join the SignalR group for a session.
    /// Requires the opaque token that was returned when the session was created.
    /// </summary>
    public async Task JoinSession(int sessionId, string token)
    {
        var session = await _db.ChatSessions.FindAsync(sessionId);

        if (session == null || session.SessionToken != token)
        {
            await Clients.Caller.SendAsync("Error", "Invalid session or token.");
            _logger.LogWarning("JoinSession rejected — invalid token for session {Id} from {Conn}", sessionId, Context.ConnectionId);
            return;
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, $"session-{sessionId}");
        _customerSessions[Context.ConnectionId] = sessionId;

        _logger.LogDebug("Connection {Conn} joined session-{Id}", Context.ConnectionId, sessionId);
    }

    /// <summary>
    /// Send a message in a session.
    /// Agents (Admin/SuperAdmin) may send to any open session.
    /// Customers may only send to the session they joined via JoinSession.
    /// Message body is capped at 2 000 characters.
    /// </summary>
    public async Task SendMessage(int sessionId, string body)
    {
        if (string.IsNullOrWhiteSpace(body)) return;

        if (body.Length > 2000)
        {
            await Clients.Caller.SendAsync("Error", "Message exceeds the 2 000-character limit.");
            return;
        }

        var session = await _db.ChatSessions.FindAsync(sessionId);
        if (session == null || session.Status == ChatSessionStatus.Closed)
        {
            await Clients.Caller.SendAsync("Error", "Session is closed or does not exist.");
            return;
        }

        bool isAgent = Context.User?.IsInRole("Admin") == true
                    || Context.User?.IsInRole("SuperAdmin") == true;

        // Non-agents must own the session they are writing to
        if (!isAgent)
        {
            if (!_customerSessions.TryGetValue(Context.ConnectionId, out var ownedSession) || ownedSession != sessionId)
            {
                await Clients.Caller.SendAsync("Error", "Not authorised to send to this session.");
                _logger.LogWarning("SendMessage rejected — connection {Conn} tried to write to session {Id} without ownership", Context.ConnectionId, sessionId);
                return;
            }
        }

        string senderName;
        SenderRole role;

        if (isAgent)
        {
            senderName = Context.User!.FindFirst(ClaimTypes.GivenName)?.Value
                      ?? Context.User.FindFirst(ClaimTypes.Name)?.Value
                      ?? "Agent";
            role = SenderRole.Agent;

            // First agent message activates the session
            if (session.Status == ChatSessionStatus.Waiting)
            {
                session.Status = ChatSessionStatus.Active;
                session.AgentConnectionId = Context.ConnectionId;
                await _db.SaveChangesAsync();

                await Clients.Group($"session-{sessionId}").SendAsync("SessionActivated", sessionId);
            }
        }
        else
        {
            senderName = session.GuestName
                      ?? Context.User?.FindFirst(ClaimTypes.GivenName)?.Value
                      ?? "Customer";
            role = SenderRole.Customer;
        }

        var message = new ChatMessage
        {
            SessionId = sessionId,
            SenderName = senderName,
            SenderRole = role,
            Body = body,
            SentAt = DateTime.UtcNow
        };

        _db.ChatMessages.Add(message);
        await _db.SaveChangesAsync();

        var payload = new
        {
            id = message.Id,
            sessionId,
            senderName,
            senderRole = role.ToString(),
            body,
            sentAt = message.SentAt
        };

        await Clients.Group($"session-{sessionId}").SendAsync("ReceiveMessage", payload);

        await Clients.Group("agents").SendAsync("SessionUpdated", new
        {
            sessionId,
            lastMessage = body,
            senderRole = role.ToString(),
            senderName
        });
    }

    // ── Agent methods ────────────────────────────────────────────────────────

    /// <summary>Agents call this on connect to receive new-session notifications.</summary>
    [Authorize(Policy = "AdminOnly")]
    public async Task JoinAgentsRoom()
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, "agents");
        _logger.LogDebug("Agent {Conn} joined agents room", Context.ConnectionId);
    }

    /// <summary>Agent joins the group for a specific session to read/write messages.</summary>
    [Authorize(Policy = "AdminOnly")]
    public async Task JoinSessionAsAgent(int sessionId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"session-{sessionId}");
    }

    /// <summary>Close a session — admin only.</summary>
    [Authorize(Policy = "AdminOnly")]
    public async Task CloseSession(int sessionId)
    {
        var session = await _db.ChatSessions.FindAsync(sessionId);
        if (session == null) return;

        session.Status = ChatSessionStatus.Closed;
        session.ClosedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        _logger.LogInformation("Chat session {Id} closed by agent {Conn}", sessionId, Context.ConnectionId);

        await Clients.Group($"session-{sessionId}").SendAsync("SessionClosed", sessionId);
        await Clients.Group("agents").SendAsync("SessionClosed", sessionId);
    }

    // ── Lifecycle ─────────────────────────────────────────────────────────────

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        _customerSessions.TryRemove(Context.ConnectionId, out _);
        return base.OnDisconnectedAsync(exception);
    }
}
