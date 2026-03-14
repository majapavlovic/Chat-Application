namespace Chat.AuthService.Models;

public record RegisterRequest(
    string Username,
    string DisplayName,
    string Password
);
