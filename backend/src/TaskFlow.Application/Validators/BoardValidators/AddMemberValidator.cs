using FluentValidation;
using TaskFlow.Application.Common;
using TaskFlow.Application.DTOs.Board;

namespace TaskFlow.Application.Validators.BoardValidators;

public class AddMemberValidator : BaseValidator<AddMemberRequest>
{
    public AddMemberValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required");
        /*
        RuleFor(x => x.Role)
            .NotEmpty().WithMessage("Role is required")
            .Must(role => role == "owner" || role == "admin" || role == "member")
            .WithMessage("Role must be 'owner', 'admin', or 'member'");
    */
    }
}
