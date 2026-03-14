using Microsoft.AspNetCore.SignalR;
using Chat.Gateway.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace Chat.Gateway.Hubs;

[Authorize]
public class ChatHub : Hub
{
    private readonly IHttpClientFactory _httpClientFactory;

    public ChatHub(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task JoinConversation(string conversationId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, conversationId);
    }

    public async Task LeaveConversation(string conversationId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, conversationId);
    }

    public async Task SendMessage(string conversationId, string message, string clientMessageId)
    {
        var senderId = Context.User?.FindFirstValue("sub")
            ?? Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(senderId))
        {
            await Clients.Caller.SendAsync("System", "Unauthorized sender.");
            return;
        }

        var client = _httpClientFactory.CreateClient("messaging");

        var req = new
        {
            conversationId,
            senderId,
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

        await Clients.Group(conversationId).SendAsync("ReceiveMessage", new
        {
            conversationId = saved.ConversationId,
            senderId = saved.SenderId,
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
