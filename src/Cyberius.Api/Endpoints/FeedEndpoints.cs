using System.Globalization;
using Cyberius.Application.Features.Blog.Interfaces;

namespace Cyberius.Api.Endpoints;

public static class FeedEndpoints
{
    public static IEndpointRouteBuilder MapFeedEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/feed.xml", GetRssFeed)
            .WithDisplayName("RSS Feed")
            .WithTags("Feed")
            .Produces(200, contentType: "application/rss+xml; charset=utf-8");
 
        return endpoints;
    }
 
    private static async Task<IResult> GetRssFeed(
        IPostService postService,
        CancellationToken ct)
    {
        var result = await postService.GetPublishedAsync(1, 20, ct);
        if (result.IsFailure) return Results.Problem("Не удалось загрузить статьи");
 
        var posts    = result.Value.Items;
        var siteUrl  = "http://localhost:4200";
        var apiUrl   = "http://localhost:5273";
        var now      = DateTimeOffset.UtcNow.ToString("R");
 
        var items = string.Join('\n', posts.Select(p =>
        {
            var pubDate = p.PublishedAt.HasValue
                ? DateTimeOffset.Parse(p.PublishedAt.Value.ToString(CultureInfo.CurrentCulture)).ToString("R")
                : now;
 
            var description = System.Security.SecurityElement.Escape(p.Excerpt ?? p.Title);
            var title       = System.Security.SecurityElement.Escape(p.Title);
            var link        = $"{siteUrl}/posts/{p.Slug}";
 
            return $"""
            <item>
              <title>{title}</title>
              <link>{link}</link>
              <guid isPermaLink="true">{link}</guid>
              <pubDate>{pubDate}</pubDate>
              <description>{description}</description>
              <author>{System.Security.SecurityElement.Escape(p.Author.FullName)}</author>
              <category>{System.Security.SecurityElement.Escape(p.Category.Name)}</category>
            </item>
            """;
        }));
 
        var rss = $"""
        <?xml version="1.0" encoding="UTF-8"?>
        <rss version="2.0" xmlns:atom="http://www.w3.org/2005/Atom">
          <channel>
            <title>Cyberius — C# .NET &amp; Angular</title>
            <link>{siteUrl}</link>
            <description>Практические статьи о разработке на .NET 10, C# 14 и Angular 21+</description>
            <language>ru</language>
            <lastBuildDate>{now}</lastBuildDate>
            <atom:link href="{apiUrl}/feed.xml" rel="self" type="application/rss+xml"/>
            {items}
          </channel>
        </rss>
        """;
 
        return Results.Content(rss, "application/rss+xml; charset=utf-8");
    }
}