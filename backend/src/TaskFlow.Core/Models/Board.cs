// TaskFlow.Core/Models/Board.cs
using TaskFlow.Core.Common;

namespace TaskFlow.Core.Models;

public class Board
{
    public Guid Id { get; private set; }
    public string Title { get; private set; }
    public string? Description { get; private set; }
    public string Color { get; private set; }
    public bool IsFavorite { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public Guid OwnerId { get; private set; }

    // Навигационные свойства
    private readonly List<Guid> _columnIds = new();
    private readonly List<Guid> _memberIds = new();
    private readonly List<Guid> _labelIds = new();
    private readonly List<ActivityLog> _activityLogs = new();

    public IReadOnlyCollection<Guid> ColumnIds => _columnIds.AsReadOnly();
    public IReadOnlyCollection<Guid> MemberIds => _memberIds.AsReadOnly();
    public IReadOnlyCollection<Guid> LabelIds => _labelIds.AsReadOnly();
    public IReadOnlyCollection<ActivityLog> ActivityLogs => _activityLogs.AsReadOnly();

    // Приватный конструктор
    private Board(
        Guid id,
        string title,
        Guid ownerId,
        DateTime createdAt,
        string color)
    {
        // Инварианты
        if (id == Guid.Empty)
            throw new DomainException("Board ID cannot be empty");

        if (string.IsNullOrEmpty(title))
            throw new DomainException("Board title is required for existing board");

        if (ownerId == Guid.Empty)
            throw new DomainException("Board must have an owner");

        Id = id;
        Title = title;
        OwnerId = ownerId;
        CreatedAt = createdAt;
        Color = color;
        IsFavorite = false;
        UpdatedAt = null;
    }

    // Фабричный метод создания доски
    public static Board Create(string title, Guid ownerId, string? description = null, string color = "#4f46e5")
    {
        // Предполагается, что title уже валидирован в Application слое
        return new Board(
            id: Guid.NewGuid(),
            title: title,
            ownerId: ownerId,
            createdAt: DateTime.UtcNow,
            color: color
        )
        {
            Description = description
        };
    }

    // Метод для восстановления из БД
    public static Board Restore(
        Guid id,
        string title,
        string? description,
        string color,
        bool isFavorite,
        Guid ownerId,
        DateTime createdAt,
        DateTime? updatedAt)
    {
        return new Board(id, title, ownerId, createdAt, color)
        {
            Description = description,
            IsFavorite = isFavorite,
            UpdatedAt = updatedAt
        };
    }

    // Доменные методы
    public void UpdateTitle(string title)
    {
        if (string.IsNullOrEmpty(title))
            throw new DomainException("Board title cannot be null or empty");

        Title = title;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateDescription(string? description)
    {
        Description = description;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateColor(string color)
    {
        if (string.IsNullOrEmpty(color) || color.Length != 7 || color[0] != '#')
            throw new DomainException("Invalid color format. Expected #RRGGBB");

        Color = color;
        UpdatedAt = DateTime.UtcNow;
    }

    public void ToggleFavorite()
    {
        IsFavorite = !IsFavorite;
        UpdatedAt = DateTime.UtcNow;
    }

    public void AddColumn(Guid columnId)
    {
        if (!_columnIds.Contains(columnId))
        {
            _columnIds.Add(columnId);
            UpdatedAt = DateTime.UtcNow;
        }
    }

    public void RemoveColumn(Guid columnId)
    {
        if (_columnIds.Contains(columnId))
        {
            _columnIds.Remove(columnId);
            UpdatedAt = DateTime.UtcNow;
        }
    }

    public void AddMember(Guid memberId)
    {
        if (!_memberIds.Contains(memberId))
        {
            _memberIds.Add(memberId);
            UpdatedAt = DateTime.UtcNow;
        }
    }

    public void RemoveMember(Guid memberId)
    {
        if (_memberIds.Contains(memberId))
        {
            _memberIds.Remove(memberId);
            UpdatedAt = DateTime.UtcNow;
        }
    }

    public void AddLabel(Guid labelId)
    {
        if (!_labelIds.Contains(labelId))
        {
            _labelIds.Add(labelId);
            UpdatedAt = DateTime.UtcNow;
        }
    }

    public void RemoveLabel(Guid labelId)
    {
        if (_labelIds.Contains(labelId))
        {
            _labelIds.Remove(labelId);
            UpdatedAt = DateTime.UtcNow;
        }
    }

    public void AddActivity(ActivityLog activity)
    {
        if (activity == null)
            throw new DomainException("Activity cannot be null");

        if (activity.BoardId != Id)
            throw new DomainException("Activity must belong to this board");

        _activityLogs.Add(activity);
    }

    public bool IsMember(Guid userId)
    {
        return userId == OwnerId || _memberIds.Contains(userId);
    }

    public bool CanEdit(Guid userId)
    {
        return IsMember(userId);
    }

    public bool CanDelete(Guid userId)
    {
        return userId == OwnerId;
    }

    // Методы для работы с активностью
    public IEnumerable<ActivityLog> GetRecentActivities(int count = 50)
    {
        return _activityLogs
            .OrderByDescending(a => a.CreatedAt)
            .Take(count);
    }

    public IEnumerable<ActivityLog> GetActivitiesByType(ActivityType activityType)
    {
        return _activityLogs
            .Where(a => a.ActivityType == activityType)
            .OrderByDescending(a => a.CreatedAt);
    }

    public Dictionary<ActivityType, int> GetActivityStatistics()
    {
        return _activityLogs
            .GroupBy(a => a.ActivityType)
            .ToDictionary(g => g.Key, g => g.Count());
    }

    public IEnumerable<ActivityLog> GetUserActivities(Guid userId)
    {
        return _activityLogs
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.CreatedAt);
    }

    // Приватный конструктор для EF Core
    private Board() : this(
        Guid.NewGuid(),
        "Untitled Board",
        Guid.NewGuid(),
        DateTime.UtcNow,
        "#4f46e5")
    { }
}