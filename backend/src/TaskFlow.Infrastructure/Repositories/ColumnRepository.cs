using Microsoft.EntityFrameworkCore;
using TaskFlow.Core.Models;
using TaskFlow.Infrastructure.Data;
using TaskFlow.Infrastructure.Entities;
using TaskFlow.Infrastructure.Repositories;

public class ColumnRepository : BaseRepository<Column, ColumnEntity>, IColumnRepository
{
   // private readonly ApplicationDbContext _context;

    public ColumnRepository(ApplicationDbContext context) : base(context)
    {
     //   _context = context;
    }

    public async Task<IReadOnlyList<Column>> GetBoardColumnsAsync(Guid boardId, CancellationToken cancellationToken = default)
    {
        var entities = await _dbSet
            .AsNoTracking()
            .Where(c => c.BoardId == boardId)
            .OrderBy(c => c.Order)
            .ToListAsync(cancellationToken);

        return entities.Select(MapToModel).ToList();
    }

    public async Task<Column?> GetColumnWithTasksAsync(Guid columnId, CancellationToken cancellationToken = default)
    {
        var columnEntity = await _dbSet
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == columnId, cancellationToken);

        if (columnEntity == null)
            return null;

        var column = MapToModel(columnEntity);

        // Получаем задачи колонки
        var tasks = await _context.Tasks
            .Where(t => t.ColumnId == columnId)
            .OrderBy(t => t.Order)
            .ToListAsync(cancellationToken);

        foreach (var taskEntity in tasks)
        {
            // Здесь нужно преобразовать TaskEntity в TaskItem
            // Для этого нужен маппинг или отдельный метод
        }

        return column;
    }

    public async Task ReorderColumnsAsync(Guid boardId, List<Guid> columnIdsInOrder, CancellationToken cancellationToken = default)
    {
        var columns = await _dbSet
            .Where(c => c.BoardId == boardId && columnIdsInOrder.Contains(c.Id))
            .ToListAsync(cancellationToken);

        var order = 0;
        foreach (var columnId in columnIdsInOrder)
        {
            var column = columns.FirstOrDefault(c => c.Id == columnId);
            if (column != null)
            {
                column.Order = order++;
            }
        }

        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<int> GetNextOrderAsync(Guid boardId, CancellationToken cancellationToken = default)
    {
        var maxOrder = await _dbSet
            .Where(c => c.BoardId == boardId)
            .MaxAsync(c => (int?)c.Order, cancellationToken);

        return (maxOrder ?? -1) + 1;
    }

    protected override Column MapToModel(ColumnEntity entity)
    {
        return Column.Restore(
            entity.Id,
            entity.Title,
            entity.Order,
            entity.BoardId,
            entity.WipLimit,
            entity.CreatedAt
        );
    }

    protected override ColumnEntity MapToEntity(Column model)
    {
        return new ColumnEntity
        {
            Id = model.Id,
            Title = model.Title,
            Order = model.Order,
            BoardId = model.BoardId,
            WipLimit = model.WipLimit,
            CreatedAt = model.CreatedAt
        };
    }

    protected override void UpdateEntity(ColumnEntity entity, Column model)
    {
        entity.Title = model.Title;
        entity.Order = model.Order;
        entity.WipLimit = model.WipLimit;
    }
}