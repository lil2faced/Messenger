using FluentValidation;
using Messenger.Application.DTOs;

namespace Messenger.Application.Validators;

/// <summary>
/// Валидатор для обновления никнейма.
/// </summary>
public class UpdateProfileDtoValidator : AbstractValidator<UpdateProfileDto>
{
    public UpdateProfileDtoValidator()
    {
        RuleFor(x => x.Nickname)
            .NotEmpty().WithMessage("Никнейм обязателен")
            .MinimumLength(3).WithMessage("Никнейм должен быть не менее 3 символов")
            .MaximumLength(30).WithMessage("Никнейм не должен превышать 30 символов");
    }
}