using Messenger.Application.DTOs;
using Messenger.Application.Interfaces;
using Messenger.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace Messenger.Application.Services;

/// <summary>
/// Сервис для поиска пользователей.
/// </summary>
public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<UserService> _logger;

    public UserService(IUserRepository userRepository, ILogger<UserService> logger)
    {
        _userRepository = userRepository;
        _logger = logger;
    }

    /// <summary>
    /// Поиск пользователей по никнейму, возвращает не более 10 результатов.
    /// </summary>
    /// <param name="query">Строка поиска.</param>
    public async Task<List<UserDto>> SearchUsersAsync(string query)
    {
        if (string.IsNullOrWhiteSpace(query) || query.Length < 2)
            return new List<UserDto>();

        _logger.LogInformation("Поиск пользователей по запросу: {Query}", query);
        var users = await _userRepository.SearchByNicknameAsync(query.Trim(), 10);
        return users.Select(u => new UserDto { Id = u.Id, Nickname = u.Nickname }).ToList();
    }

    public async Task UpdateNicknameAsync(Guid userId, string newNickname)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
            throw new ArgumentException("Пользователь не найден.");
        if (await _userRepository.GetByNicknameAsync(newNickname) != null)
            throw new ApplicationException("Никнейм уже занят.");
        user.Nickname = newNickname;
        user.UpdatedAt = DateTime.UtcNow;
        await _userRepository.SaveChangesAsync();
    }
}