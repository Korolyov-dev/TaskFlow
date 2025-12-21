using TaskFlow.Core.Models;

namespace TaskFlow.Core.Interfaces;
public interface IBoardRepository : IRepository<Board>
{
    Task<Board?> GetBoardWithDetailsAsync(Guid boardId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Board>> GetUserBoardsAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Board>> GetUserFavoriteBoardsAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<bool> IsUserBoardMemberAsync(Guid boardId, Guid userId, CancellationToken cancellationToken = default);
    Task<bool> IsUserBoardOwnerAsync(Guid boardId, Guid userId, CancellationToken cancellationToken = default);
    Task AddMemberAsync(Guid boardId, Guid userId, BoardRole role, CancellationToken cancellationToken = default);
    Task RemoveMemberAsync(Guid boardId, Guid userId, CancellationToken cancellationToken = default);
}
