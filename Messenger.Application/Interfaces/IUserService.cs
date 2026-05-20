using Messenger.Application.DTOs;

namespace Messenger.Application.Interfaces;

/// <summary>
/// Сервис работы с пользователями (поиск).
/// </summary>
public interface IUserService
{
    /// <summary>
    /// Поиск пользователей по никнейму.
    /// </summary>
    /// <param name="query">Поисковый запрос.</param>
    /// <returns>Список пользователей.</returns>
    Task<List<UserDto>> SearchUsersAsync(string query);
    
    Task UpdateNicknameAsync(Guid userId, string newNickname);
}