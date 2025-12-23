// TaskFlow.Infrastructure/Repositories/ActivityLogRepository.cs
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Text.Json;
using TaskFlow.Core.Common;
using TaskFlow.Core.Enums;
using TaskFlow.Core.Interfaces;
using TaskFlow.Core.Models;
using TaskFlow.Infrastructure.Data;
using TaskFlow.Infrastructure.Entities;

namespace TaskFlow.Infrastructure.Repositories;

public class ActivityLogRepository : BaseRepository<ActivityLog, ActivityLogEntity>, IActivityLogRepository
{
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = false
    };

    public ActivityLogRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<ActivityLog>> GetBoardActivitiesAsync(Guid boardId, CancellationToken cancellationToken = default)
    {
        var entities = await _dbSet
            .AsNoTracking()
            .Where(al => al.BoardId == boardId)
            .OrderByDescending(al => al.CreatedAt)
            .Take(100)
            .ToListAsync(cancellationToken);

        return entities.Select(MapToModel).ToList();
    }

    public async Task<IReadOnlyList<ActivityLog>> GetUserActivitiesAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var entities = await _dbSet
            .AsNoTracking()
            .Where(al => al.UserId == userId)
            .OrderByDescending(al => al.CreatedAt)
            .Take(50)
            .ToListAsync(cancellationToken);

        return entities.Select(MapToModel).ToList();
    }

    public async Task<IReadOnlyList<ActivityLog>> GetTaskActivitiesAsync(Guid taskId, CancellationToken cancellationToken = default)
    {
        var entities = await _dbSet
            .AsNoTracking()
            .Where(al => al.RelatedTaskId == taskId)
            .OrderByDescending(al => al.CreatedAt)
            .Take(50)
            .ToListAsync(cancellationToken);

        return entities.Select(MapToModel).ToList();
    }

    public async Task<IReadOnlyList<ActivityLog>> GetRecentActivitiesAsync(int count, CancellationToken cancellationToken = default)
    {
        var entities = await _dbSet
            .AsNoTracking()
            .OrderByDescending(al => al.CreatedAt)
            .Take(count)
            .ToListAsync(cancellationToken);

        return entities.Select(MapToModel).ToList();
    }

    protected override ActivityLog MapToModel(ActivityLogEntity entity)
    {
        return ActivityLog.Restore(
            entity.Id,
            entity.BoardId,
            entity.UserId,
            (ActivityType)entity.ActivityType,
            entity.Description,
            entity.CreatedAt,
            entity.RelatedTaskId,
            entity.RelatedColumnId,
            entity.RelatedUserId,
            entity.OldValue,
            entity.NewValue
        );
    }

    protected override ActivityLogEntity MapToEntity(ActivityLog model)
    {
        return new ActivityLogEntity
        {
            Id = model.Id,
            BoardId = model.BoardId,
            UserId = model.UserId,
            ActivityType = (int)model.ActivityType,
            Description = model.Description,
            CreatedAt = model.CreatedAt,
            RelatedTaskId = model.RelatedTaskId,
            RelatedColumnId = model.RelatedColumnId,
            RelatedUserId = model.RelatedUserId,
            OldValue = model.OldValue,
            NewValue = model.NewValue
        };
    }

    protected override void UpdateEntity(ActivityLogEntity entity, ActivityLog model)
    {
        // ActivityLog обычно только создается, не обновляется
        entity.Description = model.Description;
    }
}