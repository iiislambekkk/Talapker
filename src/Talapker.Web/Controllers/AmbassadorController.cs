using Microsoft.AspNetCore.Mvc;
using Talapker.Application;
using Talapker.Application.AmbassadorFeatures.Commands;
using Talapker.Application.AmbassadorFeatures.DTOs;
using Talapker.Application.AmbassadorFeatures.Queries;
using Wolverine;

namespace Talapker.Web.Controllers;

[ApiController]
[Route("api/ambassadors")]
public class AmbassadorController(IMessageBus messageBus) : ControllerBase
{
    [HttpGet]
    [Route("all")]
    public async Task<ActionResult<List<AmbassadorDto>>> GetAllAmbassadorsAsync([FromQuery] Guid? tenantId)
    {
        return await messageBus.InvokeAsync<List<AmbassadorDto>>(new GetAllAmbassadorsQuery(tenantId));
    }
    
    [HttpPost]
    [Route("invite/accept")]
    public async Task<ActionResult<ApiResponse>> AcceptInviteAsync(AcceptAmbassadorInvitationCommand command)
    {
        var result = await messageBus.InvokeAsync<ApiResponse>(command);
        return result.ToActionResult();
    }
    
    [HttpPut]
    [Route("change-info")]
    public async Task<ActionResult<ApiResponse>> ChangeAmbassadorInfoAsync(ChangeAmbassadorInfoCommand command)
    {
        var result = await messageBus.InvokeAsync<ApiResponse>(command);
        return result.ToActionResult();
    }
    
    [HttpGet]
    [Route("by-user/{userId}")]
    public async Task<ActionResult<AmbassadorDto>> GetAmbassadorByUserIdAsync(Guid userId)
    {
        var result = await messageBus.InvokeAsync<AmbassadorDto>(new GetAmbassadorByUserIdQuery(userId));
        return Ok(result);
    }
}