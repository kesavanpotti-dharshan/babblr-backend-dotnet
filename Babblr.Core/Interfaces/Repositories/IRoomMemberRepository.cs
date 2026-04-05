using Babblr.Core.Entities;

namespace Babblr.Core.Interfaces.Repositories;

public interface IRoomMemberRepository : IRepository<RoomMember>
{
    Task<RoomMember?> GetMembershipAsync(Guid roomId, string userId);
    Task<IEnumerable<RoomMember>> GetMembersByRoomIdAsync(Guid roomId);
}