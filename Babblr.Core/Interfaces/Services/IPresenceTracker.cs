namespace Babblr.Core.Interfaces.Services;

public interface IPresenceTracker
{
    Task UserConnectedAsync(string userId, string connectionId);
    Task UserDisconnectedAsync(string userId, string connectionId);
    Task<bool> IsUserOnlineAsync(string userId);
    Task<IEnumerable<string>> GetOnlineUsersAsync();
    Task<IEnumerable<string>> GetConnectionsForUserAsync(string userId);
}