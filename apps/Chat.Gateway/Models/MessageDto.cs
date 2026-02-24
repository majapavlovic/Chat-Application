namespace Chat.Gateway.Models;

public record MessageDto(
    string MessageId,
    string RoomId,
    string SenderId,
    string Text,
    DateTime PersistedAtUtc
);
