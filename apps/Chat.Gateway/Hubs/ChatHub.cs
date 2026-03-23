using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;
using Chat.Gateway.Models;
using Chat.Gateway.Services;

namespace Chat.Gateway.Hubs;

[Authorize]
public class ChatHub : Hub
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ConversationTracker _tracker;

    public ChatHub(IHttpClientFactory httpClientFactory, ConversationTracker tracker)
    {
        _httpClientFactory = httpClientFactory;
        _tracker = tracker;
    }

    private string GetCallerId() =>
        Context.UserIdentifier
        ?? throw new HubException("Unauthenticated.");

    public async Task JoinConversation(string conversationId)
    {
        var userId = GetCallerId();

        var client = _httpClientFactory.CreateClient("conversation");
        var res = await client.GetAsync($"/api/conversations/{Uri.EscapeDataString(conversationId)}");

        if (res.StatusCode == System.Net.HttpStatusCode.NotFound)
            throw new HubException("Conversation not found.");

        if (!res.IsSuccessStatusCode)
            throw new HubException($"You can't send messages to this conversation ({(int)res.StatusCode}).");

        var conv = await res.Content.ReadFromJsonAsync<ConversationSummary>();

        if (conv is null || !conv.ParticipantIds.Contains(userId))
            throw new HubException("You are not a participant of this conversation.");

        _tracker.Join(Context.ConnectionId, conversationId);
        await Groups.AddToGroupAsync(Context.ConnectionId, conversationId);
    }

    public async Task LeaveConversation(string conversationId)
    {
        _tracker.Leave(Context.ConnectionId, conversationId);
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, conversationId);
    }

    public async Task SendMessage(string conversationId, string message, string clientMessageId)
    {
        var senderId = GetCallerId();

        if (!_tracker.HasJoined(Context.ConnectionId, conversationId))
            throw new HubException("You have not joined this conversation.");

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
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        _tracker.Disconnect(Context.ConnectionId);
        await base.OnDisconnectedAsync(exception);
    }
}
