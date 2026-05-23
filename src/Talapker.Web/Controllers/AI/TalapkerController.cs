using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Talapker.Application.AI.Talapker;
using Talapker.Application.AI.TranslationAgent;
using Talapker.Infrastructure.Data;

namespace Talapker.Web.Controllers.AI;

[ApiController]
[Route("api/talapker")]
public class TalapkerController(ITalapkerAgent talapkerAgent, TalapkerDbContext context) : ControllerBase
{
    [HttpPost]
    [Route("ask")]
    public async Task<ActionResult<TalapkerChatResponse>> TranslateText(TalapkerChatRequest request)
    {
        return await talapkerAgent.AskAsync(request);
    }
    
    [HttpDelete("history")]
    public async Task<ActionResult<ClearChatHistoryResponse>> ClearChatHistory(
        [FromQuery] Guid? userId,
        [FromQuery] Guid institutionId,
        CancellationToken cancellationToken = default)
    {
        var command = new ClearChatHistoryCommand(
            UserId: userId,
            InstitutionId: institutionId
        );

        if (command.UserId is null || command.UserId == Guid.Empty)
            return new ClearChatHistoryResponse(0);

        var deleted = await context.AssistantChatMessage
            .Where(m => m.UserId == command.UserId && m.InstitutionId == command.InstitutionId)
            .ExecuteDeleteAsync(cancellationToken);

        return Ok(new ClearChatHistoryResponse(deleted));
    }
    
    
    public record ClearChatHistoryCommand(
        Guid? UserId,
        Guid InstitutionId
    );

    public record ClearChatHistoryResponse(int DeletedCount);
}