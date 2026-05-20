using System.Security.Claims;
using Messenger.Application.DTOs;
using Messenger.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Messenger.API.Controllers;

/// <summary>
/// Контроллер для работы с сообщениями через REST (загрузка файлов и история).
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MessagesController : ControllerBase
{
    private readonly IMessageService _messageService;
    private readonly IFileStorageService _fileStorageService;
    private readonly ILogger<MessagesController> _logger;

    public MessagesController(IMessageService messageService, IFileStorageService fileStorageService, ILogger<MessagesController> logger)
    {
        _messageService = messageService;
        _fileStorageService = fileStorageService;
        _logger = logger;
    }

    /// <summary>
    /// Загрузить файл (изображение или голосовое).
    /// </summary>
    [HttpPost("upload")]
    public async Task<IActionResult> UploadFile(IFormFile file, [FromQuery] string type)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new { error = "Файл не выбран." });
        if (type != "image" && type != "voice")
            return BadRequest(new { error = "Тип файла должен быть 'image' или 'voice'." });

        try
        {
            var url = await _fileStorageService.SaveFileAsync(file.OpenReadStream(), file.FileName, type);
            return Ok(new { url });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка загрузки файла");
            return StatusCode(500, new { error = "Ошибка при сохранении файла." });
        }
    }

    /// <summary>
    /// Получить историю сообщений с пользователем.
    /// </summary>
    [HttpGet("conversation/{otherUserId:guid}")]
    public async Task<IActionResult> GetConversation(Guid otherUserId, [FromQuery] int skip = 0, [FromQuery] int take = 50)
    {
        var currentUserId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        try
        {
            var messages = await _messageService.GetConversationAsync(currentUserId, otherUserId, skip, take);
            return Ok(messages);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка получения истории");
            return StatusCode(500, new { error = "Ошибка получения сообщений." });
        }
    }

    /// <summary>
    /// Получить список контактов (чатов) для текущего пользователя.
    /// </summary>
    [HttpGet("contacts")]
    public async Task<IActionResult> GetContacts()
    {
        var currentUserId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        try
        {
            var contacts = await _messageService.GetContactsAsync(currentUserId);
            return Ok(contacts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка получения контактов");
            return StatusCode(500, new { error = "Ошибка получения контактов." });
        }
    }
}