using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Talapker.Application.CityFeatures.Commands.UpdateCityInfoCommand;
using Talapker.Application.CityFeatures.Queries.GetAll;
using Talapker.Infrastructure.AuthZ;
using Talapker.Infrastructure.Data.Institution;
using Wolverine;

namespace Talapker.Web.Controllers;

[ApiController]
[Route("cities")]
public class CityController(IMessageBus messageBus) : ControllerBase
{
    [HttpGet]
    [Route("all")]
    public async Task<ActionResult<List<City>>> GetAllCities()
    {
        return await messageBus.InvokeAsync<List<City>>(new GetAllCitiesQuery());
    }
    
    [HttpPut]
    [Route("update")]
    [Authorize(Policy = AuthorizationPolicies.SystemAdminOnly)]
    public async Task<ActionResult> UpdateCityInfo(UpdateCityInfoCommand command)
    {
        await messageBus.InvokeAsync(command);
        return Ok();
    }
}