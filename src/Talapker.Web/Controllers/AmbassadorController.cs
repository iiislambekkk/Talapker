using Microsoft.AspNetCore.Mvc;
using Talapker.Application;
using Talapker.Application.AmbassadorFeatures.Commands.ChangeAmbassadorInfo;
using Talapker.Application.AmbassadorFeatures.Commands.Queries;
using Talapker.Application.AmbassadorFeatures.DTOs;
using Talapker.Application.AmbassadorFeatures.InviteAmbassador;
using Talapker.Application.AmbassadorFeatures.Queries;
using Talapker.Infrastructure.Data.Institution;
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
    [Route("invite")]
    public async Task<ActionResult<ApiResponse>> InviteAmbassadorAsync(InviteAmbassadorCommand command)
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