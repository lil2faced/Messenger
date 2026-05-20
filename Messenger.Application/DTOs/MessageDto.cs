namespace Messenger.Application.DTOs;

/// <summary>
/// Сообщение для отображения в чате.
/// </summary>
public class MessageDto
{
    public Guid Id { get; set; }
    public Guid SenderId { get; set; }
    public string SenderNickname { get; set; } = string.Empty;
    public string? Text { get; set; }
    public Guid? RepliedToMessageId { get; set; }
    public string? RepliedToText { get; set; }
    public string? RepliedToSenderNickname { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public List<AttachmentDto> Attachments { get; set; } = new();
    public List<AttachmentDto> RepliedToAttachments { get; set; } = new();
    /// <summary>
    /// Принадлежит ли сообщение текущему пользователю (отправлено им).
    /// </summary>
    public bool IsMine { get; set; }
}