namespace Messenger.Application.Interfaces;

/// <summary>
/// Сервис хэширования и проверки паролей.
/// </summary>
public interface IHashService
{
    string HashPassword(string password);
    bool VerifyPassword(string password, string hash);
}