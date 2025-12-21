using Microsoft.EntityFrameworkCore;
using TaskFlow.Core.Interfaces;
using TaskFlow.Core.Models;
using TaskFlow.Infrastructure.Data;
using TaskFlow.Infrastructure.Entities;

namespace TaskFlow.Infrastructure.Repositories;

public class BoardRepository : BaseRepository<Board, BoardEntity>, IBoardRepository
{
    public BoardRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<Board?> GetBoardWithDetailsAsync(Guid boardId, CancellationToken cancellationToken = default)
    {
        // 1. Получаем доску
        var boardEntity = await _dbSet
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.Id == boardId, cancellationToken);

        if (boardEntity == null)
            return null;

        var board = MapToModel(boardEntity);

        // 2. Получаем колонки доски через JOIN
        var columns = await _context.Columns
            .Where(c => c.BoardId == boardId)
            .OrderBy(c => c.Order)
            .ToListAsync(cancellationToken);

        // 3. Получаем задачи для каждой колонки
        foreach (var columnEntity in columns)
        {
            var column = MapToColumnModel(columnEntity);
            board.AddColumn(column.Id);

            // Можно загрузить задачи здесь или в отдельном методе
            // Для производительности лучше отдельным запросом
        }

        // 4. Получаем участников доски
        var members = await GetBoardMembersAsync(boardId, cancellationToken);
        foreach (var member in members)
        {
            board.AddMember(member);
        }

        // 5. Получаем метки доски
        var labels = await _context.Labels
            .Where(l => l.BoardId == boardId)
            .ToListAsync(cancellationToken);

        foreach (var labelEntity in labels)
        {
            board.AddLabel(labelEntity.Id);
        }

        return board;
    }

    private async Task<List<Guid>> GetBoardMembersAsync(Guid boardId, CancellationToken cancellationToken)
    {
        return await _context.BoardMembers
            .Where(bm => bm.BoardId == boardId)
            .Select(bm => bm.UserId)
            .Union(
                _context.Boards
                    .Where(b => b.Id == boardId)
                    .Select(b => b.OwnerId)
            )
            .Distinct()
            .ToListAsync(cancellationToken);
    }

    private Column MapToColumnModel(ColumnEntity entity)
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


    public async Task<IReadOnlyList<Board>> GetUserBoardsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        // Доски, где пользователь владелец или участник
        var boardIds = await _context.BoardMembers
            .Where(bm => bm.UserId == userId)
            .Select(bm => bm.BoardId)
            .Union(
                _context.Boards
                    .Where(b => b.OwnerId == userId)
                    .Select(b => b.Id)
            )
            .Distinct()
            .ToListAsync(cancellationToken);

        var boardEntities = await _dbSet
            .AsNoTracking()
            .Where(b => boardIds.Contains(b.Id))
            .OrderByDescending(b => b.IsFavorite)
            .ThenByDescending(b => b.CreatedAt)
            .ToListAsync(cancellationToken);

        return boardEntities.Select(MapToModel).ToList();
    }

    public async Task<IReadOnlyList<Board>> GetUserFavoriteBoardsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var boardIds = await _context.BoardMembers
            .Where(bm => bm.UserId == userId)
            .Select(bm => bm.BoardId)
            .Union(
                _context.Boards
                    .Where(b => b.OwnerId == userId)
                    .Select(b => b.Id)
            )
            .Distinct()
            .ToListAsync(cancellationToken);

        var boardEntities = await _dbSet
            .AsNoTracking()
            .Where(b => boardIds.Contains(b.Id) && b.IsFavorite)
            .OrderByDescending(b => b.UpdatedAt)
            .ToListAsync(cancellationToken);

        return boardEntities.Select(MapToModel).ToList();
    }

    public async Task<bool> IsUserBoardMemberAsync(Guid boardId, Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.BoardMembers
            .AnyAsync(bm => bm.BoardId == boardId && bm.UserId == userId, cancellationToken) ||
               await _context.Boards
                   .AnyAsync(b => b.Id == boardId && b.OwnerId == userId, cancellationToken);
    }

    public async Task<bool> IsUserBoardOwnerAsync(Guid boardId, Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.Boards
            .AnyAsync(b => b.Id == boardId && b.OwnerId == userId, cancellationToken);
    }

    public async Task AddMemberAsync(Guid boardId, Guid userId, BoardRole role, CancellationToken cancellationToken = default)
    {
        var boardMember = new BoardMemberEntity
        {
            BoardId = boardId,
            UserId = userId,
            Role = role,
            JoinedAt = DateTime.UtcNow
        };

        await _context.BoardMembers.AddAsync(boardMember, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task RemoveMemberAsync(Guid boardId, Guid userId, CancellationToken cancellationToken = default)
    {
        var boardMember = await _context.BoardMembers
            .FirstOrDefaultAsync(bm => bm.BoardId == boardId && bm.UserId == userId, cancellationToken);

        if (boardMember != null)
        {
            _context.BoardMembers.Remove(boardMember);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    protected override Board MapToModel(BoardEntity entity)
    {
        return Board.Restore(
            entity.Id,
            entity.Title,
            entity.Description,
            entity.Color,
            entity.IsFavorite,
            entity.OwnerId,
            entity.CreatedAt,
            entity.UpdatedAt
        );
    }

    protected override BoardEntity MapToEntity(Board model)
    {
        return new BoardEntity
        {
            Id = model.Id,
            Title = model.Title,
            Description = model.Description,
            Color = model.Color,
            IsFavorite = model.IsFavorite,
            OwnerId = model.OwnerId,
            CreatedAt = model.CreatedAt,
            UpdatedAt = model.UpdatedAt
        };
    }

    protected override void UpdateEntity(BoardEntity entity, Board model)
    {
        entity.Title = model.Title;
        entity.Description = model.Description;
        entity.Color = model.Color;
        entity.IsFavorite = model.IsFavorite;
        entity.UpdatedAt = model.UpdatedAt;
    }
}