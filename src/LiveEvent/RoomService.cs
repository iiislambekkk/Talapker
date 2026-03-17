using Livekit.Server.Sdk.Dotnet;

namespace LiveEvent;

public class RoomService
{
    private readonly RoomServiceClient _client;

    public RoomService(IConfiguration config)
    {
        _client = new RoomServiceClient(
            config["LiveKit:Host"]!,
            config["LiveKit:ApiKey"]!,
            config["LiveKit:ApiSecret"]!);
    }

    public async Task<string> CreateRoomAsync(string name)
    {
        var room = await _client.CreateRoom(new CreateRoomRequest
        {
            Name = name,
            EmptyTimeout = 300,   // auto-delete after 5min empty
            MaxParticipants = 10,
        });
        return room.Name;
    }

    public async Task DeleteRoomAsync(string roomId) =>
        await _client.DeleteRoom(new DeleteRoomRequest { Room = roomId });
}