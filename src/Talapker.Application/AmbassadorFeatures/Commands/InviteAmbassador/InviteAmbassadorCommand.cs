namespace Talapker.Application.AmbassadorFeatures.InviteAmbassador;

public record InviteAmbassadorCommand(string Email, Guid TenantId);