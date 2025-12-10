using Talapker.Infrastructure;

namespace Talapker.Application.CityFeatures.Commands.UpdateCityInfoCommand;

public record UpdateCityInfoCommand(Guid Id, LocalizedText Name, string LogoKey);