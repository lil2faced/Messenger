using Messenger.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace Messenger.Infrastructure.Services;

/// <summary>
/// Сервис сохранения файлов на диск в папку uploads.
/// </summary>
public class FileStorageService : IFileStorageService
{
    private readonly string _uploadPath;
    private readonly ILogger<FileStorageService> _logger;

    public FileStorageService(ILogger<FileStorageService> logger)
    {
        _uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "uploads");
        if (!Directory.Exists(_uploadPath))
            Directory.CreateDirectory(_uploadPath);
        _logger = logger;
    }

    public async Task<string> SaveFileAsync(Stream fileStream, string fileName, string fileType)
    {
        // Уникальное имя
        var uniqueName = $"{Guid.NewGuid()}_{fileName}";
        var typeFolder = fileType == "voice" ? "voice" : "images";
        var dir = Path.Combine(_uploadPath, typeFolder);
        if (!Directory.Exists(dir))
            Directory.CreateDirectory(dir);
        var filePath = Path.Combine(dir, uniqueName);

        using (var output = File.Create(filePath))
        {
            await fileStream.CopyToAsync(output);
        }

        var relativeUrl = $"/uploads/{typeFolder}/{uniqueName}";
        _logger.LogInformation("Файл сохранён: {Url}", relativeUrl);
        return relativeUrl;
    }
}