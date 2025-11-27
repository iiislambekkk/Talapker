using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Talapker.Application.File.GetPresignUrl;
using Wolverine;

namespace Talapker.Web.Controllers;

[Authorize(AuthenticationSchemes="OpenIddict.Validation.AspNetCore")]
[Route("/file")]
public class FileController(IMessageBus messageBus)
{
    [HttpPost("get-presigned-url")]
    public async Task<GetPresignUrlResponse> GetPresignedUrl(GetPresignUrlCommand command)
    {
        return await messageBus.InvokeAsync<GetPresignUrlResponse>(command);
    }
}