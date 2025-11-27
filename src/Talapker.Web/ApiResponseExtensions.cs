// ApiResponseExtensions.cs

using Microsoft.AspNetCore.Mvc;
using Talapker.Application;

namespace Talapker.Web;

public static class ApiResponseExtensions
{
    public static ActionResult ToActionResult<T>(this ApiResponse<T> response)
    {
        if (response.IsSuccess)
        {
            return new OkObjectResult(response);
        }

        return response.ErrorCode switch
        {
            StatusCodes.Status400BadRequest => new BadRequestObjectResult(response),
            StatusCodes.Status401Unauthorized => new UnauthorizedObjectResult(response),
            StatusCodes.Status403Forbidden => new ObjectResult(response) { StatusCode = StatusCodes.Status403Forbidden },
            StatusCodes.Status404NotFound => new NotFoundObjectResult(response),
            StatusCodes.Status409Conflict => new ConflictObjectResult(response),
            StatusCodes.Status422UnprocessableEntity => new UnprocessableEntityObjectResult(response),
            StatusCodes.Status429TooManyRequests => new ObjectResult(response) { StatusCode = StatusCodes.Status429TooManyRequests },
            _ => new BadRequestObjectResult(response)
        };
    }

    public static ActionResult ToActionResult(this ApiResponse response)
    {
        if (response.IsSuccess)
        {
            return new OkObjectResult(response);
        }

        return response.ErrorCode switch
        {
            StatusCodes.Status400BadRequest => new BadRequestObjectResult(response),
            StatusCodes.Status401Unauthorized => new UnauthorizedObjectResult(response),
            StatusCodes.Status403Forbidden => new ObjectResult(response) { StatusCode = StatusCodes.Status403Forbidden },
            StatusCodes.Status404NotFound => new NotFoundObjectResult(response),
            StatusCodes.Status409Conflict => new ConflictObjectResult(response),
            StatusCodes.Status422UnprocessableEntity => new UnprocessableEntityObjectResult(response),
            StatusCodes.Status429TooManyRequests => new ObjectResult(response) { StatusCode = StatusCodes.Status429TooManyRequests },
            _ => new BadRequestObjectResult(response)
        };
    }
}