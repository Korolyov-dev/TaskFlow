using TaskFlow.Infrastructure.Common;

namespace TaskFlow.Infrastructure.Entities;

public class ActivityLogEntity : ICreationTimestampedEntity
{
    public Guid Id { get; set; }
    public Guid BoardId { get; set; }
    public Guid? UserId { get; set; }
    public int ActivityType { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }

    public Guid? RelatedTaskId { get; set; }
    public Guid? RelatedColumnId { get; set; }
    public Guid? RelatedUserId { get; set; }
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
}
