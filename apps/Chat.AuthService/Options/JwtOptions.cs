namespace Chat.AuthService.Options;

public class JwtOptions
{
    public string Issuer { get; set; } = "chat.auth";
    public string Audience { get; set; } = "chat.clients";
    public string SigningKey { get; set; } = default!;
    public int ExpiryMinutes { get; set; } = 15;
    public int RefreshTokenExpiryDays { get; set; } = 7;
}
