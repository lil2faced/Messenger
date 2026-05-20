using Messenger.Application.DTOs;
using Messenger.Application.Interfaces;
using Messenger.Domain.Entities;
using Messenger.Domain.Enums;
using Messenger.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace Messenger.Application.Services;

/// <summary>
/// Сервис обработки сообщений: отправка, получение истории, отметка о прочтении.
/// </summary>
public class MessageService : IMessageService
{
    private readonly IMessageRepository _messageRepository;
    private readonly IUserRepository _userRepository;
    private readonly ILogger<MessageService> _logger;

    public MessageService(IMessageRepository messageRepository, IUserRepository userRepository, ILogger<MessageService> logger)
    {
        _messageRepository = messageRepository;
        _userRepository = userRepository;
        _logger = logger;
    }

    /// <summary>
    /// Отправляет новое сообщение.
    /// </summary>
    public async Task<MessageDto> SendMessageAsync(Guid senderId, Guid recipientId, string? text, Guid? repliedToMessageId, List<AttachmentDto>? attachments)
    {
        _logger.LogInformation("Отправка сообщения от {Sender} к {Recipient}", senderId, recipientId);

        var sender = await _userRepository.GetByIdAsync(senderId);
        var recipient = await _userRepository.GetByIdAsync(recipientId);
        if (sender == null || recipient == null)
            throw new ArgumentException("Отправитель или получатель не найден.");

        var message = new Message
        {
            Id = Guid.NewGuid(),
            SenderId = senderId,
            RecipientId = recipientId,
            Text = text,
            RepliedToMessageId = repliedToMessageId,
            Status = MessageStatus.Sent,
            CreatedAt = DateTime.UtcNow
        };

        // Прикрепляем вложения
        if (attachments != null)
        {
            foreach (var att in attachments)
            {
                message.Attachments.Add(new MessageAttachment
                {
                    Id = Guid.NewGuid(),
                    MessageId = message.Id,
                    FileUrl = att.FileUrl,
                    Type = att.Type
                });
            }
        }

        await _messageRepository.AddAsync(message);
        await _messageRepository.SaveChangesAsync();

        return MapToDto(message);
    }

    /// <summary>
    /// Получает историю сообщений между двумя пользователями.
    /// </summary>
    public async Task<List<MessageDto>> GetConversationAsync(Guid currentUserId, Guid otherUserId, int skip, int take)
    {
        _logger.LogInformation("Запрос истории сообщений между {U1} и {U2}", currentUserId, otherUserId);
        var messages = await _messageRepository.GetConversationAsync(currentUserId, otherUserId, skip, take);
        return messages.Select(m => MapToDto(m, currentUserId)).ToList();
    }

    /// <summary>
    /// Отмечает сообщение как прочитанное получателем.
    /// </summary>
    public async Task MarkAsReadAsync(Guid messageId, Guid readerId)
    {
        _logger.LogInformation("Пользователь {Reader} отметил сообщение {MsgId} как прочитанное", readerId, messageId);
        var message = await _messageRepository.GetByIdAsync(messageId);
        if (message == null)
            throw new ArgumentException("Сообщение не найдено.");
        if (message.RecipientId != readerId)
            throw new UnauthorizedAccessException("Только получатель может отметить сообщение прочитанным.");

        if (message.Status != MessageStatus.Read)
        {
            await _messageRepository.UpdateStatusAsync(messageId, MessageStatus.Read);
            await _messageRepository.SaveChangesAsync();
        }
    }

    public async Task<List<ChatContactDto>> GetContactsAsync(Guid userId)
    {
        _logger.LogInformation("Получение контактов для пользователя {UserId}", userId);

        // Получаем все сообщения, где userId является отправителем или получателем
        var messages = await _messageRepository.GetAllMessagesForUserAsync(userId);

        // Группируем по собеседнику и выбираем последнее сообщение
        var contacts = messages
            .GroupBy(m => m.SenderId == userId ? m.RecipientId : m.SenderId)
            .Select(g => new
            {
                InterlocutorId = g.Key,
                LastMessage = g.OrderByDescending(m => m.CreatedAt).FirstOrDefault()
            })
            .Select(g => new ChatContactDto
            {
                UserId = g.InterlocutorId,
                Nickname = g.LastMessage!.SenderId == userId
                    ? g.LastMessage.Recipient.Nickname
                    : g.LastMessage.Sender.Nickname,
                LastMessage = g.LastMessage.Text?.Length > 50
                    ? g.LastMessage.Text[..50] + "..."
                    : g.LastMessage.Text,
                LastMessageTime = g.LastMessage.CreatedAt
            })
            .OrderByDescending(c => c.LastMessageTime)
            .ToList();

        return contacts;
    }

    private MessageDto MapToDto(Message msg, Guid currentUserId)
    {
        var dto = new MessageDto
        {
            Id = msg.Id,
            SenderId = msg.SenderId,
            SenderNickname = msg.Sender?.Nickname ?? "",
            Text = msg.Text,
            RepliedToMessageId = msg.RepliedToMessageId,
            RepliedToText = msg.RepliedToMessage?.Text,
            RepliedToSenderNickname = msg.RepliedToMessage?.Sender?.Nickname,
            RepliedToAttachments = msg.RepliedToMessage?.Attachments?.Select(a => new AttachmentDto { FileUrl = a.FileUrl, Type = a.Type }).ToList() ?? new(),
            Status = msg.Status.ToString(),
            CreatedAt = msg.CreatedAt,
            Attachments = msg.Attachments?.Select(a => new AttachmentDto { FileUrl = a.FileUrl, Type = a.Type }).ToList() ?? new(),
            IsMine = msg.SenderId == currentUserId
        };
        return dto;
    }
    private MessageDto MapToDto(Message msg)
    {
        return MapToDto(msg, Guid.Empty);
    }
}