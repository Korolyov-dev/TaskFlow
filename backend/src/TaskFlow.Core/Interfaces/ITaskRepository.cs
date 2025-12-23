using TaskFlow.Core.Models;

namespace TaskFlow.Core.Interfaces;

public interface ITaskRepository : IRepository<TaskItem>
{
    Task<TaskItem?> GetTaskWithDetailsAsync(Guid taskId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TaskItem>> GetTasksByColumnAsync(Guid columnId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TaskItem>> GetTasksByBoardAsync(Guid boardId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TaskItem>> GetUserTasksAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TaskItem>> GetAssignedTasksAsync(Guid userId, CancellationToken cancellationToken = default);
    Task UpdateTaskOrderAsync(Guid taskId, Guid newColumnId, int newOrder, CancellationToken cancellationToken = default);
    Task ReorderTasksInColumnAsync(Guid columnId, List<Guid> taskIdsInOrder, CancellationToken cancellationToken = default);
    Task<int> GetNextOrderInColumnAsync(Guid columnId, CancellationToken cancellationToken = default);
}