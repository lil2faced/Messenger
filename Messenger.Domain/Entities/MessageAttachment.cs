namespace Messenger.Domain.Entities;

/// <summary>
/// Вложение к сообщению (изображение или голосовое сообщение).
/// </summary>
public class MessageAttachment
{
    /// <summary>
    /// Уникальный идентификатор вложения.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Идентификатор сообщения, к которому относится вложение.
    /// </summary>
    public Guid MessageId { get; set; }

    /// <summary>
    /// Сообщение, к которому относится вложение.
    /// </summary>
    public Message Message { get; set; } = null!;

    /// <summary>
    /// URL файла (относительный путь).
    /// </summary>
    public string FileUrl { get; set; } = string.Empty;

    /// <summary>
    /// Тип вложения: "image" или "voice".
    /// </summary>
    public string Type { get; set; } = string.Empty;
}