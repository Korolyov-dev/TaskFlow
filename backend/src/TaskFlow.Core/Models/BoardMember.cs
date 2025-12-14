using TaskFlow.Core.Common;

namespace TaskFlow.Core.Models;

public class BoardMember
{
    public Guid BoardId { get; private set; }
    public Guid UserId { get; private set; }
    public BoardRole Role { get; private set; }
    public DateTime JoinedAt { get; private set; }

    // Навигационные свойства (только Ids)
    public string? UserName { get; private set; }
    public string? UserEmail { get; private set; }
    public string? UserAvatarUrl { get; private set; }

    private BoardMember(Guid boardId, Guid userId, BoardRole role, DateTime joinedAt)
    {
        if (boardId == Guid.Empty)
            throw new DomainException("Board ID cannot be empty");

        if (userId == Guid.Empty)
            throw new DomainException("User ID cannot be empty");

        BoardId = boardId;
        UserId = userId;
        Role = role;
        JoinedAt = joinedAt;
    }

    public static BoardMember Create(Guid boardId, Guid userId, BoardRole role = BoardRole.Member)
    {
        return new BoardMember(
            boardId: boardId,
            userId: userId,
            role: role,
            joinedAt: DateTime.UtcNow
        );
    }

    public static BoardMember Restore(
        Guid boardId,
        Guid userId,
        BoardRole role,
        DateTime joinedAt,
        string? UserName = null,
        string? userEmail = null,
        string? userAvatarUrl = null)
    {
        return new BoardMember(boardId, userId, role, joinedAt)
        {
            UserName = UserName,
            UserEmail = userEmail,
            UserAvatarUrl = userAvatarUrl
        };
    }

    public void ChangeRole(BoardRole newRole)
    {
        Role = newRole;
    }

    // Приватный конструктор для EF Core
    private BoardMember() : this(Guid.NewGuid(), Guid.NewGuid(), BoardRole.Member, DateTime.UtcNow) { }
}