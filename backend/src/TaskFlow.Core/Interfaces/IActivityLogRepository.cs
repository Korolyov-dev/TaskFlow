using TaskFlow.Core.Models;

namespace TaskFlow.Core.Interfaces;

public interface IActivityLogRepository : IRepository<ActivityLog>
{
    Task<IReadOnlyList<ActivityLog>> GetBoardActivitiesAsync(Guid boardId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ActivityLog>> GetUserActivitiesAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ActivityLog>> GetTaskActivitiesAsync(Guid taskId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ActivityLog>> GetRecentActivitiesAsync(int count, CancellationToken cancellationToken = default);
}
