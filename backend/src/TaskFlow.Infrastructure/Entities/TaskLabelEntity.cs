namespace TaskFlow.Infrastructure.Entities;

public class TaskLabelEntity
{
    public Guid TaskId { get; set; }
    public Guid LabelId { get; set; }
    public DateTime AssignedAt { get; set; }
}
