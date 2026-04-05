using System.ComponentModel.DataAnnotations;

namespace Babblr.Core.DTOs.Room;

public class CreateRoomDto
{
    [Required]
    [MinLength(3)]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public bool IsPrivate { get; set; } = false;
}