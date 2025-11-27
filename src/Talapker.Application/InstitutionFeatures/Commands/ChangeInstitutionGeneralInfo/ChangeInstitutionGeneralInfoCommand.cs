using Talapker.Infrastructure;
using Talapker.Infrastructure.Data.Institution.InstitutionEntity;

namespace Talapker.Application.InstitutionFeatures.Commands.ChangeInstitutionGeneralInfo;

public record ChangeInstitutionGeneralInfoCommand(Guid Id, LocalizedText Name, int NationalCode, InstitutionType Type, string FileKey);