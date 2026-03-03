using Microsoft.AspNetCore.Mvc;
using Talapker.Application;
using Talapker.Application.FacultyFeatures.Command.ChangeFaculty;
using Talapker.Application.FacultyFeatures.Command.CreateFaculty;
using Talapker.Application.FacultyFeatures.DTOs;
using Talapker.Application.FacultyFeatures.Queries;
using Talapker.Application.FacultyFeatures.Queries.GetAllFaculties;
using Talapker.Infrastructure.Data.Institution;
using Wolverine;

namespace Talapker.Web.Controllers;

[ApiController]
[Route("api/faculties")]
public class FacultyController(IMessageBus messageBus) : ControllerBase
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
        var result = await messageBus.InvokeAsync<ApiResponse<Guid>>(command);
        return result.ToActionResult();
    }
    
    [HttpPut]
    [Route("change")]
    public async Task<ActionResult<ApiResponse>> ChangeFacultyAsync(ChangeFacultyCommand command)
    {
        var result = await messageBus.InvokeAsync<ApiResponse>(command);
        return result.ToActionResult();
    }
}