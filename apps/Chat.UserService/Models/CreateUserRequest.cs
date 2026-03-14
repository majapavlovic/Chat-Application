namespace Chat.UserService.Models;

public record CreateUserRequest(
    string UserId,
    string DisplayName
);
