// TaskFlow.Core/Models/TaskItem.cs
using TaskFlow.Core.Common;
using TaskFlow.Core.Enums;

namespace TaskFlow.Core.Models;

public class TaskItem
{
    public Guid Id { get; private set; }
    public string Title { get; private set; }
    public string? Description { get; private set; }
    public int Order { get; private set; }
    public Priority Priority { get; private set; }
    public DateTime? DueDate { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    // Внешние ключи
    public Guid ColumnId { get; private set; }
    public Guid? AssignedUserId { get; private set; }
    public Guid CreatedById { get; private set; }

    // Навигационные свойства
    private readonly List<TaskLabel> _taskLabels = new();
    private readonly List<Comment> _comments = new();
    private readonly List<Attachment> _attachments = new();

    public IReadOnlyCollection<TaskLabel> TaskLabels => _taskLabels.AsReadOnly();
    public IReadOnlyCollection<Comment> Comments => _comments.AsReadOnly();
    public IReadOnlyCollection<Attachment> Attachments => _attachments.AsReadOnly();

    // Приватный конструктор
    private TaskItem(
        Guid id,
        string title,
        Guid columnId,
        Guid createdById,
        int order,
        DateTime createdAt)
    {
        // Инварианты
        if (id == Guid.Empty)
            throw new DomainException("Task ID cannot be empty");

        if (string.IsNullOrEmpty(title))
            throw new DomainException("Task title is required for existing task");

        if (columnId == Guid.Empty)
            throw new DomainException("Task must belong to a column");

        if (createdById == Guid.Empty)
            throw new DomainException("Task must have a creator");

        if (order < 0)
            throw new DomainException("Task order cannot be negative");

        Id = id;
        Title = title;
        ColumnId = columnId;
        CreatedById = createdById;
        Order = order;
        CreatedAt = createdAt;
        Priority = Priority.Medium;
        UpdatedAt = null;
        CompletedAt = null;
        AssignedUserId = null;
    }

    // Фабричный метод создания задачи
    public static TaskItem Create(
        string title,
        Guid columnId,
        Guid createdById,
        int order,
        string? description = null,
        Priority? priority = null,
        DateTime? dueDate = null)
    {
        // Предполагается, что title и order уже валидированы в Application слое
        // И что order уникален в пределах колонки (проверяется в репозитории)

        var task = new TaskItem(
            id: Guid.NewGuid(),
            title: title,
            columnId: columnId,
            createdById: createdById,
            order: order,
            createdAt: DateTime.UtcNow
        )
        {
            Description = description,
            DueDate = dueDate
        };

        if (priority.HasValue)
        {
            task.Priority = priority.Value;
        }

        return task;
    }

    // Метод для восстановления из БД
    public static TaskItem Restore(
        Guid id,
        string title,
        string? description,
        int order,
        Guid columnId,
        Guid createdById,
        Guid? assignedUserId,
        Priority priority,
        DateTime? dueDate,
        DateTime createdAt,
        DateTime? completedAt,
        DateTime? updatedAt)
    {
        return new TaskItem(id, title, columnId, createdById, order, createdAt)
        {
            Description = description,
            AssignedUserId = assignedUserId,
            Priority = priority,
            DueDate = dueDate,
            CompletedAt = completedAt,
            UpdatedAt = updatedAt
        };
    }

    // Основные доменные методы
    public void UpdateTitle(string title)
    {
        if (string.IsNullOrEmpty(title))
            throw new DomainException("Task title cannot be null or empty");

        Title = title;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateDescription(string? description)
    {
        Description = description;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdatePriority(Priority priority)
    {
        Priority = priority;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateDueDate(DateTime? dueDate)
    {
        DueDate = dueDate;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Complete()
    {
        if (CompletedAt == null)
        {
            CompletedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }
    }

    public void Uncomplete()
    {
        CompletedAt = null;
        UpdatedAt = DateTime.UtcNow;
    }

    public void AssignToUser(Guid userId)
    {
        if (userId == Guid.Empty)
            throw new DomainException("User ID cannot be empty");

        AssignedUserId = userId;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UnassignUser()
    {
        AssignedUserId = null;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MoveToColumn(Guid newColumnId, int newOrder)
    {
        if (newColumnId == Guid.Empty)
            throw new DomainException("Column ID cannot be empty");

        if (newOrder < 0)
            throw new DomainException("Task order cannot be negative");

        ColumnId = newColumnId;
        Order = newOrder;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateOrder(int newOrder)
    {
        if (newOrder < 0)
            throw new DomainException("Task order cannot be negative");

        Order = newOrder;
        UpdatedAt = DateTime.UtcNow;
    }

    // Методы управления метками (через TaskLabel)
    public void AddLabel(Label label)
    {
        if (label == null)
            throw new DomainException("Label cannot be null");

        var taskLabel = TaskLabel.Create(Id, label.Id);

        if (!_taskLabels.Contains(taskLabel))
        {
            _taskLabels.Add(taskLabel);
            UpdatedAt = DateTime.UtcNow;
        }
    }

    public void RemoveLabel(Guid labelId)
    {
        var taskLabel = _taskLabels.FirstOrDefault(tl => tl.LabelId == labelId);
        if (taskLabel != null)
        {
            _taskLabels.Remove(taskLabel);
            UpdatedAt = DateTime.UtcNow;
        }
    }

    public bool HasLabel(Guid labelId)
    {
        return _taskLabels.Any(tl => tl.LabelId == labelId);
    }

    public IEnumerable<Guid> GetLabelIds()
    {
        return _taskLabels.Select(tl => tl.LabelId);
    }

    public void ClearLabels()
    {
        if (_taskLabels.Count > 0)
        {
            _taskLabels.Clear();
            UpdatedAt = DateTime.UtcNow;
        }
    }

    // Методы управления комментариями
    public void AddComment(Comment comment)
    {
        if (comment == null)
            throw new DomainException("Comment cannot be null");

        _comments.Add(comment);
        UpdatedAt = DateTime.UtcNow;
    }

    public void RemoveComment(Guid commentId)
    {
        var comment = _comments.FirstOrDefault(c => c.Id == commentId);
        if (comment != null && comment.CanBeDeletedBy(CreatedById))
        {
            comment.Delete();
            UpdatedAt = DateTime.UtcNow;
        }
    }

    public int GetActiveCommentCount()
    {
        return _comments.Count(c => !c.IsDeleted);
    }

    public Comment? GetLatestComment()
    {
        return _comments
            .Where(c => !c.IsDeleted)
            .OrderByDescending(c => c.CreatedAt)
            .FirstOrDefault();
    }

    // Методы управления вложениями
    public void AddAttachment(Attachment attachment)
    {
        if (attachment == null)
            throw new DomainException("Attachment cannot be null");

        _attachments.Add(attachment);
        UpdatedAt = DateTime.UtcNow;
    }

    public void RemoveAttachment(Guid attachmentId)
    {
        var attachment = _attachments.FirstOrDefault(a => a.Id == attachmentId);
        if (attachment != null && attachment.CanBeDeletedBy(CreatedById))
        {
            _attachments.Remove(attachment);
            UpdatedAt = DateTime.UtcNow;
        }
    }

    public IEnumerable<Attachment> GetImageAttachments()
    {
        return _attachments
            .Where(a => a.IsImage)
            .OrderBy(a => a.UploadedAt);
    }

    public IEnumerable<Attachment> GetDocumentAttachments()
    {
        return _attachments
            .Where(a => a.IsDocument)
            .OrderBy(a => a.UploadedAt);
    }

    public long GetTotalAttachmentSize()
    {
        return _attachments.Sum(a => a.FileSize);
    }

    public string GetFormattedTotalSize()
    {
        var totalSize = GetTotalAttachmentSize();

        string[] sizes = { "B", "KB", "MB", "GB", "TB" };
        double len = totalSize;
        int order = 0;

        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len = len / 1024;
        }

        return $"{len:0.##} {sizes[order]}";
    }

    // Вспомогательные свойства
    public bool IsCompleted => CompletedAt != null;

    public bool IsOverdue => DueDate.HasValue &&
                            DueDate.Value < DateTime.UtcNow &&
                            !IsCompleted;

    public bool HasAssignee => AssignedUserId != null;

    public TimeSpan? TimeSpent
    {
        get
        {
            if (!IsCompleted || !CompletedAt.HasValue)
                return null;

            return CompletedAt.Value - CreatedAt;
        }
    }

    // Методы изменения приоритета
    public void EscalatePriority()
    {
        Priority = Priority switch
        {
            Priority.Low => Priority.Medium,
            Priority.Medium => Priority.High,
            Priority.High => Priority.Critical,
            Priority.Critical => Priority.Critical, // уже максимальный
            _ => Priority.Medium
        };
        UpdatedAt = DateTime.UtcNow;
    }

    public void DeescalatePriority()
    {
        Priority = Priority switch
        {
            Priority.Critical => Priority.High,
            Priority.High => Priority.Medium,
            Priority.Medium => Priority.Low,
            Priority.Low => Priority.Low, // уже минимальный
            _ => Priority.Medium
        };
        UpdatedAt = DateTime.UtcNow;
    }

    // Бизнес-правила доступа
    public bool CanBeDeletedBy(Guid userId)
    {
        return userId == CreatedById ||
               userId == AssignedUserId;
    }

    public bool CanBeEditedBy(Guid userId)
    {
        return userId == CreatedById ||
               userId == AssignedUserId;
    }

    // Приватный конструктор для EF Core
    private TaskItem() : this(
        Guid.NewGuid(),
        "Untitled Task",
        Guid.NewGuid(),
        Guid.NewGuid(),
        0,
        DateTime.UtcNow)
    { }
}