using Chat.UserService.Domain.Enums;

namespace Chat.UserService.Data.Entities;

public class UserConnectionEntity
{
    public string UserAId { get; set; } = default!;
    public string UserBId { get; set; } = default!;
    public string RequestedByUserId { get; set; } = default!;
    public ConnectionStatus Status { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
}
