using Microsoft.AspNetCore.Mvc;
using Talapker.Application.InstitutionFeatures.DTOs;
using Talapker.Application.ProspectFeatures.Comands;
using Talapker.Application.ProspectFeatures.Queries;
using Wolverine;

namespace Talapker.Web.Controllers;

[ApiController] 
[Route("api/prospects")]
public class ProspectController(IMessageBus messageBus, ILogger<ProspectController> logger) : ControllerBase
{
    [HttpGet("subscriptions")]
    public async Task<ActionResult<List<InstitutionDto>>> GetSubscribedInstitutions([FromQuery] Guid userId)
    {
        return await messageBus.InvokeAsync<List<InstitutionDto>>(new GetAllSubscribedInstitutionsQuery(userId));
    }

    [HttpPost("subscriptions/{institutionId:guid}")]
    public async Task<IActionResult> Subscribe(Guid institutionId, [FromBody] Guid userId)
    {
        await messageBus.InvokeAsync(new SubscribeToInstitutionCommand(userId, institutionId));
        return NoContent();
    }

    [HttpDelete("subscriptions/{institutionId:guid}")]
    public async Task<IActionResult> Unsubscribe(Guid institutionId, [FromBody] Guid userId)
    {
        await messageBus.InvokeAsync(new UnsubscribeFromInstitutionCommand(userId, institutionId));
        return NoContent();
    }
}