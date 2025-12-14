
namespace TaskFlow.Infrastructure.Entities;

public class UserEntity
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string? FullName { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Навигационные свойства
    public virtual ICollection<BoardMemberEntity> BoardMemberships { get; set; } = new List<BoardMemberEntity>();
    public virtual ICollection<BoardEntity> OwnedBoards { get; set; } = new List<BoardEntity>();
    public virtual ICollection<TaskEntity> CreatedTasks { get; set; } = new List<TaskEntity>();
    public virtual ICollection<TaskEntity> AssignedTasks { get; set; } = new List<TaskEntity>();
    public virtual ICollection<CommentEntity> Comments { get; set; } = new List<CommentEntity>();
    public virtual ICollection<AttachmentEntity> Attachments { get; set; } = new List<AttachmentEntity>();
}
