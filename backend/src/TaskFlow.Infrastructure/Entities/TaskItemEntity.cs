using TaskFlow.Core.Enums;
using TaskFlow.Infrastructure.Common;

namespace TaskFlow.Infrastructure.Entities;

public class TaskItemEntity : ITimestampedEntity
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int Order { get; set; }
    public Priority Priority { get; set; }
    public DateTime? DueDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Внешние ключи
    public Guid ColumnId { get; set; }
    public Guid? AssignedUserId { get; set; }
    public Guid CreatedById { get; set; }
}
