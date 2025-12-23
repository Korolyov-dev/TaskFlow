using Microsoft.EntityFrameworkCore;
using TaskFlow.Core.Models;
using TaskFlow.Infrastructure.Data;
using TaskFlow.Infrastructure.Entities;

namespace TaskFlow.Infrastructure.Repositories;

public class LabelRepository : BaseRepository<Label, LabelEntity>, ILabelRepository 
{
    public LabelRepository(ApplicationDbContext context):base(context)
    {        
    }

    public async Task<IReadOnlyList<Label>> GetBoardLabelsAsync(Guid boardId, CancellationToken cancellationToken = default)
    {
        var entities = await _dbSet
            .AsNoTracking()
            .Where(l => l.BoardId == boardId)
            .OrderBy(l => l.Name)
            .ToListAsync(cancellationToken);

        return entities.Select(MapToModel).ToList();
    }

    public async Task<Label?> GetByNameAsync(Guid boardId, string name, CancellationToken cancellationToken = default)
    {
        var entity = await _dbSet
            .AsNoTracking()
            .FirstOrDefaultAsync(l => l.BoardId == boardId && l.Name == name, cancellationToken);

        return entity != null ? MapToModel(entity) : null;
    }

    public async Task<bool> NameExistsAsync(Guid boardId, string name, CancellationToken cancellationToken = default)
    {
        return await _dbSet.AnyAsync(l => l.BoardId == boardId && l.Name == name, cancellationToken);
    }

    protected override Label MapToModel(LabelEntity entity)
    {
        return Label.Restore(
            entity.Id,
            entity.Name,
            entity.BoardId,
            entity.Color,
            entity.CreatedAt
        );
    }

    protected override LabelEntity MapToEntity(Label model)
    {
        return new LabelEntity
        {
            Id = model.Id,
            Name = model.Name,
            Color = model.Color,
            BoardId = model.BoardId,
            CreatedAt = model.CreatedAt
        };
    }

    protected override void UpdateEntity(LabelEntity entity, Label model)
    {
        entity.Name = model.Name;
        entity.Color = model.Color;
    }
}
