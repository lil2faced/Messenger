using Messenger.Domain.Entities;

namespace Messenger.Domain.Interfaces;

/// <summary>
/// Репозиторий для работы с пользователями.
/// </summary>
public interface IUserRepository
{
    /// <summary>
    /// Получить пользователя по идентификатору.
    /// </summary>
    /// <param name="id">Идентификатор пользователя.</param>
    /// <returns>Пользователь или null.</returns>
    Task<User?> GetByIdAsync(Guid id);

    /// <summary>
    /// Получить пользователя по логину.
    /// </summary>
    Task<User?> GetByLoginAsync(string login);

    /// <summary>
    /// Получить пользователя по email.
    /// </summary>
    Task<User?> GetByEmailAsync(string email);

    /// <summary>
    /// Получить пользователя по никнейму.
    /// </summary>
    Task<User?> GetByNicknameAsync(string nickname);

    /// <summary>
    /// Поиск пользователей по части никнейма (автодополнение).
    /// </summary>
    /// <param name="search">Поисковый запрос.</param>
    /// <param name="limit">Максимальное количество результатов.</param>
    Task<List<User>> SearchByNicknameAsync(string search, int limit = 10);

    /// <summary>
    /// Добавить нового пользователя.
    /// </summary>
    Task AddAsync(User user);

    /// <summary>
    /// Сохранить изменения.
    /// </summary>
    Task SaveChangesAsync();
}