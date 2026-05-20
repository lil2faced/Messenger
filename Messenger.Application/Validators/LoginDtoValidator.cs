using FluentValidation;
using Messenger.Application.DTOs;

namespace Messenger.Application.Validators;

/// <summary>
/// Валидатор для входа.
/// </summary>
public class LoginDtoValidator : AbstractValidator<LoginDto>
{
    public LoginDtoValidator()
    {
        RuleFor(x => x.LoginOrEmail)
            .NotEmpty().WithMessage("Логин или Email обязателен");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Пароль обязателен");
    }
}