namespace Talapker.Notifications.Contracts;

public record SendPushCommand(
    Guid UserId,
    string Title,
    string Body,
    string? DeepLink = null,
    string? ImageUrl = null,
    string? Type = "General",
    Dictionary<string, string>? Data = null
);