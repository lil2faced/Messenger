namespace Messenger.Domain.Enums;

/// <summary>
/// Статус доставки сообщения.
/// </summary>
public enum MessageStatus
{
    /// <summary>Сообщение отправлено, но ещё не доставлено получателю.</summary>
    Sent,

    /// <summary>Сообщение доставлено на устройство получателя, но не прочитано.</summary>
    Delivered,

    /// <summary>Сообщение прочитано получателем.</summary>
    Read
}