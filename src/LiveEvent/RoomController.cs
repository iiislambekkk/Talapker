using LiveEvent;
using Livekit.Server.Sdk.Dotnet;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/room")]
public class RoomController(RoomService rooms, LiveKitService liveKit) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateRoomDto dto)
    {
        var roomId = await rooms.CreateRoomAsync(dto.Name);
        return Ok(new { roomId });
    }

    [HttpDelete("{roomId}")]
    public async Task<IActionResult> Delete(string roomId)
    {
        await rooms.DeleteRoomAsync(roomId);
        return Ok();
    }
    
    [HttpGet]
    public async Task<IActionResult> ListRooms()
    {
        var client = liveKit.GetRoomClient();
        var rooms = await client.ListRooms(new ListRoomsRequest());
        return Ok(rooms.Rooms.Select(r => new
        {
            r.Name,
            r.NumParticipants,
            r.NumPublishers,
        }));
    }

    [HttpGet("{roomId}/participants")]
    public async Task<IActionResult> ListParticipants(string roomId)
    {
        var client = liveKit.GetRoomClient();
        var result = await client.ListParticipants(new ListParticipantsRequest { Room = roomId });
        return Ok(result.Participants.Select(p => new
        {
            p.Identity,
            p.Name,
            p.IsPublisher,
        }));
    }
}

public record CreateRoomDto(string Name);