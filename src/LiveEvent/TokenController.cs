using Microsoft.AspNetCore.Mvc;

namespace LiveEvent;

[ApiController]
[Route("api/[controller]")]
public class TokenController(LiveKitService liveKit) : ControllerBase
{
    [HttpPost("host")]
    public IActionResult Host([FromBody] TokenRequest req) =>
        Ok(new { token = liveKit.CreateHostToken(req.RoomId, req.UserId, req.Username) });

    [HttpPost("viewer")]
    public IActionResult Viewer([FromBody] TokenRequest req) =>
        Ok(new { token = liveKit.CreateViewerToken(req.RoomId, req.UserId, req.Username) });
}

public record TokenRequest(string RoomId, string UserId, string Username);