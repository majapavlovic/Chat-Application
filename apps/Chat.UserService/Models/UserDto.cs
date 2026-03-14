namespace Chat.UserService.Models;

public record UserDto(
    string UserId,
    string Username,
    string DisplayName,
    bool IsOnline,
    DateTime? LastSeenAtUtc,
    DateTime CreatedAtUtc
);
