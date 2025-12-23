using Microsoft.EntityFrameworkCore;
using TaskFlow.Core.Interfaces;
using TaskFlow.Infrastructure.Data;
using TaskFlow.Infrastructure.Entities;
using Attachment = TaskFlow.Core.Models.Attachment;

namespace TaskFlow.Infrastructure.Repositories;

public class AttachmentRepository : BaseRepository<Attachment, AttachmentEntity>, IAttachmentRepository
{
    public AttachmentRepository(ApplicationDbContext context) : base(context)
    {        
    }

    public async Task<IReadOnlyList<Attachment>> GetTaskAttachmentsAsync(Guid taskId, CancellationToken cancellationToken = default)
    {
        var entities = await _dbSet
            .AsNoTracking()
            .Where(a => a.TaskId == taskId)
            .OrderByDescending(a => a.UploadedAt)
            .ToListAsync(cancellationToken);

        return entities.Select(MapToModel).ToList();
    }

    public async Task<IReadOnlyList<Attachment>> GetUserAttachmentsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var entities = await _dbSet
            .AsNoTracking()
            .Where(a => a.UploadedBy == userId)
            .OrderByDescending(a => a.UploadedAt)
            .ToListAsync(cancellationToken);

        return entities.Select(MapToModel).ToList();
    }

    public async Task<IReadOnlyList<Attachment>> GetImageAttachmentsAsync(Guid taskId, CancellationToken cancellationToken = default)
    {
        var entities = await _dbSet
            .AsNoTracking()
            .Where(a => a.TaskId == taskId && a.MimeType != null && a.MimeType.StartsWith("image/"))
            .OrderBy(a => a.UploadedAt)
            .ToListAsync(cancellationToken);

        return entities.Select(MapToModel).ToList();
    }

    public async Task<IReadOnlyList<Attachment>> GetDocumentAttachmentsAsync(Guid taskId, CancellationToken cancellationToken = default)
    {
        var documentMimeTypes = new[]
        {
            "application/pdf",
            "application/msword",
            "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            "application/vnd.ms-excel",
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "text/plain"
        };

        var entities = await _dbSet
            .AsNoTracking()
            .Where(a => a.TaskId == taskId && a.MimeType != null && documentMimeTypes.Contains(a.MimeType))
            .OrderBy(a => a.UploadedAt)
            .ToListAsync(cancellationToken);

        return entities.Select(MapToModel).ToList();
    }

    public async Task<long> GetTotalAttachmentSizeAsync(Guid taskId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(a => a.TaskId == taskId)
            .SumAsync(a => a.FileSize, cancellationToken);
    }

    public async Task<bool> HasAttachmentsAsync(Guid taskId, CancellationToken cancellationToken = default)
    {
        return await _dbSet.AnyAsync(a => a.TaskId == taskId, cancellationToken);
    }

    protected override Attachment MapToModel(AttachmentEntity entity)
    {
        return Attachment.Restore(
            entity.Id,
            entity.FileName,
            entity.FileUrl,
            entity.FileSize,
            entity.MimeType,
            entity.TaskId,
            entity.UploadedBy,
            entity.UploadedAt
        );
    }

    protected override AttachmentEntity MapToEntity(Attachment model)
    {
        return new AttachmentEntity
        {
            Id = model.Id,
            FileName = model.FileName,
            FileUrl = model.FileUrl,
            FileSize = model.FileSize,
            MimeType = model.MimeType,
            TaskId = model.TaskId,
            UploadedBy = model.UploadedBy,
            UploadedAt = model.UploadedAt
        };
    }

    protected override void UpdateEntity(AttachmentEntity entity, Attachment model)
    {
        entity.FileName = model.FileName;
        entity.FileUrl = model.FileUrl;
        entity.MimeType = model.MimeType;
    }
}
