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

    public string GetPublicUrl(string objectName)
        => $"{_options.PublicBaseUrl}/files/{Uri.EscapeDataString(objectName)}";
    
    public async Task<Result<string>> UploadAsync(
        Stream stream,
        string fileName,
        string contentType,
        CancellationToken ct = default)
    {
        try
        {
            await EnsureBucketExistsAsync(ct);

            // Уникальное имя объекта чтобы не было коллизий
            var objectName = $"{Guid.NewGuid()}_{fileName}";

            var putArgs = new PutObjectArgs()
                .WithBucket(_options.BucketName)
                .WithObject(objectName)
                .WithStreamData(stream)
                .WithObjectSize(stream.Length)
                .WithContentType(contentType);

            await minioClient.PutObjectAsync(putArgs, ct);

            logger.LogInformation("Файл {FileName} загружен как {ObjectName}", 
                fileName, objectName);

            return objectName; 
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Ошибка при загрузке файла {FileName}", fileName);
            return Errors.Internal("Ошибка при загрузке файла", ex);
        }
    }

    public async Task<Result<Stream>> GetFileStreamAsync(
        string objectName,
        CancellationToken ct = default)
    {
        try
        {
            var memoryStream = new MemoryStream();

            var getArgs = new GetObjectArgs()
                .WithBucket(_options.BucketName)
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

    public async Task<Result> DeleteAsync(
        string objectName,
        CancellationToken ct = default)
    {
        try
        {
            var removeArgs = new RemoveObjectArgs()
                .WithBucket(_options.BucketName)
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

    private async Task EnsureBucketExistsAsync(CancellationToken ct)
    {
        var existsArgs = new BucketExistsArgs()
            .WithBucket(_options.BucketName);

        var exists = await minioClient.BucketExistsAsync(existsArgs, ct);
        if (exists) return;

        var makeArgs = new MakeBucketArgs()
            .WithBucket(_options.BucketName);

        await minioClient.MakeBucketAsync(makeArgs, ct);

        logger.LogInformation("Bucket {BucketName} создан", _options.BucketName);
    }
}