using System.Security.Claims;
using Babblr.Core.Entities;
using Babblr.Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Babblr.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly UserManager<AppUser> _userManager;
    private readonly IPresenceTracker _presenceTracker;

    public UsersController(
        UserManager<AppUser> userManager,
        IPresenceTracker presenceTracker)
    {
        _userManager = userManager;
        _presenceTracker = presenceTracker;
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetProfile()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value!;
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null) return NotFound();

        return Ok(new
        {
            userId = user.Id,
            email = user.Email,
            displayName = user.DisplayName,
            avatarUrl = user.AvatarUrl,
            createdAt = user.CreatedAt,
            isOnline = true
        });
    }

    [HttpPut("me")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto dto)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value!;
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null) return NotFound();

        user.DisplayName = dto.DisplayName ?? user.DisplayName;
        user.AvatarUrl = dto.AvatarUrl ?? user.AvatarUrl;

        await _userManager.UpdateAsync(user);

        return Ok(new
        {
            userId = user.Id,
            displayName = user.DisplayName,
            avatarUrl = user.AvatarUrl
        });
    }

    [HttpGet("online")]
    public async Task<IActionResult> GetOnlineUsers()
    {
        var onlineUsers = await _presenceTracker.GetOnlineUsersAsync();
        return Ok(new { onlineUsers, count = onlineUsers.Count() });
    }
}

public class UpdateProfileDto
{
    public string? DisplayName { get; set; }
    public string? AvatarUrl { get; set; }
}