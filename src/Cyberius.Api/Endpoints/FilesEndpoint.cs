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

        group.MapGet("{objectName}", GetFile)
            .WithSummary("Files");
        
        return group;
    }

    private static async Task<IResult> GetFile(string objectName, IStorageService storageService,
        CancellationToken cancellationToken)
    {
        var result = await storageService.GetFileStreamAsync(objectName, cancellationToken);
        if (result.IsFailure)
            return Results.NotFound();
        var extension    = Path.GetExtension(objectName).ToLower();
        var contentType  = extension switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png"            => "image/png",
            ".webp"           => "image/webp",
            ".gif"            => "image/gif",
            _                 => "application/octet-stream"
        };
        
        return Results.Stream(result.Value, contentType);
    }
}