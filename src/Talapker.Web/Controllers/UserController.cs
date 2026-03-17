using Microsoft.AspNetCore.Mvc;
using Talapker.Application;
using Talapker.Application.UserAccess.Commands;
using Talapker.Application.UserAccess.Commands.Onboarding;
using Talapker.Application.UserAccess.DTOs;
using Talapker.Application.UserAccess.Queries;
using Talapker.Application.UserAccess.Queries.GetAllUsers;
using Talapker.Application.UserAccess.Queries.GetUserByIdQuery;
using Talapker.Infrastructure.Data.UserAccess;
using Wolverine;

namespace Talapker.Web.Controllers;

[ApiController]
[Route("api/user")]
public class UserController(IMessageBus messageBus) : ControllerBase
{
    [HttpGet("{id}")]
    public async Task<ActionResult<UserDto>> GetUser(string id)
    {
        var user = await messageBus.InvokeAsync<UserDto?>(new GetUserByIdQuery(id));
        
        if (user == null) return NotFound();
        return Ok(user);
    }
    
    [HttpGet("all")]
    public async Task<ActionResult<List<UserDto>>> GetUser()
    {
        var user = await messageBus.InvokeAsync<List<UserDto>>(new GetAllUsersQuery());
        
        return Ok(user);
    }
    
     
    [HttpPost("onboarding")]
    public async Task<ActionResult> OnBoardingUser(OnboardingCommand command)
    {
        await messageBus.InvokeAsync(command);
        
        return Ok();
    }
    
    [HttpPost]
    [Route("invite")]
    public async Task<ActionResult<ApiResponse>> SendInvitationAsync(SendInvitationCommand command)
    {
        var result = await messageBus.InvokeAsync<ApiResponse>(command);
        return result.ToActionResult();
    }
    
    [HttpPost]
    [Route("invite/decilne")]
    public async Task<ActionResult<ApiResponse>> DeclineInvite(DeclineInvitationCommand command)
    {
        var result = await messageBus.InvokeAsync<ApiResponse>(command);
        return result.ToActionResult();
    }
    
    [HttpGet]
    [Route("invitations")]
    public async Task<ActionResult<ApiResponse<List<InvitationDto>>>> GetInvitationsAsync(
        [FromQuery] Guid tenantId,
        [FromQuery] UserRoles.UserRolesEnum? role = null)
    {
        var query = new GetInvitationsQuery(tenantId, role);
        var result = await messageBus.InvokeAsync<ApiResponse<List<InvitationDto>>>(query);
        return result.ToActionResult();
    }
}