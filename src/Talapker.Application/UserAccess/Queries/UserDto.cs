namespace Talapker.Application.UserAccess.Queries;

public record UserDto(string Id, string Email, string FirstName, string LastName, string Image, Guid? TenantId);