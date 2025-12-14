// TaskFlow.Core/Models/User.cs
using TaskFlow.Core.Common;

namespace TaskFlow.Core.Models;

public class User
{
    public Guid Id { get; private set; }
    public string Email { get; private set; }
    public string UserName { get; private set; }
    public string? FullName { get; private set; }
    public string? AvatarUrl { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    

    // Приватный конструктор
    private User(Guid id, string email, string UserName, DateTime createdAt)
    {
        if (id == Guid.Empty)
            throw new DomainException("User ID cannot be empty");

        if (string.IsNullOrEmpty(email))
            throw new DomainException("Email is required for existing user");

        if (string.IsNullOrEmpty(UserName))
            throw new DomainException("UserName is required for existing user");

        Id = id;
        Email = email;
        UserName = UserName;
        CreatedAt = createdAt;
        UpdatedAt = null;
    }

    // Фабричный метод
    public static User Create(string email, string UserName, string? fullName = null)
    {
        if (email == null || UserName == null)
            throw new DomainException("Email and UserName cannot be null");

        return new User(
            id: Guid.NewGuid(),
            email: email,
            UserName: UserName,
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
        string UserName,
        string? fullName,
        string? avatarUrl,
        DateTime createdAt,
        DateTime? updatedAt)
    {
        return new User(id, email, UserName, createdAt)
        {
            FullName = fullName,
            AvatarUrl = avatarUrl,
            UpdatedAt = updatedAt
        };
    }

    // Доменные методы
    public void UpdateProfile(string? fullName, string? avatarUrl)
    {
        FullName = fullName;
        AvatarUrl = avatarUrl;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateUserName(string UserName)
    {
        if (string.IsNullOrEmpty(UserName))
            throw new DomainException("UserName cannot be null or empty for existing user");

        UserName = UserName;
        UpdatedAt = DateTime.UtcNow;
    }

    // Приватный конструктор для EF Core
    private User() : this(Guid.NewGuid(), "temp@email.com", "tempuser", DateTime.UtcNow) { }
}