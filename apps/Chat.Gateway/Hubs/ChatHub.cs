using Microsoft.AspNetCore.SignalR;

namespace Chat.Gateway.Hubs;

public class ChatHub : Hub
{
    public async Task JoinRoom(string roomId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, roomId);
        await Clients.Caller.SendAsync("System", $"Joined room {roomId}");
    }

    public async Task SendMessage(string roomId, string message)
    {
        await Clients.Group(roomId).SendAsync("ReceiveMessage", new
        {
            roomId,
            user = Context.ConnectionId,
            message,
            ts = DateTime.UtcNow
        });
    }

    public override async Task OnConnectedAsync()
    {
        await Clients.Caller.SendAsync("System", "Connected");
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await base.OnDisconnectedAsync(exception);
    }
}
