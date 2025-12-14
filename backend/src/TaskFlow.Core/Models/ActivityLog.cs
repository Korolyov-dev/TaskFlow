using TaskFlow.Core.Common;
using TaskFlow.Core.Enums;

namespace TaskFlow.Core.Models;

public class ActivityLog
{
    public Guid Id { get; private set; }
    public Guid BoardId { get; private set; }
    public Guid? UserId { get; private set; } // Может быть null для системных событий
    public ActivityType ActivityType { get; private set; }
    public string Description { get; private set; }
    public Dictionary<string, object> Metadata { get; private set; }
    public DateTime CreatedAt { get; private set; }

    // Навигационные свойства (денормализация)
    public string? UserName { get; private set; }
    public string? UserAvatarUrl { get; private set; }
    public string? TaskTitle { get; private set; }
    public string? ColumnTitle { get; private set; }

    // Приватный конструктор
    private ActivityLog(
        Guid id,
        Guid boardId,
        ActivityType activityType,
        string description,
        DateTime createdAt)
    {
        // Инварианты
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
        Metadata = new Dictionary<string, object>();
        UserId = null;
    }

    // Фабричные методы для различных типов активности
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

    // Специализированные фабричные методы для удобства
    public static ActivityLog TaskCreated(
        Guid boardId,
        Guid userId,
        Guid taskId,
        string taskTitle,
        Guid columnId,
        string columnTitle)
    {
        var log = Create(
            boardId: boardId,
            activityType: ActivityType.TaskCreated,
            description: $"создал(а) задачу '{taskTitle}' в колонке '{columnTitle}'",
            userId: userId
        );

        log.AddMetadata("taskId", taskId);
        log.AddMetadata("taskTitle", taskTitle);
        log.AddMetadata("columnId", columnId);
        log.AddMetadata("columnTitle", columnTitle);

        return log;
    }

    public static ActivityLog TaskMoved(
        Guid boardId,
        Guid userId,
        Guid taskId,
        string taskTitle,
        Guid fromColumnId,
        string fromColumnTitle,
        Guid toColumnId,
        string toColumnTitle)
    {
        var log = Create(
            boardId: boardId,
            activityType: ActivityType.TaskMoved,
            description: $"переместил(а) задачу '{taskTitle}' из '{fromColumnTitle}' в '{toColumnTitle}'",
            userId: userId
        );

        log.AddMetadata("taskId", taskId);
        log.AddMetadata("taskTitle", taskTitle);
        log.AddMetadata("fromColumnId", fromColumnId);
        log.AddMetadata("fromColumnTitle", fromColumnTitle);
        log.AddMetadata("toColumnId", toColumnId);
        log.AddMetadata("toColumnTitle", toColumnTitle);

        return log;
    }

    public static ActivityLog TaskCompleted(
        Guid boardId,
        Guid userId,
        Guid taskId,
        string taskTitle)
    {
        var log = Create(
            boardId: boardId,
            activityType: ActivityType.TaskCompleted,
            description: $"завершил(а) задачу '{taskTitle}'",
            userId: userId
        );

        log.AddMetadata("taskId", taskId);
        log.AddMetadata("taskTitle", taskTitle);

        return log;
    }

    public static ActivityLog CommentAdded(
        Guid boardId,
        Guid userId,
        Guid taskId,
        string taskTitle,
        Guid commentId)
    {
        var log = Create(
            boardId: boardId,
            activityType: ActivityType.CommentAdded,
            description: $"прокомментировал(а) задачу '{taskTitle}'",
            userId: userId
        );

        log.AddMetadata("taskId", taskId);
        log.AddMetadata("taskTitle", taskTitle);
        log.AddMetadata("commentId", commentId);

        return log;
    }

    public static ActivityLog MemberAdded(
        Guid boardId,
        Guid addedByUserId,
        Guid addedUserId,
        string addedUserName)
    {
        var log = Create(
            boardId: boardId,
            activityType: ActivityType.MemberAdded,
            description: $"добавил(а) участника '{addedUserName}' на доску",
            userId: addedByUserId
        );

        log.AddMetadata("addedUserId", addedUserId);
        log.AddMetadata("addedUserName", addedUserName);

        return log;
    }

    // Метод для восстановления из БД
    public static ActivityLog Restore(
        Guid id,
        Guid boardId,
        Guid? userId,
        ActivityType activityType,
        string description,
        Dictionary<string, object> metadata,
        DateTime createdAt,
        string? UserName = null,
        string? userAvatarUrl = null,
        string? taskTitle = null,
        string? columnTitle = null)
    {
        return new ActivityLog(id, boardId, activityType, description, createdAt)
        {
            UserId = userId,
            Metadata = metadata ?? new Dictionary<string, object>(),
            UserName = UserName,
            UserAvatarUrl = userAvatarUrl,
            TaskTitle = taskTitle,
            ColumnTitle = columnTitle
        };
    }

    // Методы работы с метаданными
    public void AddMetadata(string key, object value)
    {
        if (string.IsNullOrEmpty(key))
            throw new DomainException("Metadata key cannot be null or empty");

        Metadata[key] = value;
    }

    public T? GetMetadata<T>(string key)
    {
        if (Metadata.TryGetValue(key, out var value) && value is T typedValue)
        {
            return typedValue;
        }
        return default;
    }

    public bool TryGetMetadata<T>(string key, out T? value)
    {
        if (Metadata.TryGetValue(key, out var objValue) && objValue is T typedValue)
        {
            value = typedValue;
            return true;
        }

        value = default;
        return false;
    }

    // Вспомогательные свойства
    public bool IsSystemEvent => UserId == null;

    public string GetRelativeTime()
    {
        var timeSpan = DateTime.UtcNow - CreatedAt;

        if (timeSpan.TotalSeconds < 60)
            return "только что";

        if (timeSpan.TotalMinutes < 60)
        {
            var minutes = (int)timeSpan.TotalMinutes;
            return $"{minutes} {Pluralize(minutes, "минуту", "минуты", "минут")} назад";
        }

        if (timeSpan.TotalHours < 24)
        {
            var hours = (int)timeSpan.TotalHours;
            return $"{hours} {Pluralize(hours, "час", "часа", "часов")} назад";
        }

        if (timeSpan.TotalDays < 30)
        {
            var days = (int)timeSpan.TotalDays;
            return $"{days} {Pluralize(days, "день", "дня", "дней")} назад";
        }

        var months = (int)(timeSpan.TotalDays / 30);
        return $"{months} {Pluralize(months, "месяц", "месяца", "месяцев")} назад";
    }

    public string GetIcon()
    {
        return ActivityType switch
        {
            ActivityType.TaskCreated => "plus-circle",
            ActivityType.TaskUpdated => "edit",
            ActivityType.TaskDeleted => "trash",
            ActivityType.TaskMoved => "move",
            ActivityType.TaskCompleted => "check-circle",
            ActivityType.CommentAdded => "message-circle",
            ActivityType.CommentDeleted => "message-square",
            ActivityType.MemberAdded => "user-plus",
            ActivityType.MemberRemoved => "user-minus",
            ActivityType.BoardCreated => "layout",
            ActivityType.BoardUpdated => "edit-2",
            ActivityType.AttachmentAdded => "paperclip",
            ActivityType.LabelAdded => "tag",
            ActivityType.LabelRemoved => "tag-off",
            _ => "activity"
        };
    }

    public string GetColor()
    {
        return ActivityType switch
        {
            ActivityType.TaskCreated => "#10b981",      // Green
            ActivityType.TaskCompleted => "#10b981",    // Green
            ActivityType.TaskDeleted => "#ef4444",      // Red
            ActivityType.TaskMoved => "#3b82f6",        // Blue
            ActivityType.CommentAdded => "#8b5cf6",     // Purple
            ActivityType.MemberAdded => "#f59e0b",      // Amber
            ActivityType.MemberRemoved => "#ef4444",    // Red
            ActivityType.BoardCreated => "#06b6d4",     // Cyan
            _ => "#6b7280"                             // Gray
        };
    }

    private static string Pluralize(int number, string one, string two, string five)
    {
        var n = Math.Abs(number) % 100;
        var n1 = n % 10;

        if (n > 10 && n < 20) return five;
        if (n1 > 1 && n1 < 5) return two;
        if (n1 == 1) return one;
        return five;
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


// Расширения для работы с логом активности
public static class ActivityLogExtensions
{
    public static IEnumerable<ActivityLog> ByUser(this IEnumerable<ActivityLog> logs, Guid userId)
    {
        return logs.Where(l => l.UserId == userId);
    }

    public static IEnumerable<ActivityLog> ByActivityType(this IEnumerable<ActivityLog> logs, ActivityType activityType)
    {
        return logs.Where(l => l.ActivityType == activityType);
    }

    public static IEnumerable<ActivityLog> Recent(this IEnumerable<ActivityLog> logs, TimeSpan timeSpan)
    {
        var cutoff = DateTime.UtcNow - timeSpan;
        return logs.Where(l => l.CreatedAt >= cutoff);
    }

    public static IEnumerable<ActivityLog> ForTask(this IEnumerable<ActivityLog> logs, Guid taskId)
    {
        return logs.Where(l =>
            l.TryGetMetadata<Guid>("taskId", out var metadataTaskId) &&
            metadataTaskId == taskId);
    }

    public static Dictionary<ActivityType, int> GetActivityStats(this IEnumerable<ActivityLog> logs)
    {
        return logs
            .GroupBy(l => l.ActivityType)
            .ToDictionary(g => g.Key, g => g.Count());
    }

    public static Dictionary<Guid, int> GetUserActivityStats(this IEnumerable<ActivityLog> logs)
    {
        return logs
            .Where(l => l.UserId.HasValue)
            .GroupBy(l => l.UserId!.Value)
            .ToDictionary(g => g.Key, g => g.Count());
    }
}