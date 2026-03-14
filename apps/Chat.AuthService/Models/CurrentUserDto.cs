namespace Chat.AuthService.Models;

public record CurrentUserDto(
    string UserId,
    string Username,
    string DisplayName
);
