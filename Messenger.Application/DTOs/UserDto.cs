namespace Messenger.Application.DTOs;

/// <summary>
/// Данные пользователя для отображения.
/// </summary>
public class UserDto
{
    public Guid Id { get; set; }
    public string Nickname { get; set; } = string.Empty;
}