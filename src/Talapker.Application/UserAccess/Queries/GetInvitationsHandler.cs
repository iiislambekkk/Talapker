using Microsoft.EntityFrameworkCore;
using Talapker.Application.UserAccess.DTOs;
using Talapker.Infrastructure.Data;
using Talapker.Infrastructure.Data.UserAccess;

namespace Talapker.Application.UserAccess.Queries;

public record GetInvitationsQuery(
    Guid TenantId,
    UserRoles.UserRolesEnum? Role = null
);

public class GetInvitationsHandler
{
    public async Task<ApiResponse<List<InvitationDto>>> Handle(
        GetInvitationsQuery query,
        TalapkerDbContext db)
    {
        var invitations = await db.Invitations
            .AsNoTracking()
            .Where(i => i.TenantId == query.TenantId)
            .Where(i => query.Role == null || i.Role == query.Role)
            .OrderByDescending(i => i.CreatedAt)
            .Select(i => new InvitationDto(
                i.Id,
                i.Email,
                i.TenantId,
                i.Role,
                i.Status,
                i.CreatedAt,
                i.ExpiresAt,
                i.RespondedAt,
                i.InvitedByUserId,
                i.AcceptedByUserId
            ))
            .ToListAsync();

        return ApiResponse<List<InvitationDto>>.Success(invitations);
    }
}