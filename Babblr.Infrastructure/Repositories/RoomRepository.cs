using Babblr.Core.Entities;
using Babblr.Core.Interfaces.Repositories;
using Babblr.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Babblr.Infrastructure.Repositories;

public class RoomRepository : BaseRepository<Room>, IRoomRepository
{
    public RoomRepository(AppDbContext context) : base(context) { }

    public async Task<IEnumerable<Room>> GetRoomsByUserIdAsync(string userId) =>
        await _context.Rooms
            .Where(r => r.Members.Any(m => m.UserId == userId))
            .Include(r => r.Members)
            .ToListAsync();

    public async Task<Room?> GetRoomWithMembersAsync(Guid roomId) =>
        await _context.Rooms
            .Include(r => r.Members)
            .ThenInclude(m => m.User)
            .FirstOrDefaultAsync(r => r.Id == roomId);
}