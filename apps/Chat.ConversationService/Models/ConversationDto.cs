using Chat.ConversationService.Domain.Enums;

namespace Chat.ConversationService.Models;

public record ConversationDto(
    string ConversationId,
    ConversationType Type,
    string? Name,
    DateTime CreatedAtUtc,
    List<string> ParticipantIds
);
