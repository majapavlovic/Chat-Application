namespace Chat.AuthService.Data.Entities;

public class AuthAccountEntity
{
    public Guid Id { get; set; }
    public string UserId { get; set; } = default!;
    public string Username { get; set; } = default!;
    public string DisplayName { get; set; } = default!;
    public string PasswordHash { get; set; } = default!;
    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
}
