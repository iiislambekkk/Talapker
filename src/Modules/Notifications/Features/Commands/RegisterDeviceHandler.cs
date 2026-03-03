using Marten;
using Talapker.Application;
using Talapker.Notifications.Models;

namespace Talapker.Notifications.Features.Commands;

public record RegisterDeviceCommand(
    Guid UserId,
    string DeviceToken,
    string? Platform,
    string? DeviceModel,
    string? AppVersion
);

public class RegisterDeviceHandler
{
    public async Task<ApiResponse> Handle(RegisterDeviceCommand command, IDocumentSession session)
    {
        var existingDevice = await session.Query<UserDevice>()
            .FirstOrDefaultAsync(d => d.UserId == command.UserId && 
                                      d.DeviceToken == command.DeviceToken);

        if (existingDevice != null)
        {
            existingDevice.Platform = command.Platform;
            existingDevice.DeviceModel = command.DeviceModel;
            existingDevice.AppVersion = command.AppVersion;
            existingDevice.LastUsedAt = DateTime.UtcNow;
            existingDevice.IsActive = true;
            
            session.Update(existingDevice);
        }
        else
        {
            var device = new UserDevice
            {
                Id = Guid.NewGuid(),
                UserId = command.UserId,
                DeviceToken = command.DeviceToken,
                Platform = command.Platform,
                DeviceModel = command.DeviceModel,
                AppVersion = command.AppVersion,
                IsActive = true,
                RegisteredAt = DateTime.UtcNow,
                LastUsedAt = DateTime.UtcNow
            };
            
            session.Store(device);
        }
        
        await session.SaveChangesAsync();
        return ApiResponse.Success();
    }
}