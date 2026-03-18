using System.ComponentModel.DataAnnotations;

namespace PowersportsApi.Models;

public class ContactSubmission
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string Email { get; set; }
    public string? Subject { get; set; }
    public required string Message { get; set; }
    public ContactStatus Status { get; set; } = ContactStatus.New;
    [StringLength(2000)]
    public string? AdminNotes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ReadAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public int? AssignedToUserId { get; set; }
    public User? AssignedToUser { get; set; }
}

public enum ContactStatus
{
    New,
    Read,
    InProgress,
    Replied,
    Resolved,
    Archived
}
