using Microsoft.EntityFrameworkCore;
using TaskFlow.Core.Interfaces;
using TaskFlow.Infrastructure.Data;

namespace TaskFlow.Infrastructure.Repositories;

public abstract class BaseRepository<TModel, TEntity> : IRepository<TModel>
    where TModel : class
    where TEntity : class
{
    protected readonly ApplicationDbContext _context;
    protected readonly DbSet<TEntity> _dbSet;

    protected BaseRepository(ApplicationDbContext context)
    {
        _context = context;
        _dbSet = context.Set<TEntity>();
    }

    // Абстрактные методы для маппинга (должны быть реализованы в наследниках)
    protected abstract TModel MapToModel(TEntity entity);
    protected abstract TEntity MapToEntity(TModel model);
    protected abstract void UpdateEntity(TEntity entity, TModel model);

    public virtual async Task<TModel?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _dbSet.FindAsync(new object[] { id }, cancellationToken);
        return entity != null ? MapToModel(entity) : null;
    }

    public virtual async Task<IReadOnlyList<TModel>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var entities = await _dbSet.AsNoTracking().ToListAsync(cancellationToken);
        return entities.Select(MapToModel).ToList();
    }

    public virtual async Task<TModel> AddAsync(TModel model, CancellationToken cancellationToken = default)
    {
        var entity = MapToEntity(model);
        await _dbSet.AddAsync(entity, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return MapToModel(entity);
    }

    public virtual async Task UpdateAsync(TModel model, CancellationToken cancellationToken = default)
    {
        var entity = await _dbSet.FindAsync(new object[] { GetEntityId(model) }, cancellationToken);
        if (entity != null)
        {
            UpdateEntity(entity, model);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public virtual async Task DeleteAsync(TModel model, CancellationToken cancellationToken = default)
    {
        var entity = await _dbSet.FindAsync(new object[] { GetEntityId(model) }, cancellationToken);
        if (entity != null)
        {
            _dbSet.Remove(entity);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public virtual async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet.AnyAsync(e => EF.Property<Guid>(e, "Id") == id, cancellationToken);
    }

    // Вспомогательный метод для получения ID сущности
    private Guid GetEntityId(TModel model)
    {
        var property = model.GetType().GetProperty("Id");
        if (property == null)
            throw new InvalidOperationException($"Model {typeof(TModel).Name} must have an Id property");

        return (Guid)property.GetValue(model)!;
    }
}