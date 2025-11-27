using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace Talapker.Infrastructure.Data.UserAccess;

public class ApplicationUser : IdentityUser
{
    [MaxLength(100)]
    public string FirstName { get; set; } = String.Empty;
    
    [MaxLength(100)]
    public string LastName { get; set; } = String.Empty;
    public string? AvatarKey { get; set; }
    public Guid? TenantId { get; set; }
}