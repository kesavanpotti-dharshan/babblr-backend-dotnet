namespace Babblr.Core.Entities;

public class Room : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsPrivate { get; set; } = false;
    public string CreatedByUserId { get; set; } = string.Empty;

    public AppUser CreatedBy { get; set; } = null!;
    public ICollection<Message> Messages { get; set; } = new List<Message>();
    public ICollection<RoomMember> Members { get; set; } = new List<RoomMember>();
}