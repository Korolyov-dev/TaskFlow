// TaskFlow.Core/Models/Column.cs
using TaskFlow.Core.Common;

namespace TaskFlow.Core.Models;

public class Column
{
    public Guid Id { get; private set; }
    public string Title { get; private set; }
    public int Order { get; private set; }
    public int? WipLimit { get; private set; }
    public DateTime CreatedAt { get; private set; }

    // Внешние ключи
    public Guid BoardId { get; private set; }

    // Навигационные свойства (только Ids)
    private readonly List<Guid> _taskIds = new();
    public IReadOnlyCollection<Guid> TaskIds => _taskIds.AsReadOnly();

    // Приватный конструктор
    private Column(Guid id, string title, int order, Guid boardId, DateTime createdAt)
    {
        // Инварианты
        if (id == Guid.Empty)
            throw new DomainException("Column ID cannot be empty");

        if (string.IsNullOrEmpty(title))
            throw new DomainException("Column title is required for existing column");

        if (boardId == Guid.Empty)
            throw new DomainException("Column must belong to a board");

        if (order < 0)
            throw new DomainException("Column order cannot be negative");

        Id = id;
        Title = title;
        Order = order;
        BoardId = boardId;
        CreatedAt = createdAt;
        WipLimit = null;
    }

    // Фабричный метод создания колонки
    public static Column Create(string title, Guid boardId, int order, int? wipLimit = null)
    {
        // Предполагается, что title и order уже валидированы в Application слое
        // И что order уникален в пределах доски (это должно проверяться в репозитории)

        var column = new Column(
            id: Guid.NewGuid(),
            title: title,
            order: order,
            boardId: boardId,
            createdAt: DateTime.UtcNow
        )
        {
            WipLimit = wipLimit
        };

        return column;
    }

    // Метод для восстановления из БД
    public static Column Restore(
        Guid id,
        string title,
        int order,
        Guid boardId,
        int? wipLimit,
        DateTime createdAt)
    {
        return new Column(id, title, order, boardId, createdAt)
        {
            WipLimit = wipLimit
        };
    }

    // Доменные методы
    public void UpdateTitle(string title)
    {
        if (string.IsNullOrEmpty(title))
            throw new DomainException("Column title cannot be null or empty");

        Title = title;
    }

    public void UpdateOrder(int newOrder)
    {
        if (newOrder < 0)
            throw new DomainException("Column order cannot be negative");

        Order = newOrder;
    }

    public void UpdateWipLimit(int? wipLimit)
    {
        if (wipLimit.HasValue && wipLimit.Value <= 0)
            throw new DomainException("WIP limit must be positive or null");

        WipLimit = wipLimit;
    }

    public void AddTask(Guid taskId)
    {
        if (!_taskIds.Contains(taskId))
        {
            _taskIds.Add(taskId);
        }
    }

    public void RemoveTask(Guid taskId)
    {
        if (_taskIds.Contains(taskId))
        {
            _taskIds.Remove(taskId);
        }
    }

    public void ReorderTasks(List<Guid> taskIdsInOrder)
    {
        // Этот метод скорее всего будет вызываться из Board
        // Фактическое переупорядочивание происходит на уровне TaskItem
        // Здесь мы только обновляем нашу ссылку на порядок

        // Очищаем и добавляем в новом порядке
        _taskIds.Clear();
        _taskIds.AddRange(taskIdsInOrder);
    }

    public bool HasReachedWipLimit()
    {
        if (!WipLimit.HasValue)
            return false;

        return _taskIds.Count >= WipLimit.Value;
    }

    public bool CanAddTask()
    {
        return !HasReachedWipLimit();
    }

    public int GetTaskCount()
    {
        return _taskIds.Count;
    }

    // Вспомогательный метод для пересчета порядка задач
    // Этот метод будет использоваться при массовом обновлении
    public Dictionary<Guid, int> CalculateNewTaskOrder(List<Guid> taskIdsInOrder)
    {
        var newOrder = new Dictionary<Guid, int>();

        for (int i = 0; i < taskIdsInOrder.Count; i++)
        {
            newOrder[taskIdsInOrder[i]] = i;
        }

        return newOrder;
    }

    // Приватный конструктор для EF Core
    private Column() : this(
        Guid.NewGuid(),
        "Untitled Column",
        0,
        Guid.NewGuid(),
        DateTime.UtcNow)
    { }
}