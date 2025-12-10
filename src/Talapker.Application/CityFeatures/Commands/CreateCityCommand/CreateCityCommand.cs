using Talapker.Infrastructure;

namespace Talapker.Application.CityFeatures.Commands.CreateCityCommand;

public record CreateCityCommand(LocalizedText Name, string LogoKey);