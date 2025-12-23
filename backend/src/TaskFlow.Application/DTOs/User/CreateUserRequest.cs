namespace TaskFlow.Application.DTOs.User;
public class CreateUserRequest
{
    public string Email { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string? FullName { get; set; }
}