using Chat.UserService.Domain.Enums;

namespace Chat.UserService.Models;

public record UpsertConnectionRequest(
    string UserId,
    string OtherUserId,
    ConnectionStatus Status
);
