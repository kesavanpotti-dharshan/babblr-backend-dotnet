using Babblr.Core.Entities;

namespace Babblr.Core.Interfaces.Repositories;

public interface IRoomRepository : IRepository<Room>
{
    Task<IEnumerable<Room>> GetRoomsByUserIdAsync(string userId);
    Task<Room?> GetRoomWithMembersAsync(Guid roomId);
    Task<IEnumerable<Room>> GetPublicRoomsAsync();
}