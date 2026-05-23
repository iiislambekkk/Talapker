using Microsoft.AspNetCore.SignalR;

namespace Talapker.Application.AI.Knowledge;

public class KnowledgeHub : Hub
{
    public async Task JoinInstitution(string institutionId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"institution:{institutionId}");
    }

    public async Task LeaveInstitution(string institutionId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"institution:{institutionId}");
    }
}