namespace Chat.AuthService.Models;

public record LoginRequest(
    string Username,
    string Password
);
