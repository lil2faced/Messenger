using System.ComponentModel.DataAnnotations;

namespace Messenger.Domain.Entities;

/// <summary>
/// Представляет пользователя мессенджера.
/// </summary>
public class User
{
    /// <summary>
    /// Уникальный идентификатор пользователя.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Логин пользователя для входа в систему.
    /// </summary>
    public string Login { get; set; } = string.Empty;

    /// <summary>
    /// Хэшированный пароль пользователя.
    /// </summary>
    public string PasswordHash { get; set; } = string.Empty;

    /// <summary>
    /// Адрес электронной почты.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Отображаемый никнейм.
    /// </summary>
    public string Nickname { get; set; } = string.Empty;

    /// <summary>
    /// Дата и время создания учётной записи.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Дата и время последнего обновления.
    /// </summary>
    public DateTime UpdatedAt { get; set; }
}