// TaskFlow.Application/Services/BoardService.cs
using TaskFlow.Application.Common;
using TaskFlow.Application.DTOs.Board;
using TaskFlow.Application.Services;
using TaskFlow.Core.Models;

namespace TaskFlow.Application.Interfaces
{
    public interface IBoardService
    {
        Task<ValidationResult> AddMemberAsync(Guid boardId, AddMemberRequest request, CancellationToken cancellationToken = default);
        Task<ValidationResult<Board>> CreateBoardAsync(Guid ownerId, CreateBoardRequest request, CancellationToken cancellationToken = default);
        Task<ValidationResult<Board?>> GetBoardByIdAsync(Guid boardId, CancellationToken cancellationToken = default);
        Task<ValidationResult<Board?>> GetBoardWithDetailsAsync(Guid boardId, CancellationToken cancellationToken = default);
        Task<ValidationResult<IReadOnlyList<Board>>> GetUserBoardsAsync(Guid userId, CancellationToken cancellationToken = default);
        Task<ValidationResult> UpdateBoardAsync(Guid boardId, UpdateBoardRequest request, CancellationToken cancellationToken = default);
    }
}