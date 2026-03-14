namespace Chat.AuthService.Models;

public record RegisterRequest(
    string DisplayName,
    string Password
);
