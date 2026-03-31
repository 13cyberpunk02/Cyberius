using Cyberius.Api.Common.Filters;
using Cyberius.Domain.Interfaces;

namespace Cyberius.Api.Endpoints;

public static class FilesEndpoint
{
    public static IEndpointRouteBuilder MapFilesEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints
            .MapGroup("api/files")
            .WithDisplayName("Files Endpoints")
            .WithTags("Files")
            .AddEndpointFilter<RequestLoggingFilter>();

        group.MapGet("{**objectName}", GetFile)
            .WithSummary("Get file by object name");

        group.MapPost("covers", UploadCover)
            .RequireAuthorization()
            .DisableAntiforgery()
            .Accepts<IFormFile>("multipart/form-data")
            .WithSummary("Upload post cover image");

        group.MapPost("blocks", UploadBlockImage)
            .RequireAuthorization()
            .DisableAntiforgery()
            .Accepts<IFormFile>("multipart/form-data")
            .WithSummary("Upload image for content block");

        group.MapPost("avatars", UploadAvatar)
            .RequireAuthorization()
            .DisableAntiforgery()
            .Accepts<IFormFile>("multipart/form-data")
            .WithSummary("Upload user avatar");

        group.MapDelete("{**objectName}", DeleteFile)
            .RequireAuthorization()
            .WithSummary("Delete file by object name");

        return group;
    }

    private static async Task<IResult> GetFile(
        string objectName,
        IStorageService storageService,
        CancellationToken cancellationToken)
    {
        var result = await storageService.GetFileStreamAsync(objectName, cancellationToken);
        if (result.IsFailure)
            return Results.NotFound();

        var ext = Path.GetExtension(objectName).ToLowerInvariant();
        var contentType = ext switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".webp" => "image/webp",
            ".gif" => "image/gif",
            ".svg" => "image/svg+xml",
            _ => "application/octet-stream"
        };

        return Results.Stream(result.Value, contentType);
    }

    private static Task<IResult> UploadCover(
        IFormFile file, IStorageService storageService, CancellationToken ct) =>
        UploadImage(file, storageService, "covers", ct);

    private static Task<IResult> UploadBlockImage(
        IFormFile file, IStorageService storageService, CancellationToken ct) =>
        UploadImage(file, storageService, "blocks", ct);

    private static Task<IResult> UploadAvatar(
        IFormFile file, IStorageService storageService, CancellationToken ct) =>
        UploadImage(file, storageService, "avatars", ct);

    private static async Task<IResult> DeleteFile(
        string objectName, IStorageService storageService, CancellationToken ct)
    {
        var result = await storageService.DeleteAsync(objectName, ct);
        return result.IsSuccess ? Results.NoContent() : Results.NotFound();
    }

    // ── Shared upload ──────────────────────────────────────────────────────

    private static readonly HashSet<string> AllowedTypes =
        ["image/jpeg", "image/png", "image/webp", "image/gif", "image/svg+xml"];

    private const long MaxBytes = 5 * 1024 * 1024; // 5 MB

    private static async Task<IResult> UploadImage(
        IFormFile file,
        IStorageService storageService,
        string prefix,
        CancellationToken ct)
    {
        if (file is null || file.Length == 0)
            return Results.BadRequest(new { Message = "Файл не выбран" });

        if (!AllowedTypes.Contains(file.ContentType))
            return Results.BadRequest(new { Message = "Допустимы только изображения: JPEG, PNG, WebP, GIF, SVG" });

        if (file.Length > MaxBytes)
            return Results.BadRequest(new { Message = "Размер файла не должен превышать 5 МБ" });

        // Формируем имя: "covers/uuid.jpg"
        // UUID только здесь — UploadAsync больше не добавляет свой
        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        var objectName = $"{prefix}/{Guid.NewGuid()}{ext}";

        await using var stream = file.OpenReadStream();
        var result = await storageService.UploadAsync(stream, objectName, file.ContentType, ct);

        if (result.IsFailure)
            return Results.Problem(result.Error.Message);

        return Results.Ok(new
        {
            ObjectName = result.Value,
            Url = storageService.GetPublicUrl(result.Value),
        });
    }
}