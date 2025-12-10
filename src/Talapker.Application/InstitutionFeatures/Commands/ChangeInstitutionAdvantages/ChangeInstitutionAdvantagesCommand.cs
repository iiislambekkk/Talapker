using Talapker.Infrastructure.Data.Institution.InstitutionEntity;

namespace Talapker.Application.InstitutionFeatures.Commands.ChangeInstitutionAdvantages;

public record ChangeInstitutionAdvantagesCommand(Guid Id, List<InstitutionAdvantage> Advantages);