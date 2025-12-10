using Talapker.Infrastructure.Data;
using Talapker.Infrastructure.Data.Institution;

namespace Talapker.Application.CityFeatures.Commands.CreateCityCommand;

public class CreateCityCommandHandler
{
    public async Task Handle(CreateCityCommand command, TalapkerDbContext db, CancellationToken ct)
    {
        var city = new City()
        {
            Id = Guid.NewGuid(),
            Name = command.Name,
            LogoKey = command.LogoKey
        };
        
        await db.Cities.AddAsync(city, ct);
        await db.SaveChangesAsync(ct);
    }
}