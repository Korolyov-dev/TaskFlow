using TaskFlow.Infrastructure.Common;

namespace TaskFlow.Infrastructure.Entities;

public class UserEntity : ITimestampedEntity
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string? FullName { get; set; } 
    public string? AvatarUrl { get; set; } 
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
