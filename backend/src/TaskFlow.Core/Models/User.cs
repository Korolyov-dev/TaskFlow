// TaskFlow.Core/Models/User.cs
using TaskFlow.Core.Common;

namespace TaskFlow.Core.Models;

public class User
{
    public Guid Id { get; private set; }
    public string Email { get; private set; }
    public string Username { get; private set; }
    public string? FullName { get; private set; }
    public string? AvatarUrl { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    // Навигационные свойства (только Ids для чистоты Core)
    private readonly List<Guid> _boardIds = new();
    public IReadOnlyCollection<Guid> BoardIds => _boardIds.AsReadOnly();

    // Приватный конструктор - принимает уже валидированные данные
    private User(Guid id, string email, string username, DateTime createdAt)
    {
        // Только проверка инвариантов (что не должно нарушаться НИКОГДА)
        if (id == Guid.Empty)
            throw new DomainException("User ID cannot be empty");

        if (string.IsNullOrEmpty(email))
            throw new DomainException("Email is required for existing user");

        if (string.IsNullOrEmpty(username))
            throw new DomainException("Username is required for existing user");

        Id = id;
        Email = email;
        Username = username;
        CreatedAt = createdAt;
        UpdatedAt = null;
    }

    // Фабричный метод - принимает уже валидированные данные
    public static User Create(string email, string username, string? fullName = null)
    {
        // Предусловия - проверяем только то, что НЕЛЬЗЯ проверить в Application
        if (email == null || username == null)
            throw new DomainException("Email and username cannot be null");

        return new User(
            id: Guid.NewGuid(),
            email: email, // Предполагается, что email уже валидирован
            username: username, // Предполагается, что username уже валидирован
            createdAt: DateTime.UtcNow
        )
        {
            FullName = fullName
        };
    }


    // Метод для восстановления из БД
    public static User Restore(
        Guid id,
        string email,
        string username,
        string? fullName,
        string? avatarUrl,
        DateTime createdAt,
        DateTime? updatedAt)
    {
        return new User(id, email, username, createdAt)
        {
            FullName = fullName,
            AvatarUrl = avatarUrl,
            UpdatedAt = updatedAt
        };
    }

    // Доменные методы
    public void UpdateProfile(string? fullName, string? avatarUrl)
    {
        // Проверка инвариантов, если они есть
        FullName = fullName;
        AvatarUrl = avatarUrl;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateUsername(string username)
    {
        // Предусловие - но это скорее инвариант
        if (string.IsNullOrEmpty(username))
            throw new DomainException("Username cannot be null or empty for existing user");

        Username = username;
        UpdatedAt = DateTime.UtcNow;
    }

    public void AddBoard(Guid boardId)
    {
        if (!_boardIds.Contains(boardId))
        {
            _boardIds.Add(boardId);
            UpdatedAt = DateTime.UtcNow;
        }
    }

    public void RemoveBoard(Guid boardId)
    {
        if (_boardIds.Contains(boardId))
        {
            _boardIds.Remove(boardId);
            UpdatedAt = DateTime.UtcNow;
        }
    }

    // Валидационные методы
    private static bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }

    // Приватный конструктор для EF Core
    private User() : this(Guid.NewGuid(), "temp@email.com", "tempuser", DateTime.UtcNow) { }
}