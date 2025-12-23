using Microsoft.EntityFrameworkCore;
using TaskFlow.Core.Interfaces;
using TaskFlow.Core.Models;
using TaskFlow.Infrastructure.Data;
using TaskFlow.Infrastructure.Entities;

namespace TaskFlow.Infrastructure.Repositories;

public class TaskLabelRepository : BaseRepository<TaskLabel, TaskLabelEntity> , ITaskLabelRepository
{
    public TaskLabelRepository(ApplicationDbContext context) : base(context)
    {
    }

    // Переопределяем GetByIdAsync для составного ключа
    public override async Task<TaskLabel?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // Для TaskLabel не работает стандартный GetByIdAsync, так как составной ключ
        throw new NotSupportedException("Use GetByTaskAndLabelAsync instead for TaskLabel");
    }

    // Специфичные методы
    public async Task<TaskLabel?> GetByTaskAndLabelAsync(Guid taskId, Guid labelId, CancellationToken cancellationToken = default)
    {
        var entity = await _dbSet
            .AsNoTracking()
            .FirstOrDefaultAsync(tl => tl.TaskId == taskId && tl.LabelId == labelId, cancellationToken);

        return entity != null ? MapToModel(entity) : null;
    }

    public async Task<IReadOnlyList<TaskLabel>> GetByTaskAsync(Guid taskId, CancellationToken cancellationToken = default)
    {
        var entities = await _dbSet
            .AsNoTracking()
            .Where(tl => tl.TaskId == taskId)
            .OrderBy(tl => tl.AssignedAt)
            .ToListAsync(cancellationToken);

        return entities.Select(MapToModel).ToList();
    }

    public async Task<IReadOnlyList<TaskLabel>> GetByLabelAsync(Guid labelId, CancellationToken cancellationToken = default)
    {
        var entities = await _dbSet
            .AsNoTracking()
            .Where(tl => tl.LabelId == labelId)
            .OrderBy(tl => tl.AssignedAt)
            .ToListAsync(cancellationToken);

        return entities.Select(MapToModel).ToList();
    }

    public async Task RemoveByTaskAndLabelAsync(Guid taskId, Guid labelId, CancellationToken cancellationToken = default)
    {
        var entity = await _dbSet
            .FirstOrDefaultAsync(tl => tl.TaskId == taskId && tl.LabelId == labelId, cancellationToken);

        if (entity != null)
        {
            _dbSet.Remove(entity);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task RemoveAllForTaskAsync(Guid taskId, CancellationToken cancellationToken = default)
    {
        var entities = await _dbSet
            .Where(tl => tl.TaskId == taskId)
            .ToListAsync(cancellationToken);

        if (entities.Any())
        {
            _dbSet.RemoveRange(entities);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task RemoveAllForLabelAsync(Guid labelId, CancellationToken cancellationToken = default)
    {
        var entities = await _dbSet
            .Where(tl => tl.LabelId == labelId)
            .ToListAsync(cancellationToken);

        if (entities.Any())
        {
            _dbSet.RemoveRange(entities);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<bool> ExistsAsync(Guid taskId, Guid labelId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AnyAsync(tl => tl.TaskId == taskId && tl.LabelId == labelId, cancellationToken);
    }

    public async Task<int> CountLabelsForTaskAsync(Guid taskId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .CountAsync(tl => tl.TaskId == taskId, cancellationToken);
    }

    public async Task<int> CountTasksForLabelAsync(Guid labelId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .CountAsync(tl => tl.LabelId == labelId, cancellationToken);
    }

    public async Task AddMultipleAsync(Guid taskId, IEnumerable<Guid> labelIds, CancellationToken cancellationToken = default)
    {
        var existingLabelIds = await _dbSet
            .Where(tl => tl.TaskId == taskId)
            .Select(tl => tl.LabelId)
            .ToListAsync(cancellationToken);

        var newLabelIds = labelIds.Except(existingLabelIds).ToList();

        if (!newLabelIds.Any()) return;

        var entities = newLabelIds.Select(labelId => new TaskLabelEntity
        {
            TaskId = taskId,
            LabelId = labelId,
            AssignedAt = DateTime.UtcNow
        }).ToList();

        await _dbSet.AddRangeAsync(entities, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task RemoveMultipleAsync(Guid taskId, IEnumerable<Guid> labelIds, CancellationToken cancellationToken = default)
    {
        var entities = await _dbSet
            .Where(tl => tl.TaskId == taskId && labelIds.Contains(tl.LabelId))
            .ToListAsync(cancellationToken);

        if (entities.Any())
        {
            _dbSet.RemoveRange(entities);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task ReplaceTaskLabelsAsync(Guid taskId, IEnumerable<Guid> newLabelIds, CancellationToken cancellationToken = default)
    {
        using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            // Удаляем старые связи
            await RemoveAllForTaskAsync(taskId, cancellationToken);

            // Добавляем новые связи
            await AddMultipleAsync(taskId, newLabelIds, cancellationToken);

            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    // Переопределяем базовые методы, которые не работают для составного ключа
    public override Task DeleteAsync(TaskLabel model, CancellationToken cancellationToken = default)
    {
        return RemoveByTaskAndLabelAsync(model.TaskId, model.LabelId, cancellationToken);
    }

    public override async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // Для TaskLabel не работает стандартный ExistsAsync
        throw new NotSupportedException("Use ExistsAsync(taskId, labelId) instead for TaskLabel");
    }

    public override Task<TaskLabel> AddAsync(TaskLabel model, CancellationToken cancellationToken = default)
    {
        return AddAsync(model.TaskId, model.LabelId, cancellationToken);
    }

    public override Task UpdateAsync(TaskLabel model, CancellationToken cancellationToken = default)
    {
        // TaskLabel обычно не обновляется, только создается/удаляется
        // Если нужно обновить AssignedAt, можно сделать так:
        return Task.CompletedTask;
    }

    // Вспомогательный метод
    public async Task<TaskLabel> AddAsync(Guid taskId, Guid labelId, CancellationToken cancellationToken = default)
    {
        if (await ExistsAsync(taskId, labelId, cancellationToken))
        {
            throw new InvalidOperationException($"TaskLabel already exists for Task {taskId} and Label {labelId}");
        }

        var taskLabel = TaskLabel.Create(taskId, labelId);
        var entity = MapToEntity(taskLabel);

        await _dbSet.AddAsync(entity, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return MapToModel(entity);
    }

    // Маппинг методы
    protected override TaskLabel MapToModel(TaskLabelEntity entity)
    {
        return TaskLabel.Restore(
            entity.TaskId,
            entity.LabelId,
            entity.AssignedAt
        );
    }

    protected override TaskLabelEntity MapToEntity(TaskLabel model)
    {
        return new TaskLabelEntity
        {
            TaskId = model.TaskId,
            LabelId = model.LabelId,
            AssignedAt = model.AssignedAt
        };
    }

    protected override void UpdateEntity(TaskLabelEntity entity, TaskLabel model)
    {
        // TaskLabel обычно не обновляется
        entity.AssignedAt = model.AssignedAt;
    }

    // Вспомогательный метод для получения ID (не используется для составного ключа)
  /*  protected override Guid GetEntityId(TaskLabel model)
    {
        throw new NotSupportedException("TaskLabel has composite key");
    }
*/
}
