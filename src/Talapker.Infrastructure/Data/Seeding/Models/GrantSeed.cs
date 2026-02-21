using Talapker.Institutions.Infrastructure.Data.Seeding.Models;

namespace Talapker.Infrastructure.Data.Seeding.Models;

public class GrantSeed
{
    public GrantCompetitionStatisticsSeed? General { get; set; }
    public GrantCompetitionStatisticsSeed? Rural { get; set; }
}
