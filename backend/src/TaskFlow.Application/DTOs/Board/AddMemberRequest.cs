
namespace TaskFlow.Application.DTOs.Board;

public class AddMemberRequest
{
    public Guid UserId { get; set; }
    public BoardRole Role { get; set; } = BoardRole.Member;
    
}
