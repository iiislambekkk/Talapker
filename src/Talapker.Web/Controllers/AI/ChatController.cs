using Microsoft.AspNetCore.Mvc;
using Talapker.Application.AI.Talapker.Queries;
using Wolverine;

namespace Talapker.Web.Controllers.AI;

[ApiController]
[Route("api/chat")]
public class ChatController(IMessageBus messageBus) : ControllerBase
{
    [HttpGet("history")]
    public async Task<ActionResult<GetChatHistoryResponse>> GetChatHistory(
        [FromQuery] Guid? userId,
        [FromQuery] Guid institutionId,
        [FromQuery] DateTime? cursor,
        [FromQuery] int limit = 30,
        [FromQuery] bool older = true,
        CancellationToken cancellationToken = default)
    {
        var query = new GetChatHistoryQuery(
            UserId: userId,
            InstitutionId: institutionId,
            Cursor: cursor,
            Limit: limit,
            Older: older
        );

        var result = await messageBus.InvokeAsync<GetChatHistoryResponse>(query, cancellationToken);
        return Ok(result);
    }
}