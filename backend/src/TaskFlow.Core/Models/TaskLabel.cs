// TaskFlow.Core/Models/TaskLabel.cs
using TaskFlow.Core.Common;

namespace TaskFlow.Core.Models;

/// <summary>
/// Value Object для связи задачи и метки
/// Не имеет собственного ID, составной ключ (TaskId + LabelId)
/// </summary>
public class TaskLabel
{
    public Guid TaskId { get; private set; }
    public Guid LabelId { get; private set; }
    public DateTime AssignedAt { get; private set; }

    // Навигационные свойства (опционально, только для удобства)
    public string? LabelName { get; private set; }
    public string? LabelColor { get; private set; }

    // Приватный конструктор
    private TaskLabel(Guid taskId, Guid labelId, DateTime assignedAt)
    {
        // Инварианты
        if (taskId == Guid.Empty)
            throw new DomainException("Task ID cannot be empty");

        if (labelId == Guid.Empty)
            throw new DomainException("Label ID cannot be empty");

        TaskId = taskId;
        LabelId = labelId;
        AssignedAt = assignedAt;
    }

    // Фабричный метод создания связи
    public static TaskLabel Create(Guid taskId, Guid labelId)
    {
        return new TaskLabel(
            taskId: taskId,
            labelId: labelId,
            assignedAt: DateTime.UtcNow
        );
    }

    // Метод для восстановления из БД с дополнительными данными
    public static TaskLabel Restore(
        Guid taskId,
        Guid labelId,
        DateTime assignedAt,
        string? labelName = null,
        string? labelColor = null)
    {
        return new TaskLabel(taskId, labelId, assignedAt)
        {
            LabelName = labelName,
            LabelColor = labelColor
        };
    }

    // Value Object методы сравнения
    public override bool Equals(object? obj)
    {
        if (obj is not TaskLabel other)
            return false;

        return TaskId == other.TaskId && LabelId == other.LabelId;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(TaskId, LabelId);
    }

    // Методы для удобства
    public bool IsForTask(Guid taskId)
    {
        return TaskId == taskId;
    }

    public bool IsForLabel(Guid labelId)
    {
        return LabelId == labelId;
    }

    // Приватный конструктор для EF Core
    private TaskLabel() : this(Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow) { }
}

// Расширения для работы с коллекциями TaskLabel
public static class TaskLabelExtensions
{
    public static bool ContainsLabel(this IEnumerable<TaskLabel> taskLabels, Guid labelId)
    {
        return taskLabels.Any(tl => tl.LabelId == labelId);
    }

    public static bool ContainsTask(this IEnumerable<TaskLabel> taskLabels, Guid taskId)
    {
        return taskLabels.Any(tl => tl.TaskId == taskId);
    }

    public static IEnumerable<Guid> GetLabelIdsForTask(this IEnumerable<TaskLabel> taskLabels, Guid taskId)
    {
        return taskLabels
            .Where(tl => tl.TaskId == taskId)
            .Select(tl => tl.LabelId);
    }

    public static IEnumerable<Guid> GetTaskIdsForLabel(this IEnumerable<TaskLabel> taskLabels, Guid labelId)
    {
        return taskLabels
            .Where(tl => tl.LabelId == labelId)
            .Select(tl => tl.TaskId);
    }

    public static int CountLabelsForTask(this IEnumerable<TaskLabel> taskLabels, Guid taskId)
    {
        return taskLabels.Count(tl => tl.TaskId == taskId);
    }

    public static int CountTasksForLabel(this IEnumerable<TaskLabel> taskLabels, Guid labelId)
    {
        return taskLabels.Count(tl => tl.LabelId == labelId);
    }
}