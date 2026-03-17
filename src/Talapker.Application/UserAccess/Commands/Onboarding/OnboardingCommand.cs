namespace Talapker.Application.UserAccess.Commands.Onboarding;

public record OnboardingCommand(
    string Email,
    string Code,
    string NewPassword,
    string FirstName,
    string LastName
);