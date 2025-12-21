using Microsoft.EntityFrameworkCore;
using TaskFlow.Core.Interfaces;
using TaskFlow.Core.Models;
using TaskFlow.Infrastructure.Data;
using TaskFlow.Infrastructure.Entities;

namespace TaskFlow.Infrastructure.Repositories;

public class TaskRepository : BaseRepository<TaskItem, TaskItemEntity>, ITaskRepository
{
    public TaskRepository(ApplicationDbContext context):base(context)
    {
    }

    public Task<IReadOnlyList<TaskItem>> GetTasksByBoardAsync(Guid boardId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<IReadOnlyList<TaskItem>> GetTasksByColumnAsync(Guid columnId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<TaskItem?> GetTaskWithDetailsAsync(Guid taskId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<IReadOnlyList<TaskItem>> GetUserTasksAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task ReorderTasksInColumnAsync(Guid columnId, List<Guid> taskIdsInOrder, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task UpdateTaskOrderAsync(Guid taskId, Guid newColumnId, int newOrder, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    protected override TaskItemEntity MapToEntity(TaskItem model)
    {
        throw new NotImplementedException();
    }

    protected override TaskItem MapToModel(TaskItemEntity entity)
    {
        throw new NotImplementedException();
    }

    protected override void UpdateEntity(TaskItemEntity entity, TaskItem model)
    {
        throw new NotImplementedException();
    }
}
