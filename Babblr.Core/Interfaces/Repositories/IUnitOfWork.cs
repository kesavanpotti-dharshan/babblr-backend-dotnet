namespace Babblr.Core.Interfaces.Repositories;

public interface IUnitOfWork : IDisposable
{
    IRoomRepository Rooms { get; }
    IMessageRepository Messages { get; }
    Task<int> SaveChangesAsync();
}