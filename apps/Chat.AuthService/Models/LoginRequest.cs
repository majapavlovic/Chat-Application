namespace Chat.AuthService.Models;

public record LoginRequest(
    string UserId,
    string Password
);
