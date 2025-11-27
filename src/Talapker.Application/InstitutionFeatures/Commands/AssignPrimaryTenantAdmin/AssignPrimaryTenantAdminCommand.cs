namespace Talapker.Application.InstitutionFeatures.Commands.AssignPrimaryTenantAdmin;

public record AssignPrimaryTenantAdminCommand(string Email, Guid InstitutionId);