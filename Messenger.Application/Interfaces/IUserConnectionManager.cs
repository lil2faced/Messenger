namespace Messenger.Application.Interfaces;

/// <summary>
/// Менеджер подключений пользователей (SignalR).
/// </summary>
public interface IUserConnectionManager
{
    /// <summary>
    /// Добавить соединение для пользователя.
    /// </summary>
    void AddConnection(Guid userId, string connectionId);

    /// <summary>
    /// Удалить конкретное соединение пользователя.
    /// </summary>
    void RemoveConnection(Guid userId, string connectionId);

    /// <summary>
    /// Получить все активные соединения пользователя.
    /// </summary>
    IEnumerable<string> GetConnections(Guid userId);

    /// <summary>
    /// Проверить, находится ли пользователь онлайн.
    /// </summary>
    bool IsOnline(Guid userId);
}