// TaskFlow.Core/Models/Attachment.cs
using TaskFlow.Core.Common;

namespace TaskFlow.Core.Models;

public class Attachment
{
    public Guid Id { get; private set; }
    public string FileName { get; private set; }
    public string FileUrl { get; private set; }
    public long FileSize { get; private set; }
    public string? MimeType { get; private set; }
    public DateTime UploadedAt { get; private set; }

    // Внешние ключи
    public Guid TaskId { get; private set; }
    public Guid UploadedBy { get; private set; }

    // Навигационные свойства (денормализация)
    public string? UploaderName { get; private set; }
    public string? UploaderAvatarUrl { get; private set; }

    // Приватный конструктор
    private Attachment(
        Guid id,
        string fileName,
        string fileUrl,
        long fileSize,
        Guid taskId,
        Guid uploadedBy,
        DateTime uploadedAt)
    {
        // Инварианты
        if (id == Guid.Empty)
            throw new DomainException("Attachment ID cannot be empty");

        if (string.IsNullOrEmpty(fileName))
            throw new DomainException("File name is required for existing attachment");

        if (string.IsNullOrEmpty(fileUrl))
            throw new DomainException("File URL is required for existing attachment");

        if (fileSize <= 0)
            throw new DomainException("File size must be positive");

        if (taskId == Guid.Empty)
            throw new DomainException("Attachment must belong to a task");

        if (uploadedBy == Guid.Empty)
            throw new DomainException("Attachment must have an uploader");

        Id = id;
        FileName = fileName;
        FileUrl = fileUrl;
        FileSize = fileSize;
        TaskId = taskId;
        UploadedBy = uploadedBy;
        UploadedAt = uploadedAt;
    }

    // Фабричный метод создания вложения
    public static Attachment Create(
        string fileName,
        string fileUrl,
        long fileSize,
        Guid taskId,
        Guid uploadedBy,
        string? mimeType = null)
    {
        // Предполагается, что fileName и fileUrl уже валидированы в Application слое
        // Проверка размера файла, типа файла и т.д. в Application

        return new Attachment(
            id: Guid.NewGuid(),
            fileName: fileName,
            fileUrl: fileUrl,
            fileSize: fileSize,
            taskId: taskId,
            uploadedBy: uploadedBy,
            uploadedAt: DateTime.UtcNow
        )
        {
            MimeType = mimeType
        };
    }

    // Метод для восстановления из БД
    public static Attachment Restore(
        Guid id,
        string fileName,
        string fileUrl,
        long fileSize,
        string? mimeType,
        Guid taskId,
        Guid uploadedBy,
        DateTime uploadedAt,
        string? uploaderName = null,
        string? uploaderAvatarUrl = null)
    {
        return new Attachment(id, fileName, fileUrl, fileSize, taskId, uploadedBy, uploadedAt)
        {
            MimeType = mimeType,
            UploaderName = uploaderName,
            UploaderAvatarUrl = uploaderAvatarUrl
        };
    }

    // Доменные методы
    public void Rename(string newFileName)
    {
        if (string.IsNullOrEmpty(newFileName))
            throw new DomainException("File name cannot be null or empty");

        if (newFileName.Length > 255)
            throw new DomainException("File name is too long");

        FileName = newFileName;
    }

    public void UpdateUrl(string newFileUrl)
    {
        if (string.IsNullOrEmpty(newFileUrl))
            throw new DomainException("File URL cannot be null or empty");

        FileUrl = newFileUrl;
    }

    // Вспомогательные свойства
    public string FileExtension
    {
        get
        {
            var dotIndex = FileName.LastIndexOf('.');
            return dotIndex >= 0 ? FileName.Substring(dotIndex) : "";
        }
    }

    public string FileNameWithoutExtension
    {
        get
        {
            var dotIndex = FileName.LastIndexOf('.');
            return dotIndex >= 0 ? FileName.Substring(0, dotIndex) : FileName;
        }
    }

    public string FormattedFileSize
    {
        get
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            double len = FileSize;
            int order = 0;

            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }

            return $"{len:0.##} {sizes[order]}";
        }
    }

    public bool IsImage
    {
        get
        {
            if (string.IsNullOrEmpty(MimeType))
                return false;

            return MimeType.StartsWith("image/", StringComparison.OrdinalIgnoreCase);
        }
    }

    public bool IsDocument
    {
        get
        {
            if (string.IsNullOrEmpty(MimeType))
                return false;

            var docMimeTypes = new[]
            {
                "application/pdf",
                "application/msword",
                "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                "application/vnd.ms-excel",
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "text/plain"
            };

            return docMimeTypes.Contains(MimeType, StringComparer.OrdinalIgnoreCase);
        }
    }

    public string IconName
    {
        get
        {
            if (IsImage) return "image";
            if (IsDocument) return "file-text";

            var extension = FileExtension.ToLower();
            return extension switch
            {
                ".pdf" => "file-pdf",
                ".doc" or ".docx" => "file-word",
                ".xls" or ".xlsx" => "file-excel",
                ".zip" or ".rar" or ".7z" => "archive",
                ".mp4" or ".avi" or ".mov" => "video",
                ".mp3" or ".wav" => "music",
                _ => "file"
            };
        }
    }

    public TimeSpan Age => DateTime.UtcNow - UploadedAt;

    public string GetRelativeTime()
    {
        var timeSpan = Age;

        if (timeSpan.TotalSeconds < 60)
            return "только что";

        if (timeSpan.TotalMinutes < 60)
        {
            var minutes = (int)timeSpan.TotalMinutes;
            return $"{minutes} {Pluralize(minutes, "минуту", "минуты", "минут")} назад";
        }

        if (timeSpan.TotalHours < 24)
        {
            var hours = (int)timeSpan.TotalHours;
            return $"{hours} {Pluralize(hours, "час", "часа", "часов")} назад";
        }

        if (timeSpan.TotalDays < 30)
        {
            var days = (int)timeSpan.TotalDays;
            return $"{days} {Pluralize(days, "день", "дня", "дней")} назад";
        }

        var months = (int)(timeSpan.TotalDays / 30);
        return $"{months} {Pluralize(months, "месяц", "месяца", "месяцев")} назад";
    }

    private static string Pluralize(int number, string one, string two, string five)
    {
        var n = Math.Abs(number) % 100;
        var n1 = n % 10;

        if (n > 10 && n < 20) return five;
        if (n1 > 1 && n1 < 5) return two;
        if (n1 == 1) return one;
        return five;
    }

    // Проверка прав доступа
    public bool CanBeDeletedBy(Guid userId)
    {
        // Вложение может удалить загрузивший или администратор доски
        return userId == UploadedBy;
    }

    public bool CanBeDownloadedBy(Guid userId)
    {
        // Вложение может скачать любой участник доски
        // (Проверка участия в доске будет в Application слое)
        return true; // Базовый доступ, далее дополняется
    }

    // Приватный конструктор для EF Core
    private Attachment() : this(
        Guid.NewGuid(),
        "unnamed.file",
        "https://example.com/file",
        1024,
        Guid.NewGuid(),
        Guid.NewGuid(),
        DateTime.UtcNow)
    { }
}

// Расширения для работы с вложениями
public static class AttachmentExtensions
{
    public static IEnumerable<Attachment> Images(this IEnumerable<Attachment> attachments)
    {
        return attachments.Where(a => a.IsImage);
    }

    public static IEnumerable<Attachment> Documents(this IEnumerable<Attachment> attachments)
    {
        return attachments.Where(a => a.IsDocument);
    }

    public static IEnumerable<Attachment> ByUser(this IEnumerable<Attachment> attachments, Guid userId)
    {
        return attachments.Where(a => a.UploadedBy == userId);
    }

    public static long TotalSize(this IEnumerable<Attachment> attachments)
    {
        return attachments.Sum(a => a.FileSize);
    }

    public static string TotalFormattedSize(this IEnumerable<Attachment> attachments)
    {
        var totalSize = attachments.TotalSize();

        string[] sizes = { "B", "KB", "MB", "GB", "TB" };
        double len = totalSize;
        int order = 0;

        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len = len / 1024;
        }

        return $"{len:0.##} {sizes[order]}";
    }
}