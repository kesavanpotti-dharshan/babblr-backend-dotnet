using System.ComponentModel.DataAnnotations;

namespace Babblr.Core.DTOs.Message;

public class EditMessageDto
{
    [Required]
    [MinLength(1)]
    [MaxLength(4000)]
    public string Content { get; set; } = string.Empty;
}