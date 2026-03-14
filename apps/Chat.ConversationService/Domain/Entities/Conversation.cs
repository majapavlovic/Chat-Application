using Chat.ConversationService.Domain.Enums;

namespace Chat.ConversationService.Domain.Entities;

public class Conversation
{
    public Guid Id { get; set; }
    public ConversationType Type { get; set; }
    public string? Name { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public ICollection<ConversationParticipant> Participants { get; set; } = new List<ConversationParticipant>();
}
