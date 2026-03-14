namespace Chat.UserService.Models;

public record UserDto(
    string UserId,
    string DisplayName,
    bool IsOnline,
    DateTime? LastSeenAtUtc,
    DateTime CreatedAtUtc
);
