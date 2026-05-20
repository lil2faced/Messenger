namespace Messenger.Application.Interfaces;

/// <summary>
/// Сервис для сохранения загруженных файлов.
/// </summary>
public interface IFileStorageService
{
    /// <summary>
    /// Сохраняет файл и возвращает относительный URL.
    /// </summary>
    /// <param name="fileStream">Поток файла.</param>
    /// <param name="fileName">Имя файла.</param>
    /// <param name="fileType">Тип (image/voice).</param>
    /// <returns>Относительный путь к файлу.</returns>
    Task<string> SaveFileAsync(Stream fileStream, string fileName, string fileType);
}