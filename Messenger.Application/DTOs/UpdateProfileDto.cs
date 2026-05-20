namespace Messenger.Application.DTOs;

/// <summary>
/// Данные для обновления профиля.
/// </summary>
public class UpdateProfileDto
{
    public string Nickname { get; set; } = string.Empty;
}