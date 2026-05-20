using Messenger.Domain.Entities;

namespace Messenger.Application.Interfaces;

/// <summary>
/// Сервис генерации JWT токенов.
/// </summary>
public interface ITokenService
{
    string GenerateToken(User user);
}