using Microsoft.AspNetCore.Mvc;
using Talapker.Infrastructure.Data.Seeding;

namespace Talapker.Web.Controllers;

[ApiController]
[Route("seeding")]
public class SeedingController(SeedData seedData) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult> SeedData(CancellationToken cancellationToken)
    {
        if (!Environment.GetCommandLineArgs().Contains("--seed")) return BadRequest("Seeding not enabled");
        
        await seedData.Seed();
        
        return Ok();
    }
}