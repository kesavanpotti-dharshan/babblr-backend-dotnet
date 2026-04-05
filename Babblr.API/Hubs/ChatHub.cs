using System.Security.Claims;
using Babblr.Core.DTOs.Message;
using Babblr.Core.Entities;
using Babblr.Core.Interfaces.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Babblr.API.Hubs;

[Authorize]
public class ChatHub : Hub
{
    private readonly IUnitOfWork _unitOfWork;

    public ChatHub(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public override async Task OnConnectedAsync()
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId is not null)
            await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId}");

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId is not null)
            await Clients.Others.SendAsync("UserOffline", userId);

        await base.OnDisconnectedAsync(exception);
    }

    public async Task JoinRoom(string roomId)
    {
        var userId = GetUserId();
        if (userId is null) return;

        await Groups.AddToGroupAsync(Context.ConnectionId, roomId);
        await Clients.Group(roomId).SendAsync("UserJoined", new
        {
            UserId = userId,
            RoomId = roomId,
            JoinedAt = DateTime.UtcNow
        });
    }

    public async Task LeaveRoom(string roomId)
    {
        var userId = GetUserId();
        if (userId is null) return;

        await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomId);
        await Clients.Group(roomId).SendAsync("UserLeft", new
        {
            UserId = userId,
            RoomId = roomId
        });
    }

    public async Task SendMessage(SendMessageDto dto)
    {
        var userId = GetUserId();
        if (userId is null)
        {
            await Clients.Caller.SendAsync("Error", "Unauthorized");
            return;
        }

        var message = new Message
        {
            Content = dto.Content,
            RoomId = dto.RoomId,
            SenderId = userId
        };

        await _unitOfWork.Messages.AddAsync(message);
        await _unitOfWork.SaveChangesAsync();

        await Clients.Group(dto.RoomId.ToString()).SendAsync("ReceiveMessage", new
        {
            MessageId = message.Id,
            Content = message.Content,
            SenderId = userId,
            RoomId = dto.RoomId,
            SentAt = message.CreatedAt
        });
    }

    public async Task TypingStarted(string roomId)
    {
        var userId = GetUserId();
        if (userId is null) return;

        await Clients.OthersInGroup(roomId).SendAsync("UserTyping", new
        {
            UserId = userId,
            RoomId = roomId
        });
    }

    public async Task TypingStopped(string roomId)
    {
        var userId = GetUserId();
        if (userId is null) return;

        await Clients.OthersInGroup(roomId).SendAsync("UserStoppedTyping", new
        {
            UserId = userId,
            RoomId = roomId
        });
    }

    private string? GetUserId() =>
        Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

    public async Task EditMessage(Guid messageId, string roomId, string newContent)
    {
        var userId = GetUserId();
        if (userId is null) return;

        await Clients.Group(roomId).SendAsync("MessageEdited", new
        {
            MessageId = messageId,
            NewContent = newContent,
            EditedAt = DateTime.UtcNow
        });
    }

    public async Task DeleteMessage(Guid messageId, string roomId)
    {
        var userId = GetUserId();
        if (userId is null) return;

        await Clients.Group(roomId).SendAsync("MessageDeleted", new
        {
            MessageId = messageId,
            DeletedAt = DateTime.UtcNow
        });
    }
}