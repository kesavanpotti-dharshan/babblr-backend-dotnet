using System.Collections.Concurrent;
using Babblr.Core.Interfaces.Services;

namespace Babblr.Infrastructure.Services;

public class InMemoryPresenceTracker : IPresenceTracker
{
    // userId -> set of connectionIds (one user can have multiple tabs open)
    private static readonly ConcurrentDictionary<string, HashSet<string>> _connections
        = new();
    private static readonly object _lock = new();

    public Task UserConnectedAsync(string userId, string connectionId)
    {
        lock (_lock)
        {
            if (!_connections.ContainsKey(userId))
                _connections[userId] = new HashSet<string>();

            _connections[userId].Add(connectionId);
        }
        return Task.CompletedTask;
    }

    public Task UserDisconnectedAsync(string userId, string connectionId)
    {
        lock (_lock)
        {
            if (_connections.ContainsKey(userId))
            {
                _connections[userId].Remove(connectionId);
                if (_connections[userId].Count == 0)
                    _connections.TryRemove(userId, out _);
            }
        }
        return Task.CompletedTask;
    }

    public Task<bool> IsUserOnlineAsync(string userId) =>
        Task.FromResult(_connections.ContainsKey(userId));

    public Task<IEnumerable<string>> GetOnlineUsersAsync() =>
        Task.FromResult<IEnumerable<string>>(_connections.Keys.ToList());

    public Task<IEnumerable<string>> GetConnectionsForUserAsync(string userId)
    {
        if (_connections.TryGetValue(userId, out var connections))
            return Task.FromResult<IEnumerable<string>>(connections.ToList());

        return Task.FromResult<IEnumerable<string>>(Enumerable.Empty<string>());
    }
}