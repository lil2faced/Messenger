namespace Messenger.Application.DTOs;

/// <summary>
/// Данные для входа в систему.
/// </summary>
public class LoginDto
{
    public string LoginOrEmail { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}