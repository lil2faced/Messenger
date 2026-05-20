using System.Security.Claims;
using Messenger.Application.DTOs;
using Messenger.Application.Interfaces;
using Messenger.Domain.Interfaces; // <-- Добавлено для IUserRepository
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Messenger.API.Hubs;

/// <summary>
/// Хаб для обмена сообщениями в реальном времени.
/// </summary>
[Authorize]
public class ChatHub : Hub
{
    private readonly IMessageService _messageService;
    private readonly IUserConnectionManager _connectionManager;
    private readonly IUserRepository _userRepository; // <-- Добавлено
    private readonly ILogger<ChatHub> _logger;

    // В конструктор добавлен IUserRepository
    public ChatHub(
        IMessageService messageService,
        IUserConnectionManager connectionManager,
        IUserRepository userRepository,
        ILogger<ChatHub> logger)
    {
        _messageService = messageService;
        _connectionManager = connectionManager;
        _userRepository = userRepository;
        _logger = logger;
    }

    public override async Task OnConnectedAsync()
    {
        var userId = GetUserId();
        _connectionManager.AddConnection(userId, Context.ConnectionId);
        _logger.LogInformation("Пользователь {UserId} подключился. ConnectionId: {ConnectionId}", userId, Context.ConnectionId);
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = GetUserId();
        _connectionManager.RemoveConnection(userId, Context.ConnectionId);
        _logger.LogInformation("Пользователь {UserId} отключился. ConnectionId: {ConnectionId}", userId, Context.ConnectionId);
        await base.OnDisconnectedAsync(exception);
    }

    /// <summary>
    /// Отправить сообщение другому пользователю.
    /// </summary>
    public async Task SendMessage(Guid recipientId, string? text, Guid? repliedToMessageId, List<AttachmentDto>? attachments)
    {
        var senderId = GetUserId();
        try
        {
            var messageDto = await _messageService.SendMessageAsync(senderId, recipientId, text, repliedToMessageId, attachments);

            // Отправляем получателю, если онлайн
            var recipientConnections = _connectionManager.GetConnections(recipientId).ToList();
            if (recipientConnections.Any())
            {
                await _messageService.MarkAsReadAsync(messageDto.Id, recipientId);
                messageDto.Status = "Delivered";
                await Clients.Clients(recipientConnections).SendAsync("ReceiveMessage", messageDto);
            }

            // Проверяем, является ли это первым сообщением между пользователями,
            // и если да – отправляем событие NewContact обеим сторонам.
            var sender = await _userRepository.GetByIdAsync(senderId);
            var recipient = await _userRepository.GetByIdAsync(recipientId);
            if (sender != null && recipient != null)
            {
                // Проверим количество сообщений между ними (пропустив пагинацию, достаточно первых двух)
                // Примечание: для этого используем отдельный метод, либо проверяем через MessageService.
                // Здесь для простоты вызовем GetConversationAsync и проверим количество.
                var conversation = await _messageService.GetConversationAsync(senderId, recipientId, 0, 2);
                if (conversation.Count <= 1) // только что созданное сообщение (или первое)
                {
                    // Отправляем событие получателю
                    if (recipientConnections.Any())
                    {
                        await Clients.Clients(recipientConnections).SendAsync("NewContact", new
                        {
                            UserId = senderId,
                            Nickname = sender.Nickname,
                            LastMessage = messageDto.Text?.Length > 50 ? messageDto.Text[..50] + "..." : messageDto.Text,
                            LastMessageTime = messageDto.CreatedAt
                        });
                    }

                    // Отправляем событие отправителю (чтобы у него появился контакт получателя, если его ещё нет)
                    var senderConnections = _connectionManager.GetConnections(senderId).ToList();
                    if (senderConnections.Any())
                    {
                        await Clients.Clients(senderConnections).SendAsync("NewContact", new
                        {
                            UserId = recipientId,
                            Nickname = recipient.Nickname,
                            LastMessage = messageDto.Text?.Length > 50 ? messageDto.Text[..50] + "..." : messageDto.Text,
                            LastMessageTime = messageDto.CreatedAt
                        });
                    }
                }
            }

            // Подтверждение отправителю с сохранённым статусом
            await Clients.Caller.SendAsync("MessageSent", messageDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при отправке сообщения от {Sender} к {Recipient}", senderId, recipientId);
            await Clients.Caller.SendAsync("Error", new { message = "Не удалось отправить сообщение." });
        }
    }

    /// <summary>
    /// Отметить сообщение как прочитанное.
    /// </summary>
    public async Task MarkAsRead(Guid messageId)
    {
        var userId = GetUserId();
        try
        {
            await _messageService.MarkAsReadAsync(messageId, userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при отметке о прочтении сообщения {MessageId}", messageId);
            await Clients.Caller.SendAsync("Error", new { message = "Не удалось отметить сообщение." });
        }
    }

    private Guid GetUserId()
    {
        var claim = Context.User?.FindFirst(ClaimTypes.NameIdentifier);
        if (claim == null || !Guid.TryParse(claim.Value, out var userId))
            throw new UnauthorizedAccessException("Пользователь не авторизован.");
        return userId;
    }
}