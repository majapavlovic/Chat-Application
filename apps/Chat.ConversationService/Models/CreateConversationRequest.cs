using Chat.ConversationService.Domain.Enums;

namespace Chat.ConversationService.Models;

public record CreateConversationRequest(
    ConversationType Type,
    string? Name,
    List<string> ParticipantIds
);
