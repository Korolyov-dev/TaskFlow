using Microsoft.EntityFrameworkCore;
using TaskFlow.Core.Interfaces;
using TaskFlow.Core.Models;
using TaskFlow.Infrastructure.Data;
using TaskFlow.Infrastructure.Entities;

namespace TaskFlow.Infrastructure.Repositories;

public class UserRepository : BaseRepository<User, UserEntity>, IUserRepository
{
    public UserRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var entity = await _dbSet
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);

        return entity != null ? MapToModel(entity) : null;
    }

    public async Task<User?> GetByUserNameAsync(string userName, CancellationToken cancellationToken = default)
    {
        var entity = await _dbSet
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.UserName == userName, cancellationToken);

        return entity != null ? MapToModel(entity) : null;
    }

    public async Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _dbSet.AnyAsync(u => u.Email == email, cancellationToken);
    }

    public async Task<bool> UsernameExistsAsync(string userName, CancellationToken cancellationToken = default)
    {
        return await _dbSet.AnyAsync(u => u.UserName == userName, cancellationToken);
    }

    protected override User MapToModel(UserEntity entity)
    {
        return User.Restore(
            entity.Id,
            entity.Email,
            entity.UserName,
            entity.FullName,
            entity.AvatarUrl,
            entity.CreatedAt,
            entity.UpdatedAt
        );
    }

    protected override UserEntity MapToEntity(User model)
    {
        return new UserEntity
        {
            Id = model.Id,
            Email = model.Email,
            UserName = model.UserName,
            FullName = model.FullName,
            AvatarUrl = model.AvatarUrl,
            CreatedAt = model.CreatedAt,
            UpdatedAt = model.UpdatedAt
        };
    }

    protected override void UpdateEntity(UserEntity entity, User model)
    {
        entity.Email = model.Email;
        entity.UserName = model.UserName;
        entity.FullName = model.FullName;
        entity.AvatarUrl = model.AvatarUrl;
        entity.UpdatedAt = model.UpdatedAt;
    }
}