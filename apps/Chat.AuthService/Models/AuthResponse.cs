namespace Chat.AuthService.Models;

public record AuthResponse(
    string AccessToken,
    DateTime ExpiresAtUtc,
    string UserId,
    string DisplayName
);
