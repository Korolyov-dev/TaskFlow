// TaskFlow.Core/Models/Label.cs
using TaskFlow.Core.Common;

namespace TaskFlow.Core.Models;

public class Label
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public string Color { get; private set; }
    public DateTime CreatedAt { get; private set; }

    // Внешние ключи
    public Guid BoardId { get; private set; }

    // Навигационные свойства (только Ids)
    private readonly List<Guid> _taskIds = new();
    public IReadOnlyCollection<Guid> TaskIds => _taskIds.AsReadOnly();

    // Приватный конструктор
    private Label(Guid id, string name, Guid boardId, DateTime createdAt, string color)
    {
        // Инварианты
        if (id == Guid.Empty)
            throw new DomainException("Label ID cannot be empty");

        if (string.IsNullOrEmpty(name))
            throw new DomainException("Label name is required for existing label");

        if (boardId == Guid.Empty)
            throw new DomainException("Label must belong to a board");

        Id = id;
        Name = name;
        BoardId = boardId;
        CreatedAt = createdAt;
        Color = color;
    }

    // Фабричный метод создания метки
    public static Label Create(string name, Guid boardId, string color = "#6b7280")
    {
        // Предполагается, что name уже валидирован в Application слое
        // И что имя уникально в пределах доски (проверяется в репозитории)

        return new Label(
            id: Guid.NewGuid(),
            name: name,
            boardId: boardId,
            createdAt: DateTime.UtcNow,
            color: color
        );
    }

    // Метод для восстановления из БД
    public static Label Restore(
        Guid id,
        string name,
        Guid boardId,
        string color,
        DateTime createdAt)
    {
        return new Label(id, name, boardId, createdAt, color);
    }

    // Доменные методы
    public void UpdateName(string name)
    {
        if (string.IsNullOrEmpty(name))
            throw new DomainException("Label name cannot be null or empty");

        Name = name;
    }

    public void UpdateColor(string color)
    {
        if (string.IsNullOrEmpty(color) || color.Length != 7 || color[0] != '#')
            throw new DomainException("Invalid color format. Expected #RRGGBB");

        Color = color;
    }

    public void AddTask(Guid taskId)
    {
        if (taskId == Guid.Empty)
            throw new DomainException("Task ID cannot be empty");

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

    public bool HasTask(Guid taskId)
    {
        return _taskIds.Contains(taskId);
    }

    public int GetTaskCount()
    {
        return _taskIds.Count;
    }

    // Валидация цвета
    public static bool IsValidColor(string color)
    {
        if (string.IsNullOrEmpty(color))
            return false;

        if (color.Length != 7 || color[0] != '#')
            return false;

        // Проверяем, что это HEX цвет
        return color.Substring(1).All(c =>
            (c >= '0' && c <= '9') ||
            (c >= 'a' && c <= 'f') ||
            (c >= 'A' && c <= 'F'));
    }

    // Генерация контрастного цвета текста для метки
    public string GetContrastTextColor()
    {
        // Простая логика: для темных цветов - белый текст, для светлых - черный
        if (string.IsNullOrEmpty(Color) || Color.Length != 7)
            return "#000000";

        // Извлекаем компоненты RGB
        var hex = Color.Substring(1);
        var r = Convert.ToInt32(hex.Substring(0, 2), 16);
        var g = Convert.ToInt32(hex.Substring(2, 2), 16);
        var b = Convert.ToInt32(hex.Substring(4, 2), 16);

        // Формула для вычисления яркости
        var luminance = (0.299 * r + 0.587 * g + 0.114 * b) / 255;

        return luminance > 0.5 ? "#000000" : "#FFFFFF";
    }

    // Создание предопределенных меток для новой доски
    public static List<Label> CreateDefaultLabels(Guid boardId)
    {
        return new List<Label>
        {
            Create("Bug", boardId, "#ef4444"),       // Red
            Create("Feature", boardId, "#10b981"),   // Green
            Create("Improvement", boardId, "#3b82f6"), // Blue
            Create("Documentation", boardId, "#8b5cf6"), // Purple
            Create("Urgent", boardId, "#f59e0b")     // Amber
        };
    }

    // Приватный конструктор для EF Core
    private Label() : this(
        Guid.NewGuid(),
        "Untitled Label",
        Guid.NewGuid(),
        DateTime.UtcNow,
        "#6b7280")
    { }
}