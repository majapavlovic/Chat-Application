using Microsoft.AspNetCore.SignalR;
using Chat.Gateway.Models;

namespace Chat.Gateway.Hubs;

public class ChatHub : Hub
{
    private readonly IHttpClientFactory _httpClientFactory;

    public ChatHub(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task JoinRoom(string roomId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, roomId);
        // await Clients.Caller.SendAsync("System", $"Joined room {roomId}");
    }

    public async Task LeaveRoom(string roomId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomId);
    }
    public async Task SendMessage(string roomId, string message, string clientMessageId)
    {
        var client = _httpClientFactory.CreateClient("messaging");

        var req = new
        {
            roomId,
            senderId = Context.ConnectionId,
            text = message,
            clientMessageId

        };

        var res = await client.PostAsJsonAsync("/api/messages", req);

        if (!res.IsSuccessStatusCode)
        {
            await Clients.Caller.SendAsync("System", $"Messaging service error: {(int)res.StatusCode}");
            return;
        }



        var saved = await res.Content.ReadFromJsonAsync<MessageDto>();

        if (saved is null)
        {
            await Clients.Caller.SendAsync("System", "Messaging service returned empty response.");
            return;
        }

        await Clients.Group(roomId).SendAsync("ReceiveMessage", new
        {
            roomId = saved.RoomId,
            user = saved.SenderId,
            message = saved.Text,
            ts = saved.PersistedAtUtc,
            messageId = saved.MessageId
        });
    }

    public override async Task OnConnectedAsync()
    {
        // await Clients.Caller.SendAsync("System", "Connected");
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await base.OnDisconnectedAsync(exception);
    }
}
