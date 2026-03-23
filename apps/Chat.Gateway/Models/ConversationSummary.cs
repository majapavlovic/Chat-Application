namespace Chat.Gateway.Models;

public record ConversationSummary(
    string ConversationId,
    int Type,
    string? Name,
    DateTime CreatedAtUtc,
    IReadOnlyList<string> ParticipantIds
);
