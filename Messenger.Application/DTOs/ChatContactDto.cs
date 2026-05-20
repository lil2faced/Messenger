namespace Messenger.Application.DTOs;

/// <summary>
/// Контакт в списке чатов.
/// </summary>
public class ChatContactDto
{
    public Guid UserId { get; set; }
    public string Nickname { get; set; } = string.Empty;
    public string? LastMessage { get; set; }
    public DateTime? LastMessageTime { get; set; }
}