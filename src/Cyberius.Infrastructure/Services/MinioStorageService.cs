using Cyberius.Domain.Interfaces;
using Cyberius.Domain.Options;
using Cyberius.Domain.Shared;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Minio;
using Minio.DataModel.Args;

namespace Cyberius.Infrastructure.Services;

public class MinioStorageService(
    IMinioClient        minioClient,
    IOptions<MinioOptions> options,
    ILogger<MinioStorageService> logger) : IStorageService
{
    private readonly MinioOptions _options = options.Value;

    // Папки для аватаров и блога лежат в разных bucket-ах
    private const string BlogBucket   = "blog-files";   // обложки и блоки статей
    // AvatarBucket берём из настроек (_options.BucketName = "avatars")
 
    // ── Public URL ─────────────────────────────────────────────────────────
    // Файлы отдаются через API: GET /api/files/{objectName}
    // При этом objectName может содержать слэш: "covers/uuid_file.jpg"
    // — его нужно НЕ экранировать, иначе роут не совпадёт
    public string GetPublicUrl(string objectName)
        => $"{_options.PublicBaseUrl}/api/files/{objectName}";
 
    // ── Upload ─────────────────────────────────────────────────────────────
    // fileName приходит уже с уникальным именем от FilesEndpoint
    // ("covers/uuid_file.jpg") — просто кладём как есть, без доп. UUID
    public async Task<Result<string>> UploadAsync(
        Stream stream,
        string fileName,
        string contentType,
        CancellationToken ct = default)
    {
        try
        {
            var bucket = BucketFor(fileName);
            await EnsureBucketExistsAsync(bucket, ct);
 
            var putArgs = new PutObjectArgs()
                .WithBucket(bucket)
                .WithObject(fileName)           // используем как есть
                .WithStreamData(stream)
                .WithObjectSize(stream.Length)
                .WithContentType(contentType);
 
            await minioClient.PutObjectAsync(putArgs, ct);
 
            logger.LogInformation(
                "Файл загружен: bucket={Bucket}, object={Object}",
                bucket, fileName);
 
            return fileName;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Ошибка при загрузке файла {FileName}", fileName);
            return Errors.Internal("Ошибка при загрузке файла", ex);
        }
    }
 
    // ── Get stream ─────────────────────────────────────────────────────────
    public async Task<Result<Stream>> GetFileStreamAsync(
        string objectName,
        CancellationToken ct = default)
    {
        try
        {
            var bucket = BucketFor(objectName);
            var memoryStream = new MemoryStream();
 
            var getArgs = new GetObjectArgs()
                .WithBucket(bucket)
                .WithObject(objectName)
                .WithCallbackStream(async (stream, token) =>
                {
                    await stream.CopyToAsync(memoryStream, token);
                });
 
            await minioClient.GetObjectAsync(getArgs, ct);
            memoryStream.Position = 0;
 
            return memoryStream;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Ошибка при получении файла {ObjectName}", objectName);
            return Errors.Internal("Ошибка при получении файла", ex);
        }
    }
 
    // ── Delete ─────────────────────────────────────────────────────────────
    public async Task<Result> DeleteAsync(
        string objectName,
        CancellationToken ct = default)
    {
        try
        {
            var bucket = BucketFor(objectName);
 
            var removeArgs = new RemoveObjectArgs()
                .WithBucket(bucket)
                .WithObject(objectName);
 
            await minioClient.RemoveObjectAsync(removeArgs, ct);
            return Result.Success();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Ошибка при удалении {ObjectName}", objectName);
            return Errors.Internal("Ошибка при удалении файла", ex);
        }
    }
 
    // ── Helpers ────────────────────────────────────────────────────────────
 
    // Определяем bucket по prefix имени файла:
    //   "covers/..."  → blog-files
    //   "blocks/..."  → blog-files
    //   "avatars/..." → avatars  (настройка BucketName)
    //   всё остальное → avatars  (обратная совместимость)
    private string BucketFor(string objectName) =>
        objectName.StartsWith("covers/") || objectName.StartsWith("blocks/")
            ? BlogBucket
            : _options.BucketName;
 
    private async Task EnsureBucketExistsAsync(string bucket, CancellationToken ct)
    {
        var exists = await minioClient.BucketExistsAsync(
            new BucketExistsArgs().WithBucket(bucket), ct);
 
        if (exists) return;
 
        await minioClient.MakeBucketAsync(
            new MakeBucketArgs().WithBucket(bucket), ct);
 
        logger.LogInformation("Bucket создан: {BucketName}", bucket);
    }
}