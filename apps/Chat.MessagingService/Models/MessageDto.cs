namespace Chat.MessagingService.Models;

public record MessageDto(
    string MessageId,
    string RoomId,
    string SenderId,
    string Text,
    DateTime PersistedAtUtc
);
