namespace Chat.MessagingService.Models;
public record CreateMessageRequest(
    string ConversationId,
    string SenderId,
    string Text,
    string ClientMessageId
);