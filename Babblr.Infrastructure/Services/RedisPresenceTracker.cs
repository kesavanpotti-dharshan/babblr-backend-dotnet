using Babblr.Core.Interfaces.Services;

namespace Babblr.Infrastructure.Services;

/// <summary>
/// Redis-backed presence tracker. Swap this in place of InMemoryPresenceTracker
/// by changing the DI registration in Program.cs to:
/// builder.Services.AddSingleton&lt;IPresenceTracker, RedisPresenceTracker&gt;()
/// Requires StackExchange.Redis package and a Redis connection string.
/// </summary>
public class RedisPresenceTracker : IPresenceTracker
{
    // TODO: inject IConnectionMultiplexer from StackExchange.Redis
    // private readonly IConnectionMultiplexer _redis;

    public Task UserConnectedAsync(string userId, string connectionId) =>
        throw new NotImplementedException("Wire up StackExchange.Redis first.");

    public Task UserDisconnectedAsync(string userId, string connectionId) =>
        throw new NotImplementedException("Wire up StackExchange.Redis first.");

    public Task<bool> IsUserOnlineAsync(string userId) =>
        throw new NotImplementedException("Wire up StackExchange.Redis first.");

    public Task<IEnumerable<string>> GetOnlineUsersAsync() =>
        throw new NotImplementedException("Wire up StackExchange.Redis first.");

    public Task<IEnumerable<string>> GetConnectionsForUserAsync(string userId) =>
        throw new NotImplementedException("Wire up StackExchange.Redis first.");
}