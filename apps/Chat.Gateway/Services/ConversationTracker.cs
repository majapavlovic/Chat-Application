using System.Collections.Concurrent;

namespace Chat.Gateway.Services;

public sealed class ConversationTracker
{
    private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, byte>> _state = new();

    public void Join(string connectionId, string conversationId) =>
        _state.GetOrAdd(connectionId, _ => new ConcurrentDictionary<string, byte>())
              .TryAdd(conversationId, 0);

    public bool HasJoined(string connectionId, string conversationId) =>
        _state.TryGetValue(connectionId, out var convs) && convs.ContainsKey(conversationId);

    public void Leave(string connectionId, string conversationId)
    {
        if (_state.TryGetValue(connectionId, out var convs))
            convs.TryRemove(conversationId, out _);
    }

    public void Disconnect(string connectionId) =>
        _state.TryRemove(connectionId, out _);
}
