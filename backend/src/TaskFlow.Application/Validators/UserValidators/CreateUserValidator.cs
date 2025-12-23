
using FluentValidation;
using TaskFlow.Application.Common;
using TaskFlow.Application.DTOs.User;
using TaskFlow.Core.Interfaces;

namespace TaskFlow.Application.Validators.UserValidators;

public class CreateUserValidator:BaseValidator<CreateUserRequest>
{
    private readonly IUserRepository _userRepository;
    public CreateUserValidator(IUserRepository userRepository)
    {
        _userRepository = userRepository;

        RuleFor(x =>x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format")
            .MaximumLength(255).WithMessage("Email cannot exceed 255 characters")
            .MustAsync(BeUniqueEmail).WithMessage("Email is already taken");

        RuleFor(x => x.UserName)
            .NotEmpty().WithMessage("User name is required")
            .MinimumLength(3).WithMessage("User name must be at least 3 characters")
            .MaximumLength(50).WithMessage("User name cannot exceed 50 characters")
            .Matches("^[a-zA-Z0-9_]+$").WithMessage("User name can only contain letters, numbers and underscores")
            .MustAsync(BeUniqueUsername).WithMessage("User name is already taken");

        RuleFor(x => x.FullName)
            .MaximumLength(255).WithMessage("Full name cannot exceed 255 characters")
            .When(x => !string.IsNullOrEmpty(x.FullName));
    }

    private async Task<bool> BeUniqueEmail(string email, CancellationToken cancellationToken)
    {
        return !await _userRepository.EmailExistsAsync(email, cancellationToken);
    }

    private async Task<bool> BeUniqueUsername(string userName, CancellationToken cancellationToken)
    {
        return !await _userRepository.UserNameExistsAsync(userName, cancellationToken);
    }
}
