using TaskFlow.Core.Interfaces;
using TaskFlow.Core.Models;

public interface IColumnRepository : IRepository<Column>
{
    Task<IReadOnlyList<Column>> GetBoardColumnsAsync(Guid boardId, CancellationToken cancellationToken = default);
    Task<Column?> GetColumnWithTasksAsync(Guid columnId, CancellationToken cancellationToken = default);
    Task ReorderColumnsAsync(Guid boardId, List<Guid> columnIdsInOrder, CancellationToken cancellationToken = default);
    Task<int> GetNextOrderAsync(Guid boardId, CancellationToken cancellationToken = default);
}