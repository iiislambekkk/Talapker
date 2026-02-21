using Microsoft.AspNetCore.Mvc;
using Talapker.Application.AI.Talapker;
using Talapker.Application.AI.TranslationAgent;

namespace Talapker.Web.Controllers.AI;

[ApiController]
[Route("api/talapker")]
public class TalapkerController(ITalapkerAgent talapkerAgent) : ControllerBase
{
    [HttpPost]
    [Route("ask")]
    public async Task<ActionResult<TalapkerChatResponse>> TranslateText(TalapkerChatRequest request)
    {
        return await talapkerAgent.AskAsync(request);
    }
}