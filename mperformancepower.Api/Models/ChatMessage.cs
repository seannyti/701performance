namespace mperformancepower.Api.Models;

public class ChatMessage
{
    public int Id { get; set; }
    public string SessionId { get; set; } = string.Empty;
    public ChatSession Session { get; set; } = null!;
    public string SenderType { get; set; } = "Visitor"; // Visitor, Admin
    public string SenderName { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime SentAt { get; set; } = DateTime.UtcNow;
}
