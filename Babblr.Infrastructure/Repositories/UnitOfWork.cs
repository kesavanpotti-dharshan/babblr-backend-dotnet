using Babblr.Core.Interfaces.Repositories;
using Babblr.Infrastructure.Data;

namespace Babblr.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;

    public IRoomRepository Rooms { get; }
    public IMessageRepository Messages { get; }
    public IRoomMemberRepository RoomMembers { get; }

    public UnitOfWork(AppDbContext context)
    {
        _context = context;
        Rooms = new RoomRepository(context);
        Messages = new MessageRepository(context);
        RoomMembers = new RoomMemberRepository(context);
    }

    public async Task<int> SaveChangesAsync() =>
        await _context.SaveChangesAsync();

    public void Dispose() =>
        _context.Dispose();
}