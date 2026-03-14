namespace Chat.ConversationService.Domain.Entities;

public class ConversationParticipant
{
    public Guid ConversationId { get; set; }
    public string UserId { get; set; } = default!;
    public DateTime JoinedAtUtc { get; set; }
    public Conversation Conversation { get; set; } = default!;
}
