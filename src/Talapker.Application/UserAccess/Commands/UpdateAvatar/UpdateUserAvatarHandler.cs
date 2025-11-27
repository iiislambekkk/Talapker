using Microsoft.AspNetCore.Identity;
using Talapker.Infrastructure.Data;
using Talapker.Infrastructure.Data.UserAccess;
using Talapker.Infrastructure.Exceptions;
using Talapker.Infrastructure.S3;
using Wolverine.Runtime;

namespace Talapker.Application.UserAccess.Commands.UpdateAvatar;

public class UpdateUserAvatarHandler
{
    public async Task Handle(
        UpdateUserAvatarCommand command,
        IS3StorageService s3Service,
        UserManager<ApplicationUser> userManager,
        MessageBus bus, 
        CancellationToken ct)
    {
        var user = await userManager.FindByIdAsync(command.UserId.ToString());
        if (user is null) throw new NotFoundException($"User with Id {command.UserId} not found");

        if (command.AvatarFile is null || command.AvatarFile.Length == 0)
            throw new InvalidOperationException("Avatar file is required");

        var uniqueKey = $"identity/{Guid.NewGuid()}-{command.FileName}";
        var presignedUrl = await s3Service.GetPresignedUrl(uniqueKey, command.AvatarFile.ContentType);

        await using var stream = command.AvatarFile.OpenReadStream();
        var content = new StreamContent(stream);
        content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(command.AvatarFile.ContentType);

        using var http = new HttpClient();
        var response = await http.PutAsync(presignedUrl, content, ct);
        if (!response.IsSuccessStatusCode)
        {
            var errorText = await response.Content.ReadAsStringAsync(ct);
            throw new Exception($"Upload failed: {(int)response.StatusCode} {errorText}");
        }

        /*
        if (!string.IsNullOrEmpty(user.AvatarKey))
        {
            await bus.SendAsync(new DeleteS3FileCommand.DeleteS3FileCommand(user.AvatarKey));
        }*/

        user.AvatarKey = uniqueKey;
        await userManager.UpdateAsync(user);
    }
}

