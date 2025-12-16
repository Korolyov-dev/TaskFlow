namespace TaskFlow.Infrastructure.Common;

public interface IUpdateTimestampedEntity
{
    DateTime? UpdatedAt { get; set; }
}