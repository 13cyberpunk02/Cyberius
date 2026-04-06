namespace Cyberius.Api.Common.Extensions;

public static class CachingExtensions
{
    /// <summary>
    /// Публичные данные которые меняются редко — кэш 60 сек 
    /// </summary>
    public static RouteHandlerBuilder WithPublicCache(
        this RouteHandlerBuilder builder, int seconds = 60) =>
        builder.CacheOutput(p => p
            .Expire(TimeSpan.FromSeconds(seconds))
            .Tag("public"));
 
    /// <summary>
    /// Короткий кэш для часто меняющихся данных — 10 сек 
    /// </summary>
    public static RouteHandlerBuilder WithShortCache(
        this RouteHandlerBuilder builder) =>
        builder.CacheOutput(p => p
            .Expire(TimeSpan.FromSeconds(10))
            .Tag("short"));
    
    /// <summary>
    /// Добавить в DI и middleware 
    /// </summary>
    public static IServiceCollection AddBlogOutputCache(
        this IServiceCollection services) =>
        services.AddOutputCache(options =>
        {
            options.AddBasePolicy(builder =>
                builder.With(ctx =>
                    ctx.HttpContext.Request.Method == HttpMethod.Get.Method));
        });
}