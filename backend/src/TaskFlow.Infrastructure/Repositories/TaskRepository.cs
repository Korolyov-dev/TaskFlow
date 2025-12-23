using Microsoft.EntityFrameworkCore;
using TaskFlow.Core.Enums;
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

    public async Task<TaskItem?> GetTaskWithDetailsAsync(Guid taskId, CancellationToken cancellationToken = default)
    {
        var taskEntity = await _dbSet
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == taskId, cancellationToken);

        if (taskEntity == null)
            return null;

        var task = MapToModel(taskEntity);

        // Загружаем метки задачи
        var labelIds = await _context.TaskLabels
            .Where(tl => tl.TaskId == taskId)
            .Select(tl => tl.LabelId)
            .ToListAsync(cancellationToken);


        foreach (var labelId in labelIds)
        {
            task.AddLabelById(labelId);
        }

        // Загружаем комментарии
        var commentEntities = await _context.Comments
            .Where(c => c.TaskId == taskId && c.DeletedAt == null)
            .OrderBy(c => c.CreatedAt)
            .ToListAsync(cancellationToken);

        foreach (var commentEntity in commentEntities)
        {
            var comment = Comment.Restore(
                commentEntity.Id,
                commentEntity.Content,
                commentEntity.TaskId,
                commentEntity.UserId,
                commentEntity.CreatedAt,
                commentEntity.UpdatedAt,
                commentEntity.DeletedAt
            );
            task.AddComment(comment);
        }

        // Загружаем вложения
        var attachmentEntities = await _context.Attachments
            .Where(a => a.TaskId == taskId)
            .OrderBy(a => a.UploadedAt)
            .ToListAsync(cancellationToken);

        foreach (var attachmentEntity in attachmentEntities)
        {
            var attachment = Attachment.Restore(
                attachmentEntity.Id,
                attachmentEntity.FileName,
                attachmentEntity.FileUrl,
                attachmentEntity.FileSize,
                attachmentEntity.MimeType,
                attachmentEntity.TaskId,
                attachmentEntity.UploadedBy,
                attachmentEntity.UploadedAt
            );
            task.AddAttachment(attachment);
        }

        return task;
    }

    public async Task<IReadOnlyList<TaskItem>> GetTasksByColumnAsync(Guid columnId, CancellationToken cancellationToken = default)
    {
        var entities = await _dbSet
            .AsNoTracking()
            .Where(t => t.ColumnId == columnId)
            .OrderBy(t => t.Order)
            .ToListAsync(cancellationToken);

        return entities.Select(MapToModel).ToList();
    }

    public async Task<IReadOnlyList<TaskItem>> GetTasksByBoardAsync(Guid boardId, CancellationToken cancellationToken = default)
    {
        // Получаем задачи через колонки доски
        var columnIds = await _context.Columns
            .Where(c => c.BoardId == boardId)
            .Select(c => c.Id)
            .ToListAsync(cancellationToken);

        var entities = await _dbSet
            .AsNoTracking()
            .Where(t => columnIds.Contains(t.ColumnId))
            .OrderBy(t => t.Order)
            .ToListAsync(cancellationToken);

        return entities.Select(MapToModel).ToList();
    }

    public async Task<IReadOnlyList<TaskItem>> GetUserTasksAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var entities = await _dbSet
            .AsNoTracking()
            .Where(t => t.CreatedById == userId)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync(cancellationToken);

        return entities.Select(MapToModel).ToList();
    }

    public async Task<IReadOnlyList<TaskItem>> GetAssignedTasksAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var entities = await _dbSet
            .AsNoTracking()
            .Where(t => t.AssignedUserId == userId)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync(cancellationToken);

        return entities.Select(MapToModel).ToList();
    }

    public async Task UpdateTaskOrderAsync(Guid taskId, Guid newColumnId, int newOrder, CancellationToken cancellationToken = default)
    {
        var taskEntity = await _dbSet.FindAsync(new object[] { taskId }, cancellationToken);
        if (taskEntity != null)
        {
            taskEntity.ColumnId = newColumnId;
            taskEntity.Order = newOrder;
            taskEntity.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task ReorderTasksInColumnAsync(Guid columnId, List<Guid> taskIdsInOrder, CancellationToken cancellationToken = default)
    {
        var tasks = await _dbSet
            .Where(t => t.ColumnId == columnId && taskIdsInOrder.Contains(t.Id))
            .ToListAsync(cancellationToken);

        var order = 0;
        foreach (var taskId in taskIdsInOrder)
        {
            var task = tasks.FirstOrDefault(t => t.Id == taskId);
            if (task != null)
            {
                task.Order = order++;
                task.UpdatedAt = DateTime.UtcNow;
            }
        }

        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<int> GetNextOrderInColumnAsync(Guid columnId, CancellationToken cancellationToken = default)
    {
        var maxOrder = await _dbSet
            .Where(t => t.ColumnId == columnId)
            .MaxAsync(t => (int?)t.Order, cancellationToken);

        return (maxOrder ?? -1) + 1;
    }

    // Вспомогательные методы маппинга
    private Comment MapToCommentModel(CommentEntity entity)
    {
        return Comment.Restore(
            entity.Id,
            entity.Content,
            entity.TaskId,
            entity.UserId,
            entity.CreatedAt,
            entity.UpdatedAt,
            entity.DeletedAt
        );
    }

    private Attachment MapToAttachmentModel(AttachmentEntity entity)
    {
        return Attachment.Restore(
            entity.Id,
            entity.FileName,
            entity.FileUrl,
            entity.FileSize,
            entity.MimeType,
            entity.TaskId,
            entity.UploadedBy,
            entity.UploadedAt
        );
    }

    protected override TaskItem MapToModel(TaskItemEntity entity)
    {
        return TaskItem.Restore(
            entity.Id,
            entity.Title,
            entity.Description,
            entity.Order,
            entity.ColumnId,
            entity.CreatedById,
            entity.AssignedUserId,
            (Priority)entity.Priority,
            entity.DueDate,
            entity.CreatedAt,
            entity.CompletedAt,
            entity.UpdatedAt
        );
    }

    protected override TaskItemEntity MapToEntity(TaskItem model)
    {
        return new TaskItemEntity
        {
            Id = model.Id,
            Title = model.Title,
            Description = model.Description,
            Order = model.Order,
            Priority = model.Priority,
            DueDate = model.DueDate,
            CreatedAt = model.CreatedAt,
            CompletedAt = model.CompletedAt,
            UpdatedAt = model.UpdatedAt,
            ColumnId = model.ColumnId,
            CreatedById = model.CreatedById,
            AssignedUserId = model.AssignedUserId
        };
    }

    protected override void UpdateEntity(TaskItemEntity entity, TaskItem model)
    {
        entity.Title = model.Title;
        entity.Description = model.Description;
        entity.Order = model.Order;
        entity.Priority = model.Priority;
        entity.DueDate = model.DueDate;
        entity.CompletedAt = model.CompletedAt;
        entity.UpdatedAt = model.UpdatedAt;
        entity.ColumnId = model.ColumnId;
        entity.AssignedUserId = model.AssignedUserId;
    }
}
