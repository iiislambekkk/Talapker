using Talapker.Infrastructure.Data.UserAccess;

namespace Talapker.Application.UserAccess.DTOs;

public record InvitationDto(
    Guid Id,
    string Email,
    Guid TenantId,
    UserRoles.UserRolesEnum Role,
    InvitationStatus Status,
    DateTime CreatedAt,
    DateTime ExpiresAt,
    DateTime? RespondedAt,
    Guid? InvitedByUserId,
    Guid? AcceptedByUserId
);