// TaskFlow.Core/Models/ActivityLog.cs
using TaskFlow.Core.Common;

namespace TaskFlow.Core.Models;

public class ActivityLog
{
    public Guid Id { get; private set; }
    public Guid BoardId { get; private set; }
    public Guid? UserId { get; private set; }
    public ActivityType ActivityType { get; private set; }
    public string Description { get; private set; }
    public DateTime CreatedAt { get; private set; }

    // Без сложных метаданных - только простые поля
    public Guid? RelatedTaskId { get; private set; }
    public Guid? RelatedColumnId { get; private set; }
    public Guid? RelatedUserId { get; private set; }
    public string? OldValue { get; private set; }
    public string? NewValue { get; private set; }

    // Приватный конструктор
    private ActivityLog(
        Guid id,
        Guid boardId,
        ActivityType activityType,
        string description,
        DateTime createdAt)
    {
        if (id == Guid.Empty)
            throw new DomainException("ActivityLog ID cannot be empty");

        if (boardId == Guid.Empty)
            throw new DomainException("ActivityLog must belong to a board");

        if (string.IsNullOrEmpty(description))
            throw new DomainException("ActivityLog description is required");

        Id = id;
        BoardId = boardId;
        ActivityType = activityType;
        Description = description;
        CreatedAt = createdAt;
        UserId = null;
        RelatedTaskId = null;
        RelatedColumnId = null;
        RelatedUserId = null;
        OldValue = null;
        NewValue = null;
    }

    // Упрощенные фабричные методы
    public static ActivityLog Create(
        Guid boardId,
        ActivityType activityType,
        string description,
        Guid? userId = null)
    {
        return new ActivityLog(
            id: Guid.NewGuid(),
            boardId: boardId,
            activityType: activityType,
            description: description,
            createdAt: DateTime.UtcNow
        )
        {
            UserId = userId
        };
    }

    // Метод для задачи
    public static ActivityLog ForTask(
        Guid boardId,
        Guid userId,
        ActivityType activityType,
        string description,
        Guid taskId)
    {
        return new ActivityLog(
            id: Guid.NewGuid(),
            boardId: boardId,
            activityType: activityType,
            description: description,
            createdAt: DateTime.UtcNow
        )
        {
            UserId = userId,
            RelatedTaskId = taskId
        };
    }

    // Метод для изменения значения
    public static ActivityLog ForValueChange(
        Guid boardId,
        Guid userId,
        ActivityType activityType,
        string description,
        string oldValue,
        string newValue)
    {
        return new ActivityLog(
            id: Guid.NewGuid(),
            boardId: boardId,
            activityType: activityType,
            description: description,
            createdAt: DateTime.UtcNow
        )
        {
            UserId = userId,
            OldValue = oldValue,
            NewValue = newValue
        };
    }

    // Метод для восстановления
    public static ActivityLog Restore(
        Guid id,
        Guid boardId,
        Guid? userId,
        ActivityType activityType,
        string description,
        DateTime createdAt,
        Guid? relatedTaskId = null,
        Guid? relatedColumnId = null,
        Guid? relatedUserId = null,
        string? oldValue = null,
        string? newValue = null)
    {
        return new ActivityLog(id, boardId, activityType, description, createdAt)
        {
            UserId = userId,
            RelatedTaskId = relatedTaskId,
            RelatedColumnId = relatedColumnId,
            RelatedUserId = relatedUserId,
            OldValue = oldValue,
            NewValue = newValue
        };
    }

    // Приватный конструктор для EF Core
    private ActivityLog() : this(
        Guid.NewGuid(),
        Guid.NewGuid(),
        ActivityType.TaskCreated,
        "System activity",
        DateTime.UtcNow)
    { }
}