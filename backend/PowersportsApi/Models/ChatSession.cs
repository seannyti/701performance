namespace PowersportsApi.Models;

public enum ChatSessionStatus { Waiting, Active, Closed }

public class ChatSession
{
    public int Id { get; set; }

    /// <summary>Null for guest (unauthenticated) sessions.</summary>
    public int? UserId { get; set; }
    public User? User { get; set; }

    /// <summary>Collected from the widget start form for guests.</summary>
    public string? GuestName { get; set; }
    public string? GuestEmail { get; set; }

    /// <summary>SignalR connection ID of the agent currently handling this session.</summary>
    public string? AgentConnectionId { get; set; }

    /// <summary>
    /// Opaque token issued to the customer at session creation.
    /// Required to join the SignalR session group — prevents enumeration / eavesdropping.
    /// </summary>
    public string SessionToken { get; set; } = Guid.NewGuid().ToString();

    public ChatSessionStatus Status { get; set; } = ChatSessionStatus.Waiting;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ClosedAt { get; set; }

    public ICollection<ChatMessage> Messages { get; set; } = new List<ChatMessage>();
}
