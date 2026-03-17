using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Talapker.Application;
using Talapker.Application.FacultyFeatures.Command;
using Talapker.Application.FacultyFeatures.DTOs;
using Talapker.Application.FacultyFeatures.Queries;
using Talapker.Application.FacultyFeatures.Queries.GetAllFaculties;
using Talapker.Infrastructure.AuthZ;
using Talapker.Infrastructure.Data.Institution;
using Wolverine;

namespace Talapker.Web.Controllers;

[ApiController]
[Route("api/faculties")]
public class FacultyController(IMessageBus messageBus, ILogger<FacultyController> logger) : ControllerBase
{
    [HttpGet]
    [Route("all")]
    public async Task<ActionResult<List<FacultyDto>>> GetAllFacultiesAsync([FromQuery] Guid institutionId)
    {
        return await messageBus.InvokeAsync<List<FacultyDto>>(new GetAllFacultiesQuery(institutionId));
    }
    
  
    
    [HttpGet("with-programs")]
    public async Task<ActionResult<ApiResponse<List<FacultyWithProgramsDto>>>> GetAllFacultiesWithPrograms([FromQuery] Guid institutionId)
    {
        var query = new GetAllFacultiesWithProgramsQuery(institutionId);
        var result = await messageBus.InvokeAsync<ApiResponse<List<FacultyWithProgramsDto>>>(query);
        return result.ToActionResult();
    }
    
    [HttpPost]
    [Route("create")]
    public async Task<ActionResult<ApiResponse<Guid>>> CreateFacultyAsync(CreateFacultyCommand command)
    {
        logger.LogInformation("Creating faculty {@Command}", command);

        var result = await messageBus.InvokeAsync<ApiResponse<Guid>>(command);

        if (result.IsSuccess)
            logger.LogInformation("Faculty created successfully {FacultyId}", result.Data);
        else
            logger.LogWarning("Faculty creation failed {@Errors}", result.Error);

        return result.ToActionResult();
    }
    
    [HttpPut]
    [Route("{tenantId}/change")]
    [TenantAuthorize(Policy = AuthorizationPolicies.TenantAdmins)]
    public async Task<ActionResult<ApiResponse>> ChangeFacultyAsync(
        [FromRoute] Guid tenantId,
        ChangeFacultyCommand command)
    {
        logger.LogInformation("Changing faculty {@Command}", command);
        
        var result = await messageBus.InvokeAsync<ApiResponse>(command);
        
        if (result.IsSuccess)
            logger.LogInformation("Faculty changed successfully {FacultyId}, by userId: {userId}", command.Id, User.FindFirstValue("sub"));
        else
            logger.LogWarning("Faculty changing by userId: {userId} failed {@Errors}", User.FindFirstValue("sub"), result.Error);
        
        
        return result.ToActionResult();
    }
}