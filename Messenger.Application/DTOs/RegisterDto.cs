namespace Messenger.Application.DTOs;

/// <summary>
/// Данные для регистрации нового пользователя.
/// </summary>
public class RegisterDto
{
    public string Login { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Nickname { get; set; } = string.Empty;
}