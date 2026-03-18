namespace PowersportsApi.Models;

public enum SenderRole { Customer, Agent, System }

public class ChatMessage
{
    public int Id { get; set; }

    public int SessionId { get; set; }
    public ChatSession Session { get; set; } = null!;

    public string SenderName { get; set; } = string.Empty;
    public SenderRole SenderRole { get; set; }

    public string Body { get; set; } = string.Empty;

    public DateTime SentAt { get; set; } = DateTime.UtcNow;
    public bool IsRead { get; set; } = false;
}
