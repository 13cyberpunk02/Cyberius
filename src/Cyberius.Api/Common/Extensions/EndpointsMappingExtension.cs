using Cyberius.Api.Endpoints;

namespace Cyberius.Api.Common.Extensions;

public static class EndpointsMappingExtension
{
    public static IEndpointRouteBuilder MapAllEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapAuthenticationEndpoints();
        endpoints.MapUserEndpoints();
        endpoints.MapFilesEndpoints();
        endpoints.MapPostEndpoints();
        endpoints.MapCommentEndpoints();
        endpoints.MapCategoryEndpoints();
        endpoints.MapTagEndpoints();
        endpoints.MapFeedEndpoints();
        return endpoints;
    }
}