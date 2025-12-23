using Attachment = TaskFlow.Core.Models.Attachment;

namespace TaskFlow.Core.Interfaces;

public interface IAttachmentRepository :IRepository<Attachment>
{    Task<IReadOnlyList<Attachment>> GetTaskAttachmentsAsync(Guid taskId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Attachment>> GetUserAttachmentsAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Attachment>> GetImageAttachmentsAsync(Guid taskId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Attachment>> GetDocumentAttachmentsAsync(Guid taskId, CancellationToken cancellationToken = default);
    Task<long> GetTotalAttachmentSizeAsync(Guid taskId, CancellationToken cancellationToken = default);
    Task<bool> HasAttachmentsAsync(Guid taskId, CancellationToken cancellationToken = default);
}
