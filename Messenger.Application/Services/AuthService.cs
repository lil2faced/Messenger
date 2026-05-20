using FluentValidation;
using Messenger.Application.DTOs;
using Messenger.Application.Interfaces;
using Messenger.Domain.Entities;
using Messenger.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace Messenger.Application.Services;

/// <summary>
/// Сервис аутентификации и регистрации пользователей.
/// </summary>
public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly ITokenService _tokenService;
    private readonly IHashService _hashService;
    private readonly IValidator<RegisterDto> _registerValidator;
    private readonly IValidator<LoginDto> _loginValidator;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        IUserRepository userRepository,
        ITokenService tokenService,
        IHashService hashService,
        IValidator<RegisterDto> registerValidator,
        IValidator<LoginDto> loginValidator,
        ILogger<AuthService> logger)
    {
        _userRepository = userRepository;
        _tokenService = tokenService;
        _hashService = hashService;
        _registerValidator = registerValidator;
        _loginValidator = loginValidator;
        _logger = logger;
    }

    /// <summary>
    /// Регистрирует нового пользователя после валидации.
    /// </summary>
    /// <param name="dto">Данные регистрации.</param>
    public async Task RegisterAsync(RegisterDto dto)
    {
        _logger.LogInformation("Регистрация пользователя {Login}", dto.Login);

        var validationResult = await _registerValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
        {
            var errors = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage));
            throw new ValidationException(validationResult.Errors);
        }

        // Проверка уникальности
        if (await _userRepository.GetByLoginAsync(dto.Login) != null)
            throw new ApplicationException("Пользователь с таким логином уже существует.");
        if (await _userRepository.GetByEmailAsync(dto.Email) != null)
            throw new ApplicationException("Пользователь с таким email уже существует.");
        if (await _userRepository.GetByNicknameAsync(dto.Nickname) != null)
            throw new ApplicationException("Пользователь с таким никнеймом уже существует.");

        var user = new User
        {
            Id = Guid.NewGuid(),
            Login = dto.Login,
            PasswordHash = _hashService.HashPassword(dto.Password),
            Email = dto.Email,
            Nickname = dto.Nickname,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _userRepository.AddAsync(user);
        await _userRepository.SaveChangesAsync();

        _logger.LogInformation("Пользователь {Login} успешно зарегистрирован", dto.Login);
    }

    /// <summary>
    /// Аутентифицирует пользователя и возвращает JWT токен.
    /// </summary>
    /// <param name="dto">Данные для входа.</param>
    /// <returns>JWT токен.</returns>
    public async Task<string> LoginAsync(LoginDto dto)
    {
        _logger.LogInformation("Попытка входа для {LoginOrEmail}", dto.LoginOrEmail);

        var validationResult = await _loginValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        User? user = await _userRepository.GetByLoginAsync(dto.LoginOrEmail)
                     ?? await _userRepository.GetByEmailAsync(dto.LoginOrEmail);

        if (user == null)
        {
            _logger.LogWarning("Пользователь {LoginOrEmail} не найден", dto.LoginOrEmail);
            throw new UnauthorizedAccessException("Неверный логин/email или пароль.");
        }

        if (!_hashService.VerifyPassword(dto.Password, user.PasswordHash))
        {
            _logger.LogWarning("Неверный пароль для {LoginOrEmail}", dto.LoginOrEmail);
            throw new UnauthorizedAccessException("Неверный логин/email или пароль.");
        }

        var token = _tokenService.GenerateToken(user);
        _logger.LogInformation("Пользователь {Login} успешно вошёл", user.Login);
        return token;
    }
}