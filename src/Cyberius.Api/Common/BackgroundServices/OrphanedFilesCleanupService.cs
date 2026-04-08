using Cyberius.Domain.Interfaces;

namespace Cyberius.Api.Common.BackgroundServices;

/// <summary>
/// Фоновая служба — удаляет файлы из MinIO которые не используются в БД.
/// Запускается раз в сутки при старте приложения.
/// </summary>
public sealed class OrphanedFilesCleanupService(
    IServiceScopeFactory scopeFactory,
    ILogger<OrphanedFilesCleanupService> logger) : BackgroundService
{
    // Интервал очистки — раз в сутки
    private static readonly TimeSpan Interval = TimeSpan.FromHours(24);
 
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("OrphanedFilesCleanupService started");
 
        // Первый запуск — через 5 минут после старта приложения
        await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
 
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CleanupAsync(stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogError(ex, "Error during orphaned files cleanup");
            }
 
            await Task.Delay(Interval, stoppingToken);
        }
    }
 
    private async Task CleanupAsync(CancellationToken ct)
    {
        logger.LogInformation("Starting orphaned files cleanup...");
 
        await using var scope = scopeFactory.CreateAsyncScope();
        var uow     = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var storage = scope.ServiceProvider.GetRequiredService<IStorageService>();
 
        var deleted = 0;
 
        // ── 1. Собираем все objectName которые реально используются в БД ──
 
        var usedFiles = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
 
        // Аватары пользователей
        var avatars = await uow.Users.GetAllAvatarObjectNamesAsync(ct);
        foreach (var a in avatars.Where(a => !string.IsNullOrEmpty(a)))
            usedFiles.Add(a!);
 
        // Обложки статей
        var covers = await uow.Posts.GetAllCoverObjectNamesAsync(ct);
        foreach (var c in covers.Where(c => !string.IsNullOrEmpty(c)))
            usedFiles.Add(ExtractObjectName(c!));
 
        // Изображения в блоках статей
        var blockImages = await uow.ContentBlocks.GetAllImageObjectNamesAsync(ct);
        foreach (var img in blockImages.Where(img => !string.IsNullOrEmpty(img)))
            usedFiles.Add(ExtractObjectName(img!));
 
        logger.LogInformation("Found {Count} files used in DB", usedFiles.Count);
 
        // ── 2. Получаем все файлы из MinIO ────────────────────────────────
        var allFiles = await storage.ListAllObjectNamesAsync(ct);
 
        // ── 3. Удаляем те что не используются ─────────────────────────────
        foreach (var file in allFiles)
        {
            if (usedFiles.Contains(file)) continue;
 
            // Не удаляем слишком новые файлы — могут быть в процессе загрузки
            var fileAge = await storage.GetObjectAgeAsync(file, ct);
            if (fileAge < TimeSpan.FromHours(1))
            {
                logger.LogDebug("Skipping recent file: {File}", file);
                continue;
            }
 
            var result = await storage.DeleteAsync(file, ct);
            if (result.IsSuccess)
            {
                deleted++;
                logger.LogDebug("Deleted orphaned file: {File}", file);
            }
        }
 
        logger.LogInformation(
            "Cleanup complete. Deleted {Deleted} orphaned files", deleted);
    }
 
    // Извлекаем objectName из полного URL: http://host/api/files/covers/uuid.jpg → covers/uuid.jpg
    private static string ExtractObjectName(string url)
    {
        if (!url.StartsWith("http")) return url; // уже objectName
 
        const string marker = "/api/files/";
        var idx = url.IndexOf(marker, StringComparison.Ordinal);
        return idx >= 0 ? url[(idx + marker.Length)..] : url;
    }
}