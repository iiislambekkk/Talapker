using Microsoft.EntityFrameworkCore;
using Talapker.Infrastructure.Data;
using Talapker.Infrastructure.Data.Institution;

namespace Talapker.Application.CityFeatures.Commands.UpdateCityInfoCommand;

public class UpdateCityInfoCommandHandler
{
    public async Task Handle(UpdateCityInfoCommand command, TalapkerDbContext db, CancellationToken ct)
    {
        await db.Cities.Where(c => c.Id == command.Id)
            .ExecuteUpdateAsync(builder => 
                builder
                    .SetProperty(c => c.Name, command.Name)
                    .SetProperty(c => c.LogoKey, command.LogoKey)
                , ct);
    }
}