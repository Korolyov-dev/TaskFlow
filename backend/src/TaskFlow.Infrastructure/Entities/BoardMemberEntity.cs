namespace TaskFlow.Infrastructure.Entities;

public class BoardMemberEntity
{
    public Guid BoardId { get; set; }
    public Guid UserId { get; set; }
    public BoardRole Role { get; set; }
    public DateTime JoinedAt { get; set; }
}
