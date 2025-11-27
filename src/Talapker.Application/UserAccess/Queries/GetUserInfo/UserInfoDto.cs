namespace Talapker.Application.UserAccess.Queries.GetUserInfo;

public record UserInfoDto
(
    string Id, 
    string FirstName, 
    string LastName, 
    string Email, 
    string Image,
    List<string> Roles,
    Guid? TenantId
);
