using Babblr.Core.Entities;
using Babblr.Core.Interfaces.Repositories;
using Babblr.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Babblr.Infrastructure.Repositories;

public class MessageRepository : BaseRepository<Message>, IMessageRepository
{
    public MessageRepository(AppDbContext context) : base(context) { }

    public async Task<IEnumerable<Message>> GetMessagesByRoomIdAsync(
        Guid roomId, int page, int pageSize) =>
        await _context.Messages
            .Where(m => m.RoomId == roomId && !m.IsDeleted)
            .Include(m => m.Sender)
            .OrderByDescending(m => m.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
}