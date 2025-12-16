namespace TaskFlow.Infrastructure.Common;

public interface ICreationTimestampedEntity
{
    DateTime CreatedAt { get; set; }
}