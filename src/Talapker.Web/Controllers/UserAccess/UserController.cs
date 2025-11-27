using Microsoft.AspNetCore.Mvc;
using Talapker.Application.UserAccess.Queries;
using Talapker.Application.UserAccess.Queries.GetAllUsers;
using Talapker.Application.UserAccess.Queries.GetUserByIdQuery;
using Talapker.UserAccess.Application.Features.ResetPassword;
using Wolverine;

namespace Talapker.Web.Controllers;

[ApiController]
[Route("user")]
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
}