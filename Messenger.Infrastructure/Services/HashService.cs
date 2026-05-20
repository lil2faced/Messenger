using Messenger.Application.Interfaces;

namespace Messenger.Infrastructure.Services;

/// <summary>
/// Сервис хэширования паролей с использованием BCrypt.
/// </summary>
public class HashService : IHashService
{
    public string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password);
    }

    public bool VerifyPassword(string password, string hash)
    {
        return BCrypt.Net.BCrypt.Verify(password, hash);
    }
}