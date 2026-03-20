using System.ComponentModel.DataAnnotations;

namespace mperformancepower.Api.DTOs.Users;

public class AdminUpdateUserDto
{
    [MaxLength(100)] public string? FirstName { get; set; }
    [MaxLength(100)] public string? LastName { get; set; }
    [MaxLength(30)]  public string? Phone { get; set; }
    [EmailAddress, MaxLength(200)] public string? Email { get; set; }
    public string? Role { get; set; }
}
