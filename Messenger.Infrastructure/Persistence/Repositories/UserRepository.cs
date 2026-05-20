using Messenger.Domain.Entities;
using Messenger.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Messenger.Infrastructure.Persistence.Repositories;

/// <summary>
/// Реализация репозитория пользователей с использованием EF Core.
/// </summary>
public class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;
    private readonly ILogger<UserRepository> _logger;

    public UserRepository(AppDbContext context, ILogger<UserRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<User?> GetByIdAsync(Guid id)
        => await _context.Users.FindAsync(id);

    public async Task<User?> GetByLoginAsync(string login)
        => await _context.Users.FirstOrDefaultAsync(u => u.Login == login);

    public async Task<User?> GetByEmailAsync(string email)
        => await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

    public async Task<User?> GetByNicknameAsync(string nickname)
        => await _context.Users.FirstOrDefaultAsync(u => u.Nickname == nickname);

    public async Task<List<User>> SearchByNicknameAsync(string search, int limit = 10)
    {
        _logger.LogInformation("Поиск пользователей по шаблону: {Search}", search);
        return await _context.Users
            .Where(u => EF.Functions.ILike(u.Nickname, $"%{search}%"))
            .Take(limit)
            .ToListAsync();
    }

    public async Task AddAsync(User user)
        => await _context.Users.AddAsync(user);

    public async Task SaveChangesAsync()
        => await _context.SaveChangesAsync();
}