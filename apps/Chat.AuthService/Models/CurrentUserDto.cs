namespace Chat.AuthService.Models;

public record CurrentUserDto(
    string UserId,
    string DisplayName
);
