using FluentValidation;
using Messenger.Application.DTOs;

namespace Messenger.Application.Validators;

/// <summary>
/// Валидатор для регистрации.
/// </summary>
public class RegisterDtoValidator : AbstractValidator<RegisterDto>
{
    public RegisterDtoValidator()
    {
        RuleFor(x => x.Login)
            .NotEmpty().WithMessage("Логин обязателен")
            .MinimumLength(3).WithMessage("Логин должен быть не менее 3 символов")
            .MaximumLength(50).WithMessage("Логин не должен превышать 50 символов");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Пароль обязателен")
            .MinimumLength(6).WithMessage("Пароль должен быть не менее 6 символов");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email обязателен")
            .EmailAddress().WithMessage("Некорректный email");

        RuleFor(x => x.Nickname)
            .NotEmpty().WithMessage("Никнейм обязателен")
            .MinimumLength(3).WithMessage("Никнейм должен быть не менее 3 символов")
            .MaximumLength(30).WithMessage("Никнейм не должен превышать 30 символов");
    }
}