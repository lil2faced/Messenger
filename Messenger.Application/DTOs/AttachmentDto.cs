namespace Messenger.Application.DTOs;

/// <summary>
/// Вложение.
/// </summary>
public class AttachmentDto
{
    public string FileUrl { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
}