using Talapker.Infrastructure;
using Talapker.Infrastructure.Data.Institution.InstitutionEntity;

namespace Talapker.Application.InstitutionFeatures.Commands.CreateInstitution;

public record CreateInstitutionCommand(LocalizedText Name, int NationalCode, InstitutionType Type, string FileKey);