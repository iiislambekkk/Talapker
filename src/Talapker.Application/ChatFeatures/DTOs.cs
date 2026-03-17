namespace Talapker.Application.ChatFeatures;

public record ChatRoomDto(
    Guid Id,
    string ProspectId,
    string ProspectName,
    Guid AmbassadorId,
    string AmbassadorName,
    string? AmbassadorAvatarUrl,
    Guid InstitutionId,
    DateTime CreatedAt,
    DateTime? LastMessageAt,
    ChatMessageDto? LastMessage,
    string? ProspectAvatarUrl
);

public record ChatMessageDto(
    Guid Id,
    Guid ChatRoomId,
    string SenderId,
    string SenderName,
    string Text,
    DateTime SentAt,
    bool IsRead
);

public record SendMessageRequest(
    Guid ChatRoomId,
    string SenderId,
    string Text
);

public record GetOrCreateChatRoomRequest(
    string ProspectId,
    Guid AmbassadorId,
    Guid InstitutionId
);