using Messenger.Domain.Entities;

namespace Messenger.Domain.Interfaces;

/// <summary>
/// Репозиторий для работы с сообщениями.
/// </summary>
public interface IMessageRepository
{
    /// <summary>
    /// Получить сообщение по идентификатору.
    /// </summary>
    Task<Message?> GetByIdAsync(Guid id);

    /// <summary>
    /// Получить историю сообщений между двумя пользователями (упорядоченную по времени).
    /// </summary>
    /// <param name="userId1">Идентификатор первого пользователя.</param>
    /// <param name="userId2">Идентификатор второго пользователя.</param>
    /// <param name="skip">Количество пропускаемых сообщений (для пагинации).</param>
    /// <param name="take">Количество загружаемых сообщений.</param>
    Task<List<Message>> GetConversationAsync(Guid userId1, Guid userId2, int skip = 0, int take = 50);

    /// <summary>
    /// Добавить сообщение.
    /// </summary>
    Task AddAsync(Message message);

    /// <summary>
    /// Обновить статус сообщения.
    /// </summary>
    Task UpdateStatusAsync(Guid messageId, Enums.MessageStatus status);

    /// <summary>
    /// Сохранить изменения.
    /// </summary>
    Task SaveChangesAsync();

    /// <summary>
    /// Получить все сообщения, где пользователь является отправителем или получателем.
    /// </summary>
    Task<List<Message>> GetAllMessagesForUserAsync(Guid userId);
}