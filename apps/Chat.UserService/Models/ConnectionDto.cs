using Chat.UserService.Domain.Enums;

namespace Chat.UserService.Models;

public record ConnectionDto(
    string UserId,
    string OtherUserId,
    ConnectionStatus Status,
    string RequestedByUserId,
    DateTime UpdatedAtUtc
);
