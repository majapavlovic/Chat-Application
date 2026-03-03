namespace Chat.MessagingService.Models;
public record CreateMessageRequest(
    string RoomId,
    string SenderId,
    string Text,
    string ClientMessageId
);