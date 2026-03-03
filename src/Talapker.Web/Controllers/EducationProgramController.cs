using System.Text.Json;
using Microsoft.Agents.AI;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.AI;
using OpenAI;
using Talapker.Application;
using Talapker.Application.FacultyFeatures.Command;
using Talapker.Application.FacultyFeatures.Command.ChangeEducationProgram;
using Talapker.Application.FacultyFeatures.Command.CreateEducationProgram;
using Talapker.Application.FacultyFeatures.DTOs.Mappers;
using Talapker.Application.FacultyFeatures.Queries;
using Talapker.Application.FacultyFeatures.Queries.GetAllEducationGroups;
using Talapker.Application.FacultyFeatures.Queries.GetEducationProgramsQuery;
using Talapker.Infrastructure;
using Talapker.Infrastructure.Data.Institution;
using Wolverine;

namespace Talapker.Web.Controllers;

public record EducationProgramPreviewRequest(string Text);

public record EducationProgramPreviewResult(
    string Code,
    LocalizedText Name,
    LocalizedText Description,
    LocalizedText WorkPlaces,
    LocalizedText PractiseBases
);

[ApiController]
[Route("api/education-programs")]
public class EducationProgramController(IMessageBus messageBus, IConfiguration configuration) : ControllerBase
{
    [HttpPost]
    [Route("create")]
    public async Task<ActionResult<ApiResponse<Guid>>> CreateEducationProgramAsync(CreateEducationProgramCommand command)
    {
        var result = await messageBus.InvokeAsync<ApiResponse<Guid>>(command);
        return result.ToActionResult();
    }
    
    [HttpPost("preview")]
    public async Task<IActionResult> Preview(EducationProgramPreviewRequest req)
    {
        var instructions = """
                           You are a professional assistant for a university admission platform.
                           The user will provide a raw description of an educational program in any language.

                           Your task:
                           1. Extract the program code (e.g. 6B01501) if present, otherwise return an empty string
                           2. Extract and improve the program name — translate to Kazakh (kk), Russian (ru), and English (en)
                           3. Extract and improve the program description (what the program is about, what students will learn)
                           4. Extract and improve the workplaces (where graduates can work, career opportunities)
                           5. Extract and improve the practise bases (where students can do their internships)
                           6. Translate fields 3-5 to Kazakh (kk), Russian (ru), and English (en)
                           7. Use professional, engaging language appropriate for a university audience
                           8. If information for a specific field is not present in the text — return an empty string for that field. Do NOT invent or assume data.
                           9. Return valid JSON only, no other text
                           """;

        JsonElement schema = AIJsonUtilities.CreateJsonSchema(typeof(EducationProgramPreviewResult));
        ChatOptions chatOptions = new()
        {
            ResponseFormat = ChatResponseFormat.ForJsonSchema(
                schema: schema,
                schemaName: "EducationProgramPreviewResult",
                schemaDescription: "Extracted and translated code, name, description, workplaces and practise bases of an educational program"),
            Instructions = instructions
        };

        AIAgent agent = new OpenAIClient(configuration["OpenAIKey"]!)
            .GetChatClient("gpt-4o-mini")
            .AsIChatClient()
            .CreateAIAgent(new ChatClientAgentOptions { ChatOptions = chatOptions });

        var response = await agent.RunAsync(req.Text);
        var result = response.Deserialize<EducationProgramPreviewResult>(JsonSerializerOptions.Web);

        return Ok(result);
    }
   
    
    [HttpGet]
    [Route("list")]
    public async Task<ActionResult<List<EducationProgramDto>>> GetEducationProgramsAsync(
        [FromQuery] Guid institutionId,
        [FromQuery] Guid? facultyId = null)
    {
        return await messageBus.InvokeAsync<List<EducationProgramDto>>(
            new GetEducationProgramsQuery(institutionId, facultyId)
        );
    }
    
     
    [HttpGet]
    [Route("id/{id:guid}")]
    public async Task<ActionResult<EducationProgramDto?>> GetEducationProgramByIdAsync(Guid id)
    {
        return await messageBus.InvokeAsync<EducationProgramDto?>(new GetEducationProgramByIdQuery(id));
    }
    
    [HttpPut]
    [Route("change")]
    public async Task<ActionResult<ApiResponse>> ChangeEducationProgramAsync(ChangeEducationProgramCommand command)
    {
        var result = await messageBus.InvokeAsync<ApiResponse>(command);
        return result.ToActionResult();
    }
    
    [HttpGet]
    [Route("education-groups")]
    public async Task<ActionResult<ApiResponse>> GEtEducationGroups()
    {
        var result = await messageBus.InvokeAsync<List<EducationGroup>>(new GetAllEducationGroupsQuery());
        return Ok(result);
    }
    
    [HttpDelete]
    [Route("{id}")]
    public async Task<ActionResult<ApiResponse>> DeleteEducationProgramAsync(Guid id)
    {
        var command = new DeleteEducationProgramCommand(id);
        var result = await messageBus.InvokeAsync<ApiResponse>(command);
        return result.ToActionResult();
    }
}