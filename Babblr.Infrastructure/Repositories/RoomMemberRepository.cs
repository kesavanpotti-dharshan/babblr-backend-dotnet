using Babblr.Core.Entities;
using Babblr.Core.Interfaces.Repositories;
using Babblr.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Babblr.Infrastructure.Repositories;

public class RoomMemberRepository : BaseRepository<RoomMember>, IRoomMemberRepository
{
    public RoomMemberRepository(AppDbContext context) : base(context) { }

    public async Task<RoomMember?> GetMembershipAsync(Guid roomId, string userId) =>
        await _context.RoomMembers
            .FirstOrDefaultAsync(rm => rm.RoomId == roomId && rm.UserId == userId);

    public async Task<IEnumerable<RoomMember>> GetMembersByRoomIdAsync(Guid roomId) =>
        await _context.RoomMembers
            .Where(rm => rm.RoomId == roomId)
            .Include(rm => rm.User)
            .ToListAsync();
}