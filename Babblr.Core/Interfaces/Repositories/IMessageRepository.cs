using Babblr.Core.Entities;

namespace Babblr.Core.Interfaces.Repositories;

public interface IMessageRepository : IRepository<Message>
{
    Task<IEnumerable<Message>> GetMessagesByRoomIdAsync(
        Guid roomId, int page, int pageSize);

    Task<int> GetMessageCountByRoomIdAsync(Guid roomId);

    Task<IEnumerable<Message>> SearchMessagesAsync(
        Guid roomId, string query);
}