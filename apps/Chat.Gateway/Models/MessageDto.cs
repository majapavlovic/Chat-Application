namespace Chat.Gateway.Models;

public record MessageDto(
    string MessageId,
    string ConversationId,
    string SenderId,
    string Text,
    DateTime PersistedAtUtc
);
