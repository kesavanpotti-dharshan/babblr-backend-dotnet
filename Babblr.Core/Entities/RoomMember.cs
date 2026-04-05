using Babblr.Core.Enums;

namespace Babblr.Core.Entities;

public class RoomMember : BaseEntity
{
    public Guid RoomId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public RoomRole Role { get; set; } = RoomRole.Member;
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

    public Room Room { get; set; } = null!;
    public AppUser User { get; set; } = null!;
}