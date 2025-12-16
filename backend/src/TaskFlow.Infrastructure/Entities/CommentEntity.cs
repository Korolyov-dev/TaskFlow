using TaskFlow.Infrastructure.Common;

namespace TaskFlow.Infrastructure.Entities;

public class CommentEntity : ITimestampedEntity
{
    public Guid Id { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }

    // Внешние ключи
    public Guid TaskId { get; set; }
    public Guid UserId { get; set; }

}
