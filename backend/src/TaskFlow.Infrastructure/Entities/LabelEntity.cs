using TaskFlow.Infrastructure.Common;

namespace TaskFlow.Infrastructure.Entities;

public class LabelEntity : ICreationTimestampedEntity
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }

    // Внешние ключи
    public Guid BoardId { get; set; }
}
