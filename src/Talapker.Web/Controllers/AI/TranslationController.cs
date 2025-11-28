using Microsoft.AspNetCore.Mvc;
using Talapker.Infrastructure.AI.TranslationAgent;

namespace Talapker.Web.Controllers.AI;

[ApiController]
public class TranslationController(ITranslationService translationService) : ControllerBase
{
    [HttpPost]
    [Route("translate")]
    public async Task<ActionResult<TranslationResult>> TranslateText(TranslationRequest request)
    {
        return await translationService.TranslateToAllLanguagesAsync(request.Text, request.ShouldImprove);
    }
}