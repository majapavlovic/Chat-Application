namespace Chat.UserService.Data.Entities;

public class UserEntity
{
    public string Id { get; set; } = default!;
    public string Username { get; set; } = default!;
    public string DisplayName { get; set; } = default!;
    public bool IsOnline { get; set; }
    public DateTime? LastSeenAtUtc { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}
