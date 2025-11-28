using Talapker.Infrastructure;

namespace Talapker.Application.InstitutionFeatures.Commands.ChangeInstitutionDescription;

public record ChangeInstitutionDescriptionCommand(Guid Id, LocalizedText Description);