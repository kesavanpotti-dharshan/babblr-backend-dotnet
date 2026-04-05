using System.Security.Claims;
using Babblr.Core.DTOs.Room;
using Babblr.Core.Entities;
using Babblr.Core.Enums;
using Babblr.Core.Interfaces.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Babblr.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class RoomsController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;

    public RoomsController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    [HttpPost]
    public async Task<IActionResult> CreateRoom([FromBody] CreateRoomDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value!;

        var room = new Room
        {
            Name = dto.Name,
            Description = dto.Description,
            IsPrivate = dto.IsPrivate,
            CreatedByUserId = userId
        };

        await _unitOfWork.Rooms.AddAsync(room);
        await _unitOfWork.SaveChangesAsync();

        var membership = new RoomMember
        {
            RoomId = room.Id,
            UserId = userId,
            Role = RoomRole.Admin,
            JoinedAt = DateTime.UtcNow
        };

        await _unitOfWork.RoomMembers.AddAsync(membership);
        await _unitOfWork.SaveChangesAsync();

        return Ok(new RoomResponseDto
        {
            Id = room.Id,
            Name = room.Name,
            Description = room.Description,
            IsPrivate = room.IsPrivate,
            CreatedByUserId = room.CreatedByUserId,
            CreatedAt = room.CreatedAt,
            MemberCount = 1
        });
    }

    [HttpGet]
    public async Task<IActionResult> GetMyRooms()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value!;
        var rooms = await _unitOfWork.Rooms.GetRoomsByUserIdAsync(userId);

        var response = rooms.Select(r => new RoomResponseDto
        {
            Id = r.Id,
            Name = r.Name,
            Description = r.Description,
            IsPrivate = r.IsPrivate,
            CreatedByUserId = r.CreatedByUserId,
            CreatedAt = r.CreatedAt,
            MemberCount = r.Members.Count
        });

        return Ok(response);
    }

    [HttpGet("{roomId}")]
    public async Task<IActionResult> GetRoom(Guid roomId)
    {
        var room = await _unitOfWork.Rooms.GetRoomWithMembersAsync(roomId);
        if (room is null)
            return NotFound(new { message = "Room not found." });

        return Ok(new RoomResponseDto
        {
            Id = room.Id,
            Name = room.Name,
            Description = room.Description,
            IsPrivate = room.IsPrivate,
            CreatedByUserId = room.CreatedByUserId,
            CreatedAt = room.CreatedAt,
            MemberCount = room.Members.Count
        });
    }
    [HttpPost("{roomId}/join")]
    public async Task<IActionResult> JoinRoom(Guid roomId)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value!;

        var room = await _unitOfWork.Rooms.GetRoomWithMembersAsync(roomId);
        if (room is null)
            return NotFound(new { message = "Room not found." });

        var existingMembership = await _unitOfWork.RoomMembers
            .GetMembershipAsync(roomId, userId);

        if (existingMembership is not null)
            return BadRequest(new { message = "You are already a member of this room." });

        var membership = new RoomMember
        {
            RoomId = roomId,
            UserId = userId,
            Role = RoomRole.Member,
            JoinedAt = DateTime.UtcNow
        };

        await _unitOfWork.RoomMembers.AddAsync(membership);
        await _unitOfWork.SaveChangesAsync();

        return Ok(new
        {
            roomId,
            userId,
            message = "Successfully joined the room."
        });
    }

    [HttpPost("{roomId}/leave")]
    public async Task<IActionResult> LeaveRoom(Guid roomId)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value!;

        var membership = await _unitOfWork.RoomMembers
            .GetMembershipAsync(roomId, userId);

        if (membership is null)
            return BadRequest(new { message = "You are not a member of this room." });

        _unitOfWork.RoomMembers.Delete(membership);
        await _unitOfWork.SaveChangesAsync();

        return NoContent();
    }
    [HttpGet("discover")]
    public async Task<IActionResult> DiscoverRooms()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value!;

        var allRooms = await _unitOfWork.Rooms.GetPublicRoomsAsync();

        var response = allRooms.Select(r => new RoomResponseDto
        {
            Id = r.Id,
            Name = r.Name,
            Description = r.Description,
            IsPrivate = r.IsPrivate,
            CreatedByUserId = r.CreatedByUserId,
            CreatedAt = r.CreatedAt,
            MemberCount = r.Members.Count
        });

        return Ok(response);
    }
}