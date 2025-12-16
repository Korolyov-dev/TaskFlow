namespace TaskFlow.Infrastructure.Common;

public interface ITimestampedEntity
{
    DateTime CreatedAt { get; set; }
    DateTime? UpdatedAt { get; set; }
}