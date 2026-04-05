namespace Babblr.Core.Interfaces.Repositories;

public interface IUnitOfWork : IDisposable
{
    IRoomRepository Rooms { get; }
    IMessageRepository Messages { get; }
    IRoomMemberRepository RoomMembers { get; }
    Task<int> SaveChangesAsync();
}