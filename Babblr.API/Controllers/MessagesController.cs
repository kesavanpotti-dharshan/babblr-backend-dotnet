using System.Security.Claims;
using Babblr.Core.DTOs.Message;
using Babblr.Core.Interfaces.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Babblr.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MessagesController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;

    public MessagesController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    [HttpGet("room/{roomId}")]
    public async Task<IActionResult> GetMessages(
    Guid roomId,
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 20)
    {
        if (pageSize > 50) pageSize = 50;

        var messages = await _unitOfWork.Messages
            .GetMessagesByRoomIdAsync(roomId, page, pageSize);

        var totalCount = await _unitOfWork.Messages
            .GetMessageCountByRoomIdAsync(roomId);

        var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

        var response = messages.Select(m => new MessageResponseDto
        {
            MessageId = m.Id,
            Content = m.Content,
            SenderId = m.SenderId,
            SenderDisplayName = m.Sender.DisplayName,
            RoomId = m.RoomId,
            SentAt = m.CreatedAt,
            IsEdited = m.IsEdited
        });

        return Ok(new
        {
            messages = response,
            pagination = new
            {
                page,
                pageSize,
                totalCount,
                totalPages,
                hasMore = page < totalPages
            }
        });
    }

    [HttpPut("{messageId}")]
    public async Task<IActionResult> EditMessage(
        Guid messageId,
        [FromBody] EditMessageDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value!;
        var message = await _unitOfWork.Messages.GetByIdAsync(messageId);

        if (message is null)
            return NotFound(new { message = "Message not found." });

        if (message.SenderId != userId)
            return Forbid();

        message.Content = dto.Content;
        message.IsEdited = true;
        message.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Messages.Update(message);
        await _unitOfWork.SaveChangesAsync();

        return Ok(new { messageId = message.Id, content = message.Content, isEdited = true });
    }

    [HttpDelete("{messageId}")]
    public async Task<IActionResult> DeleteMessage(Guid messageId)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value!;
        var message = await _unitOfWork.Messages.GetByIdAsync(messageId);

        if (message is null)
            return NotFound(new { message = "Message not found." });

        if (message.SenderId != userId)
            return Forbid();

        message.IsDeleted = true;
        message.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Messages.Update(message);
        await _unitOfWork.SaveChangesAsync();

        return NoContent();
    }

    [HttpGet("room/{roomId}/search")]
    public async Task<IActionResult> SearchMessages(
        Guid roomId,
        [FromQuery] string q)
    {
        if (string.IsNullOrWhiteSpace(q))
            return BadRequest(new { message = "Search query cannot be empty." });

        var results = await _unitOfWork.Messages
            .SearchMessagesAsync(roomId, q);

        var response = results.Select(m => new MessageResponseDto
        {
            MessageId = m.Id,
            Content = m.Content,
            SenderId = m.SenderId,
            SenderDisplayName = m.Sender.DisplayName,
            RoomId = m.RoomId,
            SentAt = m.CreatedAt,
            IsEdited = m.IsEdited
        });

        return Ok(response);
    }
}