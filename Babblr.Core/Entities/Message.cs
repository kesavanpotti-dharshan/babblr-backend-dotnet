namespace Babblr.Core.Entities;

public class Message : BaseEntity
{
    public string Content { get; set; } = string.Empty;
    public string SenderId { get; set; } = string.Empty;
    public Guid RoomId { get; set; }
    public bool IsEdited { get; set; } = false;
    public bool IsDeleted { get; set; } = false;

    public AppUser Sender { get; set; } = null!;
    public Room Room { get; set; } = null!;
}