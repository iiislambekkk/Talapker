namespace Talapker.Infrastructure.Data.UserAccess;

public class Invitation
{
    public Guid Id { get; set; }
    public string Email { get; set; }
    public Guid TenantId { get; set; }
    public Institution.Institution? Institution { get; set; }

    public UserRoles.UserRolesEnum Role { get; set; }
    public InvitationStatus Status { get; set; }

    public string SecretCodeHash { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public DateTime? RespondedAt { get; set; }

    public Guid? InvitedByUserId { get; set; }
    public Guid? AcceptedByUserId { get; set; }  
}

public enum InvitationStatus
{
    Pending,
    Accepted,
    Declined,
    Expired,
    Revoked
}