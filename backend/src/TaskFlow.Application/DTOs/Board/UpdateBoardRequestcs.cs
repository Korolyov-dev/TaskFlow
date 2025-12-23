
namespace TaskFlow.Application.DTOs.Board;

public class UpdateBoardRequest
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Color { get; set; } = "#4f46e5";
}