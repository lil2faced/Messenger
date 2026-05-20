using Messenger.Domain.Enums;

namespace Messenger.Domain.Entities;

/// <summary>
/// Сообщение в чате между двумя пользователями.
/// </summary>
public class Message
{
    /// <summary>
    /// Уникальный идентификатор сообщения.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Идентификатор отправителя.
    /// </summary>
    public Guid SenderId { get; set; }

    /// <summary>
    /// Отправитель сообщения.
    /// </summary>
    public User Sender { get; set; } = null!;

    /// <summary>
    /// Идентификатор получателя.
    /// </summary>
    public Guid RecipientId { get; set; }

    /// <summary>
    /// Получатель сообщения.
    /// </summary>
    public User Recipient { get; set; } = null!;

    /// <summary>
    /// Текстовое содержимое (может быть пустым, если есть только вложения).
    /// </summary>
    public string? Text { get; set; }

    /// <summary>
    /// Идентификатор сообщения, на которое данное сообщение является ответом (может быть null).
    /// </summary>
    public Guid? RepliedToMessageId { get; set; }

    /// <summary>
    /// Сообщение, на которое данное является ответом.
    /// </summary>
    public Message? RepliedToMessage { get; set; }

    /// <summary>
    /// Статус доставки сообщения.
    /// </summary>
    public MessageStatus Status { get; set; }

    /// <summary>
    /// Дата и время отправки.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Вложения к сообщению.
    /// </summary>
    public ICollection<MessageAttachment> Attachments { get; set; } = new List<MessageAttachment>();
}