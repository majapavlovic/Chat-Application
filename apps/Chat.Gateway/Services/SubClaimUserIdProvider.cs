using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace Chat.Gateway.Services;

public sealed class SubClaimUserIdProvider : IUserIdProvider
{
    public string? GetUserId(HubConnectionContext connection) =>
        connection.User?.FindFirstValue("sub")
        ?? connection.User?.FindFirstValue(ClaimTypes.NameIdentifier);
}
