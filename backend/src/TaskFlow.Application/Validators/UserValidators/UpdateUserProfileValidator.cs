using FluentValidation;
using TaskFlow.Application.Common;
using TaskFlow.Application.DTOs.User;

namespace TaskFlow.Application.Validators.UserValidators;

public class UpdateUserProfileValidator : BaseValidator<UpdateUserProfileRequest>
{
    public UpdateUserProfileValidator()
    {
    
        RuleFor(x => x.FullName)
           .MaximumLength(255).WithMessage("Full name cannot exceed 255 characters")
           .When(x => !string.IsNullOrEmpty(x.FullName));

        RuleFor(x => x.AvatarUrl)
            .MaximumLength(500).WithMessage("Avatar URL cannot exceed 500 characters")
            .Must(url => Uri.TryCreate(url, UriKind.Absolute, out _))
            .When(x => !string.IsNullOrEmpty(x.AvatarUrl))
            .WithMessage("Avatar URL must be a valid URL");
    }
}
