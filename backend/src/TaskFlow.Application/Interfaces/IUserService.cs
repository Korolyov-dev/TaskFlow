using TaskFlow.Application.Common;
using TaskFlow.Application.DTOs.User;
using TaskFlow.Application.Services;
using TaskFlow.Core.Models;

namespace TaskFlow.Application.Interfaces
{
    public interface IUserService
    {
        Task<ValidationResult<User>> CreateUserAsync(CreateUserRequest request, CancellationToken cancellationToken = default);
        Task<ValidationResult> DeleteUserAsync(Guid userId, CancellationToken cancellationToken = default);
        Task<ValidationResult<User?>> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default);
        Task<ValidationResult<User?>> GetUserByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<ValidationResult> UpdateUserNameAsync(Guid userId, UpdateUserNameRequest request, CancellationToken cancellationToken = default);
        Task<ValidationResult> UpdateUserProfileAsync(Guid userId, UpdateUserProfileRequest request, CancellationToken cancellationToken = default);
    }
}