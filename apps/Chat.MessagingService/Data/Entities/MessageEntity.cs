namespace Chat.MessagingService.Data.Entities;

public class MessageEntity
{
    public Guid Id { get; set; }
    public string RoomId { get; set; } = default!;
    public string SenderId { get; set; } = default!;
    public string Text { get; set; } = default!;
    public DateTime PersistedAtUtc { get; set; }
    public string ClientMessageId { get; set; } = default!;
}