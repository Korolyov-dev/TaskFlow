// TaskFlow.Core/Models/Comment.cs
using TaskFlow.Core.Common;

namespace TaskFlow.Core.Models;

public class Comment
{
    public Guid Id { get; private set; }
    public string Content { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public DateTime? DeletedAt { get; private set; }

    // Внешние ключи
    public Guid TaskId { get; private set; }
    public Guid UserId { get; private set; }

    // Навигационные свойства (только для удобства, денормализация)
    public string? UserName { get; private set; }
    public string? UserAvatarUrl { get; private set; }

    // Приватный конструктор
    private Comment(Guid id, string content, Guid taskId, Guid userId, DateTime createdAt)
    {
        // Инварианты
        if (id == Guid.Empty)
            throw new DomainException("Comment ID cannot be empty");

        if (string.IsNullOrEmpty(content))
            throw new DomainException("Comment content is required for existing comment");

        if (taskId == Guid.Empty)
            throw new DomainException("Comment must belong to a task");

        if (userId == Guid.Empty)
            throw new DomainException("Comment must have an author");

        Id = id;
        Content = content;
        TaskId = taskId;
        UserId = userId;
        CreatedAt = createdAt;
        UpdatedAt = null;
        DeletedAt = null;
    }

    // Фабричный метод создания комментария
    public static Comment Create(string content, Guid taskId, Guid userId)
    {
        // Предполагается, что content уже валидирован в Application слое
        // Проверка минимальной и максимальной длины контента

        return new Comment(
            id: Guid.NewGuid(),
            content: content,
            taskId: taskId,
            userId: userId,
            createdAt: DateTime.UtcNow
        );
    }

    // Метод для восстановления из БД
    public static Comment Restore(
        Guid id,
        string content,
        Guid taskId,
        Guid userId,
        DateTime createdAt,
        DateTime? updatedAt,
        DateTime? deletedAt,
        string? userName = null,
        string? userAvatarUrl = null)
    {
        return new Comment(id, content, taskId, userId, createdAt)
        {
            UpdatedAt = updatedAt,
            DeletedAt = deletedAt,
            UserName = userName,
            UserAvatarUrl = userAvatarUrl
        };
    }

    // Доменные методы
    public void UpdateContent(string newContent)
    {
        if (string.IsNullOrEmpty(newContent))
            throw new DomainException("Comment content cannot be null or empty");

        if (IsDeleted)
            throw new DomainException("Cannot update deleted comment");

        Content = newContent;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Delete()
    {
        if (DeletedAt == null)
        {
            DeletedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }
    }

    public void RestoreComment()
    {
        if (DeletedAt != null)
        {
            DeletedAt = null;
            UpdatedAt = DateTime.UtcNow;
        }
    }

    // Вспомогательные свойства
    public bool IsDeleted => DeletedAt != null;

    public bool IsEdited => UpdatedAt != null && UpdatedAt > CreatedAt;

    public TimeSpan Age => DateTime.UtcNow - CreatedAt;

    public bool CanBeEditedBy(Guid userId)
    {
        // Комментарий можно редактировать только автору
        // И только в течение определенного времени (например, 15 минут)
        // Это бизнес-правило может быть дополнено в Application слое

        if (userId == Guid.Empty)
            return false;

        if (IsDeleted)
            return false;

        if (userId != UserId)
            return false;

        // Позволяем редактировать только первые 15 минут
        var timeSinceCreation = DateTime.UtcNow - CreatedAt;
        return timeSinceCreation <= TimeSpan.FromMinutes(15);
    }

    public bool CanBeDeletedBy(Guid userId)
    {
        // Комментарий можно удалить автору или администратору доски
        // (Права администратора будут проверяться в Application слое)

        if (userId == Guid.Empty)
            return false;

        if (IsDeleted)
            return false;

        // Автор всегда может удалить свой комментарий
        return userId == UserId;
    }

    // Методы форматирования для UI
    public string GetRelativeTime()
    {
        var timeSpan = Age;

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

    private static string Pluralize(int number, string one, string two, string five)
    {
        var n = Math.Abs(number) % 100;
        var n1 = n % 10;

        if (n > 10 && n < 20) return five;
        if (n1 > 1 && n1 < 5) return two;
        if (n1 == 1) return one;
        return five;
    }

    // Методы для редактирования с отметкой о редактировании
    public void EditWithMark(string newContent)
    {
        UpdateContent($"{newContent}\n\n_(изменено)_");
    }

    // Приватный конструктор для EF Core
    private Comment() : this(
        Guid.NewGuid(),
        "Empty comment",
        Guid.NewGuid(),
        Guid.NewGuid(),
        DateTime.UtcNow)
    { }
}

// Расширения для работы с комментариями
public static class CommentExtensions
{
    public static IEnumerable<Comment> ActiveComments(this IEnumerable<Comment> comments)
    {
        return comments.Where(c => !c.IsDeleted);
    }

    public static IEnumerable<Comment> DeletedComments(this IEnumerable<Comment> comments)
    {
        return comments.Where(c => c.IsDeleted);
    }

    public static IEnumerable<Comment> CommentsByUser(this IEnumerable<Comment> comments, Guid userId)
    {
        return comments.Where(c => c.UserId == userId);
    }

    public static Comment? FindComment(this IEnumerable<Comment> comments, Guid commentId)
    {
        return comments.FirstOrDefault(c => c.Id == commentId);
    }

    public static bool HasCommentsFromUser(this IEnumerable<Comment> comments, Guid userId)
    {
        return comments.Any(c => c.UserId == userId && !c.IsDeleted);
    }
}