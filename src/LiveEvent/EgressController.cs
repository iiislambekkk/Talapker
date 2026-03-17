using Microsoft.AspNetCore.Mvc;

namespace LiveEvent;

[ApiController]
[Route("api/egress")]
public class EgressController(EgressService egress) : ControllerBase
{
    private static string? _currentEgressId;

    [HttpPost("start/{roomId}")]
    public async Task<IActionResult> Start(string roomId)
    {
        _currentEgressId = await egress.StartAsync(roomId);
        return Ok(new { egressId = _currentEgressId, liveUrl = egress.GetLiveUrl(roomId) });
    }

    [HttpPost("stop")]
    public async Task<IActionResult> Stop()
    {
        if (_currentEgressId is null) return BadRequest("No active egress");
        await egress.StopAsync(_currentEgressId);
        _currentEgressId = null;
        return Ok();
    }
}