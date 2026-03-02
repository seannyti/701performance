namespace PowersportsApi.Models;

/// <summary>
/// Defines the available user roles in the system
/// </summary>
public enum UserRole
{
    /// <summary>
    /// Standard user role with basic access permissions
    /// </summary>
    User = 0,
    
    /// <summary>
    /// Administrator role with elevated permissions for content management
    /// </summary>
    Admin = 1,
    
    /// <summary>
    /// Super administrator role with full system access and user management
    /// </summary>
    SuperAdmin = 2
}

/// <summary>
/// Extension methods for UserRole enum
/// </summary>
public static class UserRoleExtensions
{
    /// <summary>
    /// Gets the string representation of the role
    /// </summary>
    public static string GetRoleName(this UserRole role)
    {
        return role switch
        {
            UserRole.User => "User",
            UserRole.Admin => "Admin",
            UserRole.SuperAdmin => "SuperAdmin",
            _ => "Unknown"
        };
    }
    
    /// <summary>
    /// Checks if the role has administrative privileges
    /// </summary>
    public static bool IsAdmin(this UserRole role)
    {
        return role == UserRole.Admin || role == UserRole.SuperAdmin;
    }
    
    /// <summary>
    /// Checks if the role has super admin privileges
    /// </summary>
    public static bool IsSuperAdmin(this UserRole role)
    {
        return role == UserRole.SuperAdmin;
    }
}