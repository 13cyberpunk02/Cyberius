using Cyberius.Api.Common.Extensions;

namespace Cyberius.Api.Common;

public static class MapAllExtensions
{
    public static IServiceCollection MapAllServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddJwtAuthentication(configuration);
        services.AddCorsPolicy();
        return services;
    }
}