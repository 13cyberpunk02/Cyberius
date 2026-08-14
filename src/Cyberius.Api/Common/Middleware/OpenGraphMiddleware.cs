using System.Text;
using System.Text.RegularExpressions;
using Cyberius.Domain.Interfaces;

namespace Cyberius.Api.Common.Middleware;

/// <summary>
/// Подставляет Open Graph теги в index.html когда страницу запрашивает
/// краулер соцсети. Боты не выполняют JS, поэтому Angular Meta-сервис
/// им не поможет - теги должны быть в HTML который отдаёт сервер.
/// </summary>
public sealed class OpenGraphMiddleware(RequestDelegate next)
{
    private static readonly string[] BotAgents =
    [
        "telegrambot", "twitterbot", "facebookexternalhit", "vkshare",
        "whatsapp", "slackbot", "discordbot", "linkedinbot",
        "googlebot", "yandexbot", "bingbot", "applebot",
        "skypeuripreview", "redditbot", "pinterest",
    ];
 
    private static readonly Regex SlugRegex = new(
        @"^/posts/(?<slug>[a-z0-9\-]+)/?$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);
 
    public async Task InvokeAsync(
        HttpContext context,
        IUnitOfWork uow,
        IWebHostEnvironment env,
        IConfiguration config)
    {
        var path = context.Request.Path.Value ?? "";
        var ua   = context.Request.Headers.UserAgent.ToString().ToLowerInvariant();
 
        var isBot   = BotAgents.Any(ua.Contains);
        var isPost  = SlugRegex.Match(path);
 
        // Не бот или не страница статьи — обычная обработка
        if (!isBot || !isPost.Success)
        {
            await next(context);
            return;
        }
 
        var slug = isPost.Groups["slug"].Value;
        var post = await uow.Posts.GetBySlugAsync(slug, context.RequestAborted);
 
        if (post is null)
        {
            await next(context);
            return;
        }
 
        var baseUrl = config["AppSettings:FrontendUrl"]?.TrimEnd('/')
                      ?? $"{context.Request.Scheme}://{context.Request.Host}";
 
        var indexPath = Path.Combine(env.WebRootPath, "index.html");
        if (!File.Exists(indexPath))
        {
            await next(context);
            return;
        }
 
        var html = await File.ReadAllTextAsync(indexPath, context.RequestAborted);
 
        var title       = Escape(post.Title);
        var description = Escape(Truncate(post.Excerpt ?? post.Title, 200));
        var image       = string.IsNullOrWhiteSpace(post.CoverImageUrl)
                            ? $"{baseUrl}/og-default.png"
                            : post.CoverImageUrl;
        var url         = $"{baseUrl}/posts/{slug}";
        var published   = post.PublishedAt?.ToString("yyyy-MM-ddTHH:mm:ssZ") ?? "";
 
        var metaTags = $"""
            <title>{title} — Cyberius</title>
            <meta name="description" content="{description}"/>
 
            <meta property="og:type"           content="article"/>
            <meta property="og:site_name"      content="Cyberius"/>
            <meta property="og:locale"         content="ru_RU"/>
            <meta property="og:title"          content="{title}"/>
            <meta property="og:description"    content="{description}"/>
            <meta property="og:image"          content="{image}"/>
            <meta property="og:url"            content="{url}"/>
            <meta property="article:published_time" content="{published}"/>
 
            <meta name="twitter:card"        content="summary_large_image"/>
            <meta name="twitter:title"       content="{title}"/>
            <meta name="twitter:description" content="{description}"/>
            <meta name="twitter:image"       content="{image}"/>
            """;
 
        // Вставляем теги перед </head>, вырезав существующий <title>
        html = Regex.Replace(html, @"<title>.*?</title>", "", RegexOptions.IgnoreCase);
        html = html.Replace("</head>", metaTags + "\n</head>");
 
        context.Response.ContentType = "text/html; charset=utf-8";
        await context.Response.WriteAsync(html, Encoding.UTF8, context.RequestAborted);
    }
 
    private static string Escape(string? text) =>
        string.IsNullOrEmpty(text)
            ? ""
            : text.Replace("&", "&amp;")
                  .Replace("\"", "&quot;")
                  .Replace("<", "&lt;")
                  .Replace(">", "&gt;");
 
    private static string Truncate(string text, int max) =>
        text.Length <= max ? text : text[..max].TrimEnd() + "…";
}
 
public static class OpenGraphMiddlewareExtensions
{
    public static IApplicationBuilder UseOpenGraph(this IApplicationBuilder app) =>
        app.UseMiddleware<OpenGraphMiddleware>();
}
