using System.ComponentModel.DataAnnotations.Schema;
using TaskFlow.Infrastructure.Common;

namespace TaskFlow.Infrastructure.Entities;

public class AttachmentEntity : ICreationTimestampedEntity
{
    public Guid Id { get; set; }
    public string FileName { get; set; } = string.Empty; 
    public string FileUrl { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string? MimeType { get; set; }
    public DateTime UploadedAt { get; set; }

    // Внешние ключи
    public Guid TaskId { get; set; }
    public Guid UploadedBy { get; set; }
    
    [NotMapped] // Не маппится в БД
    public DateTime CreatedAt
    {
        get => UploadedAt;
        set => UploadedAt = value;
    }
}
