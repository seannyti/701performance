namespace mperformancepower.Api.Models;

public class ChatSession
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string VisitorName { get; set; } = string.Empty;
    public string VisitorEmail { get; set; } = string.Empty;
    public string Status { get; set; } = "Active"; // Active, Closed
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
    public DateTime LastMessageAt { get; set; } = DateTime.UtcNow;

    public ICollection<ChatMessage> Messages { get; set; } = [];
}
