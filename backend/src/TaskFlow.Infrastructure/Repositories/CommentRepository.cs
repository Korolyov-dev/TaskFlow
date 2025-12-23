using Microsoft.EntityFrameworkCore;
using TaskFlow.Core.Models;
using TaskFlow.Infrastructure.Data;
using TaskFlow.Infrastructure.Entities;

namespace TaskFlow.Infrastructure.Repositories;

public class CommentRepository : BaseRepository<Comment, CommentEntity>, ICommentRepository
{
    public CommentRepository(ApplicationDbContext context):base(context)
    {
    }
    public async Task<IReadOnlyList<Comment>> GetTaskCommentsAsync(Guid taskId, CancellationToken cancellationToken = default)
    {
        var entities = await _dbSet
            .AsNoTracking()
            .Where(c => c.TaskId == taskId && c.DeletedAt == null)
            .OrderBy(c => c.CreatedAt)
            .ToListAsync(cancellationToken);

        return entities.Select(MapToModel).ToList();
    }

    public async Task<IReadOnlyList<Comment>> GetUserCommentsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var entities = await _dbSet
            .AsNoTracking()
            .Where(c => c.UserId == userId && c.DeletedAt == null)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync(cancellationToken);

        return entities.Select(MapToModel).ToList();
    }

    protected override Comment MapToModel(CommentEntity entity)
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

    protected override CommentEntity MapToEntity(Comment model)
    {
        return new CommentEntity
        {
            Id = model.Id,
            Content = model.Content,
            TaskId = model.TaskId,
            UserId = model.UserId,
            CreatedAt = model.CreatedAt,
            UpdatedAt = model.UpdatedAt,
            DeletedAt = model.DeletedAt
        };
    }

    protected override void UpdateEntity(CommentEntity entity, Comment model)
    {
        entity.Content = model.Content;
        entity.UpdatedAt = model.UpdatedAt;
        entity.DeletedAt = model.DeletedAt;
    }
}
