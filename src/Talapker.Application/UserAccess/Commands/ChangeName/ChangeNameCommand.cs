namespace Talapker.UserAccess.Application.Features.ChangeName;

public record ChangeNameCommand(Guid UserId, string FirstName, string LastName);