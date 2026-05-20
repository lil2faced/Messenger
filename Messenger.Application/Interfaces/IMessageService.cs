using Messenger.Application.DTOs;

namespace Messenger.Application.Interfaces;

/// <summary>
/// Сервис обработки сообщений.
/// </summary>
public interface IMessageService
{
    /// <summary>
    /// Отправить сообщение.
    /// </summary>
    /// <param name="senderId">Идентификатор отправителя.</param>
    /// <param name="recipientId">Идентификатор получателя.</param>
    /// <param name="text">Текст сообщения.</param>
    /// <param name="repliedToMessageId">Идентификатор сообщения для ответа.</param>
    /// <param name="attachmentUrls">Список URL загруженных файлов.</param>
    /// <returns>Созданное сообщение.</returns>
    Task<MessageDto> SendMessageAsync(Guid senderId, Guid recipientId, string? text, Guid? repliedToMessageId, List<AttachmentDto>? attachments);

    /// <summary>
    /// Получить историю сообщений.
    /// </summary>
    /// <param name="userId1">Первый участник.</param>
    /// <param name="userId2">Второй участник.</param>
    /// <param name="skip">Пропустить.</param>
    /// <param name="take">Количество.</param>
    Task<List<MessageDto>> GetConversationAsync(Guid currentUserId, Guid otherUserId, int skip, int take);

    /// <summary>
    /// Отметить сообщение как прочитанное.
    /// </summary>
    Task MarkAsReadAsync(Guid messageId, Guid readerId);

    /// <summary>
    /// Получить список контактов (пользователей, с которыми есть переписка).
    /// </summary>
    Task<List<ChatContactDto>> GetContactsAsync(Guid userId);
}