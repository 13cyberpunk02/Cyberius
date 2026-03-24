using Cyberius.Api.Endpoints;

namespace Cyberius.Api.Common.Extensions;

public static class EndpointsMappingExtension
{
    public static IEndpointRouteBuilder MapAllEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapAuthenticationEndpoints();
        return endpoints;
    }
}