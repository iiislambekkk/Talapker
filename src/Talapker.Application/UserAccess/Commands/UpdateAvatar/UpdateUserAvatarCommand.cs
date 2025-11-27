using Microsoft.AspNetCore.Http;

namespace Talapker.Application.UserAccess.Commands.UpdateAvatar;

public record UpdateUserAvatarCommand(
    Guid UserId,
    IFormFile AvatarFile,
    string FileName
);
