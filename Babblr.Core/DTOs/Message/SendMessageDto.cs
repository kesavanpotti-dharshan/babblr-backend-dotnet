using System.ComponentModel.DataAnnotations;

namespace Babblr.Core.DTOs.Message;

public class SendMessageDto
{
    [Required]
    public Guid RoomId { get; set; }

    [Required]
    [MinLength(1)]
    [MaxLength(4000)]
    public string Content { get; set; } = string.Empty;
}