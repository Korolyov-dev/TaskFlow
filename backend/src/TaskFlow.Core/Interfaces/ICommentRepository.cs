using TaskFlow.Core.Interfaces;
using TaskFlow.Core.Models;

public interface ICommentRepository : IRepository<Comment>
{
    Task<IReadOnlyList<Comment>> GetTaskCommentsAsync(Guid taskId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Comment>> GetUserCommentsAsync(Guid userId, CancellationToken cancellationToken = default);
}