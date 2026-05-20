using Messenger.Domain.Entities;
using Messenger.Domain.Enums;
using Messenger.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Messenger.Infrastructure.Persistence.Repositories;

/// <summary>
/// Реализация репозитория сообщений.
/// </summary>
public class MessageRepository : IMessageRepository
{
    private readonly AppDbContext _context;
    private readonly ILogger<MessageRepository> _logger;

    public MessageRepository(AppDbContext context, ILogger<MessageRepository> logger)
    {
        _context = context;
        _logger = logger;
    }
    
    public async Task<List<Message>> GetAllMessagesForUserAsync(Guid userId)
    {
        return await _context.Messages
            .Include(m => m.Sender)
            .Include(m => m.Recipient)
            .Where(m => m.SenderId == userId || m.RecipientId == userId)
            .ToListAsync();
    }

    public async Task<Message?> GetByIdAsync(Guid id)
        => await _context.Messages
            .Include(m => m.Sender)
            .Include(m => m.RepliedToMessage)
                .ThenInclude(r => r!.Sender)
            .Include(m => m.Attachments)
            .FirstOrDefaultAsync(m => m.Id == id);

    public async Task<List<Message>> GetConversationAsync(Guid userId1, Guid userId2, int skip = 0, int take = 50)
    {
        return await _context.Messages
            .Include(m => m.Sender)
            .Include(m => m.RepliedToMessage)
                .ThenInclude(r => r!.Sender)
            .Include(m => m.Attachments)
            .Where(m => (m.SenderId == userId1 && m.RecipientId == userId2)
                     || (m.SenderId == userId2 && m.RecipientId == userId1))
            .OrderByDescending(m => m.CreatedAt)
            .Skip(skip)
            .Take(take)
            .OrderBy(m => m.CreatedAt) // Переворачиваем для правильного порядка
            .ToListAsync();
    }

    public async Task AddAsync(Message message)
        => await _context.Messages.AddAsync(message);

    public async Task UpdateStatusAsync(Guid messageId, MessageStatus status)
    {
        var msg = await _context.Messages.FindAsync(messageId);
        if (msg != null)
        {
            msg.Status = status;
            _context.Messages.Update(msg);
        }
    }

    public async Task SaveChangesAsync()
        => await _context.SaveChangesAsync();
}