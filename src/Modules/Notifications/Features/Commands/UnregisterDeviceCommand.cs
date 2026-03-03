using Marten;
using Talapker.Application;
using Talapker.Notifications.Models;

namespace Talapker.Notifications.Features.Commands;

public record UnregisterDeviceCommand(
    string DeviceToken
);

public class UnregisterDeviceHandler
{
    public async Task Handle(UnregisterDeviceCommand command, IDocumentSession session)
    {
        var device = await session.Query<UserDevice>()
            .FirstOrDefaultAsync(d =>  
                d.DeviceToken == command.DeviceToken);
        
        if (device != null)
        {
            device.IsActive = false;
            session.Update(device);
            await session.SaveChangesAsync();
        }
    }
}