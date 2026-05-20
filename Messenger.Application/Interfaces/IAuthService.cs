using Messenger.Application.DTOs;

namespace Messenger.Application.Interfaces;

/// <summary>
/// Сервис аутентификации и регистрации.
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Зарегистрировать нового пользователя.
    /// </summary>
    /// <param name="dto">Данные регистрации.</param>
    Task RegisterAsync(RegisterDto dto);

    /// <summary>
    /// Аутентифицировать пользователя и вернуть JWT токен.
    /// </summary>
    /// <param name="dto">Данные для входа.</param>
    /// <returns>JWT токен.</returns>
    Task<string> LoginAsync(LoginDto dto);
}