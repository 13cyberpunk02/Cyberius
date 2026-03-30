using Cyberius.Application.Features.Blog.Interfaces;
using Cyberius.Application.Features.Blog.Posts.Models;
using Cyberius.Domain.Entities;
using Cyberius.Domain.Entities.Enums;
using Cyberius.Domain.Interfaces;
using Cyberius.Domain.Shared;

namespace Cyberius.Application.Features.Blog.Posts.Services;

public sealed class PostService(IUnitOfWork uow, IStorageService storageService) : IPostService
{
        // ── Queries ────────────────────────────────────────────────────────────
 
    public async Task<Result<PostDetailResponse>> GetByIdAsync(
        Guid id, Guid? currentUserId, CancellationToken ct = default)
    {
        var post = await uow.Posts.GetFullAsync(id, ct);
        if (post is null)
            return Errors.Post.NotFound(id.ToString());
 
        await uow.PostViews.HasViewedAsync(id, currentUserId, null, TimeSpan.Zero, ct);
        return MapToDetail(post, currentUserId);
    }
 
    public async Task<Result<PostDetailResponse>> GetBySlugAsync(
        string slug, Guid? currentUserId, CancellationToken ct = default)
    {
        var post = await uow.Posts.GetBySlugAsync(slug, ct);
        if (post is null)
            return Errors.Post.NotFoundBySlug(slug);
 
        return await GetByIdAsync(post.Id, currentUserId, ct);
    }
 
    public async Task<Result<PagedResponse<PostSummaryResponse>>> GetPublishedAsync(
        int page, int pageSize, CancellationToken ct = default)
    {
        var (items, total) = await uow.Posts.GetPublishedPagedAsync(page, pageSize, ct);
        return ToPagedResult(items, total, page, pageSize);
    }
 
    public async Task<Result<PagedResponse<PostSummaryResponse>>> GetByCategoryAsync(
        Guid categoryId, int page, int pageSize, CancellationToken ct = default)
    {
        var category = await uow.Categories.GetByIdAsync(categoryId, ct);
        if (category is null)
            return Errors.Category.NotFound(categoryId.ToString());
 
        var (items, total) = await uow.Posts.GetByCategoryAsync(categoryId, page, pageSize, ct);
        return ToPagedResult(items, total, page, pageSize);
    }
 
    public async Task<Result<PagedResponse<PostSummaryResponse>>> GetByTagAsync(
        string tagSlug, int page, int pageSize, CancellationToken ct = default)
    {
        var (items, total) = await uow.Posts.GetByTagAsync(tagSlug, page, pageSize, ct);
        return ToPagedResult(items, total, page, pageSize);
    }
 
    public async Task<Result<PagedResponse<PostSummaryResponse>>> SearchAsync(
        string query, int page, int pageSize, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(query))
            return new BadRequestError("Поисковый запрос не может быть пустым");
 
        var (items, total) = await uow.Posts.SearchAsync(query.Trim(), page, pageSize, ct);
        return ToPagedResult(items, total, page, pageSize);
    }
 
    public async Task<Result<PagedResponse<PostSummaryResponse>>> GetDraftsByAuthorAsync(
        Guid authorId, CancellationToken ct = default)
    {
        var drafts = await uow.Posts.GetDraftsByAuthorAsync(authorId, ct);
        var summaries = drafts.Select(MapToSummary).ToList();
        return new PagedResponse<PostSummaryResponse>(summaries, summaries.Count, 1, summaries.Count, 1);
    }
 
    // ── Commands ───────────────────────────────────────────────────────────
 
    public async Task<Result<PostDetailResponse>> CreateAsync(
        Guid authorId, CreatePostRequest request, CancellationToken ct = default)
    {
        var category = await uow.Categories.GetByIdAsync(request.CategoryId, ct);
        if (category is null)
            return Errors.Category.NotFound(request.CategoryId.ToString());
 
        var slug = await GenerateUniqueSlugAsync(request.Title, null, ct);
 
        // Находим или создаём теги
        var tags = await ResolveTagsAsync(request.Tags, ct);
 
        var post = new Post
        {
            Id         = Guid.NewGuid(),
            Title      = request.Title.Trim(),
            Slug       = slug,
            Excerpt    = request.Excerpt?.Trim(),
            CoverImageUrl = request.CoverImageUrl,
            CategoryId = request.CategoryId,
            AuthorId   = authorId,
            Status     = PostStatus.Draft,
            CreatedAt  = DateTime.UtcNow,
            UpdatedAt  = DateTime.UtcNow,
            ReadTimeMinutes = CalculateReadTime(request.Blocks),
        };
 
        post.PostTags = tags.Select(t => new PostTag { PostId = post.Id, TagId = t.Id }).ToList();
        post.Blocks   = MapBlocks(request.Blocks, post.Id);
 
        await uow.Posts.AddAsync(post, ct);
        await uow.SaveChangesAsync(ct);
 
        var created = await uow.Posts.GetFullAsync(post.Id, ct);
        return MapToDetail(created!, null);
    }
 
    public async Task<Result<PostDetailResponse>> UpdateAsync(
        Guid postId, Guid currentUserId, UpdatePostRequest request, CancellationToken ct = default)
    {
        var post = await uow.Posts.GetWithBlocksAsync(postId, ct);
        if (post is null)
            return Errors.Post.NotFound(postId.ToString());
 
        if (post.AuthorId != currentUserId)
            return Errors.Post.NotAuthor();
 
        var category = await uow.Categories.GetByIdAsync(request.CategoryId, ct);
        if (category is null)
            return Errors.Category.NotFound(request.CategoryId.ToString());
 
        // Удаляем старую обложку из Minio если она изменилась
        if (post.CoverImageUrl is not null
            && post.CoverImageUrl != request.CoverImageUrl)
        {
            var oldObjectName = ExtractObjectName(post.CoverImageUrl);
            if (oldObjectName is not null)
                await storageService.DeleteAsync(oldObjectName, ct);
        }
 
        var slug = post.Title == request.Title
            ? post.Slug
            : await GenerateUniqueSlugAsync(request.Title, postId, ct);
 
        var tags = await ResolveTagsAsync(request.Tags, ct);
 
        // Перезаписываем блоки
        await uow.ContentBlocks.RemoveAllByPostIdAsync(postId, ct);
 
        post.Title        = request.Title.Trim();
        post.Slug         = slug;
        post.Excerpt      = request.Excerpt?.Trim();
        post.CoverImageUrl = request.CoverImageUrl;
        post.CategoryId   = request.CategoryId;
        post.UpdatedAt    = DateTime.UtcNow;
        post.ReadTimeMinutes = CalculateReadTime(request.Blocks);
        post.PostTags     = tags.Select(t => new PostTag { PostId = postId, TagId = t.Id }).ToList();
        post.Blocks       = MapBlocks(request.Blocks, postId);
 
        uow.Posts.Update(post);
        await uow.SaveChangesAsync(ct);
 
        var updated = await uow.Posts.GetFullAsync(postId, ct);
        return MapToDetail(updated!, null);
    }
 
    public async Task<Result> PublishAsync(
        Guid postId, Guid currentUserId, CancellationToken ct = default)
    {
        var post = await uow.Posts.GetByIdAsync(postId, ct);
        if (post is null) return Errors.Post.NotFound(postId.ToString());
        if (post.AuthorId != currentUserId) return Errors.Post.NotAuthor();
        if (post.Status == PostStatus.Published) return Errors.Post.AlreadyPublished();
 
        post.Status      = PostStatus.Published;
        post.PublishedAt = DateTime.UtcNow;
        post.UpdatedAt   = DateTime.UtcNow;
 
        uow.Posts.Update(post);
        await uow.SaveChangesAsync(ct);
        return Result.Success();
    }
 
    public async Task<Result> UnpublishAsync(
        Guid postId, Guid currentUserId, CancellationToken ct = default)
    {
        var post = await uow.Posts.GetByIdAsync(postId, ct);
        if (post is null) return Errors.Post.NotFound(postId.ToString());
        if (post.AuthorId != currentUserId) return Errors.Post.NotAuthor();
 
        post.Status    = PostStatus.Draft;
        post.UpdatedAt = DateTime.UtcNow;
 
        uow.Posts.Update(post);
        await uow.SaveChangesAsync(ct);
        return Result.Success();
    }
 
    public async Task<Result> DeleteAsync(
        Guid postId, Guid currentUserId, CancellationToken ct = default)
    {
        var post = await uow.Posts.GetByIdAsync(postId, ct);
        if (post is null) return Errors.Post.NotFound(postId.ToString());
        if (post.AuthorId != currentUserId) return Errors.Post.NotAuthor();
 
        // Удаляем обложку из Minio
        if (post.CoverImageUrl is not null)
        {
            var objectName = ExtractObjectName(post.CoverImageUrl);
            if (objectName is not null)
                await storageService.DeleteAsync(objectName, ct);
        }
 
        uow.Posts.Remove(post);
        await uow.SaveChangesAsync(ct);
        return Result.Success();
    }
 
    public async Task<Result> ReactAsync(
        Guid postId, Guid userId, ReactionType type, CancellationToken ct = default)
    {
        var post = await uow.Posts.GetByIdAsync(postId, ct);
        if (post is null) return Errors.Post.NotFound(postId.ToString());
 
        var existing = await uow.PostReactions.GetByPostAndUserAsync(postId, userId, ct);
 
        if (existing is not null && existing.Type == type)
        {
            // Повторное нажатие — убираем реакцию
            await uow.PostReactions.RemoveByPostAndUserAsync(postId, userId, ct);
        }
        else
        {
            if (existing is not null)
                await uow.PostReactions.RemoveByPostAndUserAsync(postId, userId, ct);
 
            await uow.PostReactions.AddAsync(new PostReaction
            {
                Id        = Guid.NewGuid(),
                PostId    = postId,
                UserId    = userId,
                Type      = type,
                CreatedAt = DateTime.UtcNow,
            }, ct);
        }
 
        await uow.SaveChangesAsync(ct);
        return Result.Success();
    }
 
    // ── Private helpers ────────────────────────────────────────────────────
 
    private async Task<string> GenerateUniqueSlugAsync(
        string title, Guid? excludeId, CancellationToken ct)
    {
        var baseSlug = GenerateSlug(title);
        var slug     = baseSlug;
        var counter  = 1;
 
        while (await uow.Posts.SlugExistsAsync(slug, excludeId, ct))
            slug = $"{baseSlug}-{counter++}";
 
        return slug;
    }
 
    private static string GenerateSlug(string title)
    {
        var translitMap = new Dictionary<char, string>
        {
            ['а']="a",['б']="b",['в']="v",['г']="g",['д']="d",['е']="e",['ё']="yo",
            ['ж']="zh",['з']="z",['и']="i",['й']="y",['к']="k",['л']="l",['м']="m",
            ['н']="n",['о']="o",['п']="p",['р']="r",['с']="s",['т']="t",['у']="u",
            ['ф']="f",['х']="kh",['ц']="ts",['ч']="ch",['ш']="sh",['щ']="shch",
            ['ъ']="",['ы']="y",['ь']="",['э']="e",['ю']="yu",['я']="ya",
        };
 
        var lower  = title.ToLower().Trim();
        var result = new System.Text.StringBuilder();
 
        foreach (var c in lower)
        {
            if (translitMap.TryGetValue(c, out var t)) result.Append(t);
            else if (char.IsLetterOrDigit(c))           result.Append(c);
            else if (c == ' ' || c == '-')              result.Append('-');
        }
 
        return System.Text.RegularExpressions.Regex
            .Replace(result.ToString(), "-{2,}", "-")
            .Trim('-');
    }
 
    private async Task<List<Tag>> ResolveTagsAsync(
        List<string> tagNames, CancellationToken ct)
    {
        var existing = await uow.Tags.GetByNamesAsync(tagNames, ct);
        var newNames = tagNames
            .Where(n => !existing.Any(e => e.Name.Equals(n, StringComparison.OrdinalIgnoreCase)))
            .ToList();
 
        foreach (var name in newNames)
        {
            var tag = new Tag
            {
                Id   = Guid.NewGuid(),
                Name = name.Trim(),
                Slug = GenerateSlug(name),
            };
            await uow.Tags.AddAsync(tag, ct);
            existing = [.. existing, tag];
        }
 
        return existing.ToList();
    }
 
    private static List<ContentBlock> MapBlocks(
        List<CreateContentBlockRequest> requests, Guid postId) =>
        requests.Select((r, i) => new ContentBlock
        {
            Id           = Guid.NewGuid(),
            PostId       = postId,
            Type         = r.Type,
            Order        = r.Order == 0 ? i : r.Order,
            Content      = r.Content,
            Language     = r.Language,
            ImageUrl     = r.ImageUrl,
            ImageCaption = r.ImageCaption,
            CalloutType  = r.CalloutType,
        }).ToList();
 
    private static int CalculateReadTime(List<CreateContentBlockRequest> blocks)
    {
        const int wordsPerMinute = 200;
        var words = blocks
            .Where(b => b.Content is not null)
            .Sum(b => b.Content!.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length);
        return Math.Max(1, (int)Math.Ceiling((double)words / wordsPerMinute));
    }
 
    private static Result<PagedResponse<PostSummaryResponse>> ToPagedResult(
        IReadOnlyList<Post> items, int total, int page, int pageSize)
    {
        var summaries   = items.Select(MapToSummary).ToList();
        var totalPages  = (int)Math.Ceiling((double)total / pageSize);
        return new PagedResponse<PostSummaryResponse>(summaries, total, page, pageSize, totalPages);
    }
 
    private static PostSummaryResponse MapToSummary(Post p) => new(
        p.Id, p.Title, p.Slug, p.Excerpt, p.CoverImageUrl,
        p.ReadTimeMinutes, p.Status, p.PublishedAt, p.CreatedAt,
        new AuthorDto(p.Author.UserId, p.Author.UserName ?? "", $"{p.Author.FirstName} {p.Author.LastName}".Trim(), p.Author.AvatarObjectName),
        new CategoryDto(p.Category.Id, p.Category.Name, p.Category.Slug, p.Category.Color, p.Category.IconUrl),
        p.PostTags.Select(pt => pt.Tag.Name).ToList(),
        ViewCount: 0, CommentCount: 0,
        Reactions: []);
 
    private static PostDetailResponse MapToDetail(Post p, Guid? currentUserId)
    {
        var myReaction = currentUserId.HasValue
            ? p.Reactions.FirstOrDefault(r => r.UserId == currentUserId.Value)?.Type.ToString()
            : null;
 
        var reactions = p.Reactions
            .GroupBy(r => r.Type)
            .ToDictionary(g => g.Key.ToString(), g => g.Count());
 
        return new PostDetailResponse(
            p.Id, p.Title, p.Slug, p.Excerpt, p.CoverImageUrl,
            p.ReadTimeMinutes, p.Status, p.PublishedAt, p.CreatedAt,
            new AuthorDto(p.Author.UserId, p.Author.UserName ?? "", $"{p.Author.FirstName} {p.Author.LastName}".Trim(), p.Author.AvatarObjectName),
            new CategoryDto(p.Category.Id, p.Category.Name, p.Category.Slug, p.Category.Color, p.Category.IconUrl),
            p.PostTags.Select(pt => pt.Tag.Name).ToList(),
            p.Blocks.OrderBy(b => b.Order).Select(b => new ContentBlockDto(
                b.Id, b.Type, b.Order, b.Content, b.Language,
                b.ImageUrl, b.ImageCaption, b.CalloutType)).ToList(),
            reactions,
            myReaction,
            ViewCount: p.Views?.Count ?? 0,
            CommentCount: p.Comments?.Count(c => !c.IsDeleted) ?? 0);
    }
    
    private static string? ExtractObjectName(string url)
    {
        const string marker = "/files/";
        var idx = url.IndexOf(marker, StringComparison.Ordinal);
        return idx >= 0 ? url[(idx + marker.Length)..] : null;
    }
}