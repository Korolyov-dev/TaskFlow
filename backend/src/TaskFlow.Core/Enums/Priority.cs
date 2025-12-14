// TaskFlow.Core/Enums/Priority.cs
namespace TaskFlow.Core.Enums;

/// <summary>
/// Приоритет задачи с цветовыми кодами для UI
/// </summary>
public enum Priority
{
    /// <summary>
    /// Низкий приоритет (зеленый)
    /// </summary>
    Low = 0,

    /// <summary>
    /// Средний приоритет (синий)
    /// </summary>
    Medium = 1,

    /// <summary>
    /// Высокий приоритет (оранжевый/желтый)
    /// </summary>
    High = 2,

    /// <summary>
    /// Критический приоритет (красный)
    /// </summary>
    Critical = 3
}

// Расширения для Priority Enum
public static class PriorityExtensions
{
    /// <summary>
    /// Получить цвет для отображения приоритета в UI
    /// </summary>
    public static string GetColor(this Priority priority)
    {
        return priority switch
        {
            Priority.Low => "#10b981",      // Green
            Priority.Medium => "#3b82f6",   // Blue
            Priority.High => "#f59e0b",     // Amber/Orange
            Priority.Critical => "#ef4444", // Red
            _ => "#6b7280"                  // Gray (default)
        };
    }

    /// <summary>
    /// Получить иконку для приоритета
    /// </summary>
    public static string GetIcon(this Priority priority)
    {
        return priority switch
        {
            Priority.Low => "arrow-down",
            Priority.Medium => "minus",
            Priority.High => "arrow-up",
            Priority.Critical => "alert-triangle",
            _ => "circle"
        };
    }

    /// <summary>
    /// Получить текстовое представление приоритета
    /// </summary>
    public static string GetDisplayName(this Priority priority)
    {
        return priority switch
        {
            Priority.Low => "Низкий",
            Priority.Medium => "Средний",
            Priority.High => "Высокий",
            Priority.Critical => "Критический",
            _ => "Не указан"
        };
    }

    /// <summary>
    /// Конвертировать числовое значение в Priority
    /// </summary>
    public static Priority FromValue(int value)
    {
        return Enum.IsDefined(typeof(Priority), value)
            ? (Priority)value
            : Priority.Medium;
    }

    /// <summary>
    /// Получить все доступные приоритеты для селекта
    /// </summary>
    public static IEnumerable<KeyValuePair<int, string>> GetOptions()
    {
        return Enum.GetValues(typeof(Priority))
            .Cast<Priority>()
            .Select(p => new KeyValuePair<int, string>((int)p, p.GetDisplayName()));
    }

    /// <summary>
    /// Проверить, является ли приоритет высоким или критическим
    /// </summary>
    public static bool IsHighOrCritical(this Priority priority)
    {
        return priority == Priority.High || priority == Priority.Critical;
    }
}