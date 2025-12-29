using FluentValidation;
using TaskManagementAPI.DTOs;

namespace TaskManagementAPI.Validators;

public class UpdateTaskDtoValidator : AbstractValidator<UpdateTaskDto>
{
    public UpdateTaskDtoValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required")
            .MaximumLength(200).WithMessage("Title must not exceed 200 characters");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required")
            .MaximumLength(1000).WithMessage("Description must not exceed 1000 characters");

        RuleFor(x => x.DueDate)
            .NotEmpty().WithMessage("Due date is required")
            .Must(BeValidFutureOrPresentDate).WithMessage("Due date must be a valid date");

        RuleFor(x => x.Priority)
            .IsInEnum().WithMessage("Priority must be Low, Medium, or High");
    }

    private static bool BeValidFutureOrPresentDate(DateTime date)
    {
        return date >= DateTime.MinValue && date <= DateTime.MaxValue;
    }
}
