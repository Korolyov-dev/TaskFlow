using TaskFlow.Infrastructure.Common;

namespace TaskFlow.Infrastructure.Entities;
public class ColumnEntity : ICreationTimestampedEntity
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public int Order { get; set; }
    public int? WipLimit { get; set; }
    public DateTime CreatedAt { get; set; }

    // Внешние ключи
    public Guid BoardId { get; set; }
}
