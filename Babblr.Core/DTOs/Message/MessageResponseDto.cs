namespace Babblr.Core.DTOs.Message;

public class MessageResponseDto
{
    public Guid MessageId { get; set; }
    public string Content { get; set; } = string.Empty;
    public string SenderId { get; set; } = string.Empty;
    public string SenderDisplayName { get; set; } = string.Empty;
    public Guid RoomId { get; set; }
    public DateTime SentAt { get; set; }
    public bool IsEdited { get; set; }
}