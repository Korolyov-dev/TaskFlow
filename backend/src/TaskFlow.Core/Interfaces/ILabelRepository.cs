using TaskFlow.Core.Interfaces;
using TaskFlow.Core.Models;

public interface ILabelRepository : IRepository<Label>
{
    Task<IReadOnlyList<Label>> GetBoardLabelsAsync(Guid boardId, CancellationToken cancellationToken = default);
    Task<Label?> GetByNameAsync(Guid boardId, string name, CancellationToken cancellationToken = default);
    Task<bool> NameExistsAsync(Guid boardId, string name, CancellationToken cancellationToken = default);
}