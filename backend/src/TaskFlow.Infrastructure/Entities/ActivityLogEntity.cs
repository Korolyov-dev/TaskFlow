using TaskFlow.Infrastructure.Common;

namespace TaskFlow.Infrastructure.Entities;

public class ActivityLogEntity : ICreationTimestampedEntity
{
    public Guid Id { get; set; }
    public Guid BoardId { get; set; }
    public Guid? UserId { get; set; } // Может быть null для системных событий
    public ActivityType ActivityType { get; set; }
    public string Description { get; set; } = string.Empty;
    public Dictionary<string, object> Metadata { get; set; }
    public DateTime CreatedAt { get; set; }
}
