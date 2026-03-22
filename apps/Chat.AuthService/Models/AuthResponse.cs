namespace Chat.AuthService.Models;

public record AuthResponse(
    string AccessToken,
    DateTime ExpiresAtUtc,
    string UserId,
    string Username,
    string DisplayName,
    string RefreshToken,
    DateTime RefreshTokenExpiresAtUtc
);
