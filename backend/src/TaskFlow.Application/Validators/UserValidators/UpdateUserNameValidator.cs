using FluentValidation;
using TaskFlow.Application.Common;
using TaskFlow.Application.DTOs.User;
using TaskFlow.Core.Interfaces;

namespace TaskFlow.Application.Validators.UserValidators;

public class UpdateUserNameValidator : BaseValidator<UpdateUserNameRequest>
{
    private readonly IUserRepository _userRepository;
    private readonly Guid _userId;
    public UpdateUserNameValidator(IUserRepository userRepository, Guid userId)
    {
        _userRepository = userRepository;
        _userId = userId;

        RuleFor(x => x.UserName)
           .NotEmpty().WithMessage("User name is required")
           .MinimumLength(3).WithMessage("User name must be at least 3 characters")
           .MaximumLength(50).WithMessage("User name cannot exceed 50 characters")
           .Matches("^[a-zA-Z0-9_]+$").WithMessage("User name can only contain letters, numbers and underscores")
           .MustAsync(BeUniqueUserNameForUser).WithMessage("User name is already taken");
    }

    private async Task<bool> BeUniqueUserNameForUser(string userName, CancellationToken cancellationToken)
    {
        var existingUser = await _userRepository.GetByUserNameAsync(userName, cancellationToken);
        return existingUser == null || existingUser.Id == _userId;
    }
}
