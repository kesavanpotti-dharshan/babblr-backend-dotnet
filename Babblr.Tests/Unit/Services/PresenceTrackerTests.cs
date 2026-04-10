using Babblr.Infrastructure.Services;
using FluentAssertions;

namespace Babblr.Tests.Unit.Services;

public class PresenceTrackerTests
{
    private readonly InMemoryPresenceTracker _sut = new();

    [Fact]
    public async Task UserConnectedAsync_ShouldMarkUserOnline()
    {
        await _sut.UserConnectedAsync("user1", "conn1");

        var isOnline = await _sut.IsUserOnlineAsync("user1");

        isOnline.Should().BeTrue();
    }

    [Fact]
    public async Task UserDisconnectedAsync_ShouldMarkUserOffline_WhenLastConnection()
    {
        await _sut.UserConnectedAsync("user1", "conn1");
        await _sut.UserDisconnectedAsync("user1", "conn1");

        var isOnline = await _sut.IsUserOnlineAsync("user1");

        isOnline.Should().BeFalse();
    }

    [Fact]
    public async Task UserDisconnectedAsync_ShouldKeepUserOnline_WhenOtherConnectionsExist()
    {
        await _sut.UserConnectedAsync("user1", "conn1");
        await _sut.UserConnectedAsync("user1", "conn2");
        await _sut.UserDisconnectedAsync("user1", "conn1");

        var isOnline = await _sut.IsUserOnlineAsync("user1");

        isOnline.Should().BeTrue();
    }

    [Fact]
    public async Task GetOnlineUsersAsync_ShouldReturnAllOnlineUsers()
    {
        await _sut.UserConnectedAsync("user1", "conn1");
        await _sut.UserConnectedAsync("user2", "conn2");
        await _sut.UserConnectedAsync("user3", "conn3");

        var onlineUsers = await _sut.GetOnlineUsersAsync();

        onlineUsers.Should().Contain(new[] { "user1", "user2", "user3" });
    }

    [Fact]
    public async Task IsUserOnlineAsync_ShouldReturnFalse_WhenUserNeverConnected()
    {
        var isOnline = await _sut.IsUserOnlineAsync("ghost-user");

        isOnline.Should().BeFalse();
    }
}