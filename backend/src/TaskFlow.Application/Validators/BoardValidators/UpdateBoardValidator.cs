
using FluentValidation;
using TaskFlow.Application.Common;
using TaskFlow.Application.DTOs.Board;

namespace TaskFlow.Application.Validators.BoardValidators;

public class UpdateBoardValidator : BaseValidator<UpdateBoardRequest>
{
    public UpdateBoardValidator()
    {
        RuleFor(x => x.Title)
           .NotEmpty().WithMessage("Board title is required")
           .MaximumLength(100).WithMessage("Board title cannot exceed 100 characters");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Board description cannot exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.Description));

        RuleFor(x => x.Color)
            .NotEmpty().WithMessage("Color is required")
            .Must(BeValidHexColor).WithMessage("Invalid color format. Expected #RRGGBB");
    }
}
