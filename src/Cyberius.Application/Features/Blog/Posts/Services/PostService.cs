using Cyberius.Application.Features.Blog.Interfaces;
using Cyberius.Application.Features.Blog.Posts.Models;
using Cyberius.Application.Features.Notifications.Interfaces;
using Cyberius.Domain.Entities;
using Cyberius.Domain.Entities.Enums;
using Cyberius.Domain.Interfaces;
using Cyberius.Domain.Shared;

namespace Cyberius.Application.Features.Blog.Posts.Services;

public sealed class PostService(IUnitOfWork uow, IStorageService storageService, INotificationService notifications)
    : IPostService
{
    // ── Queries ────────────────────────────────────────────────────────────

    public async Task<Result<PostDetailResponse>> GetByIdAsync(
        Guid id, Guid? currentUserId, CancellationToken ct = default)
    {
        var post = await uow.Posts.GetFullAsync(id, ct);
        if (post is null)
            return Errors.Post.NotFound(id.ToString());

        await uow.PostViews.HasViewedAsync(id, currentUserId, null, TimeSpan.Zero, ct);
        return await MapToDetailAsync(post, currentUserId, ct);
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
        return await ToPagedResultAsync(items, total, page, pageSize, ct);
    }

    public async Task<Result<PagedResponse<PostSummaryResponse>>> GetByCategoryAsync(
        Guid categoryId, int page, int pageSize, CancellationToken ct = default)
    {
        var category = await uow.Categories.GetByIdAsync(categoryId, ct);
        if (category is null)
            return Errors.Category.NotFound(categoryId.ToString());

        var (items, total) = await uow.Posts.GetByCategoryAsync(categoryId, page, pageSize, ct);
        return await ToPagedResultAsync(items, total, page, pageSize, ct);
    }

    public async Task<Result<PagedResponse<PostSummaryResponse>>> GetByTagAsync(
        string tagSlug, int page, int pageSize, CancellationToken ct = default)
    {
        var (items, total) = await uow.Posts.GetByTagAsync(tagSlug, page, pageSize, ct);
        return await ToPagedResultAsync(items, total, page, pageSize, ct);
    }

    public async Task<Result<IReadOnlyList<PostSummaryResponse>>> GetRelatedAsync(
        Guid postId, int count, CancellationToken ct = default)
    {
        var posts = await uow.Posts.GetRelatedAsync(postId, count, ct);
        var postIds = posts.Select(p => p.Id).ToList();
        var viewCounts = await uow.PostViews.GetCountsByPostsAsync(postIds, ct);
        var commentCounts = await uow.Comments.GetCountsByPostsAsync(postIds, ct);

        var result = posts.Select(p => MapToSummary(
            p,
            viewCounts.GetValueOrDefault(p.Id, 0),
            commentCounts.GetValueOrDefault(p.Id, 0)
        )).ToList();

        return Result<IReadOnlyList<PostSummaryResponse>>.Success(result);
    }

    public async Task<Result<PagedResponse<PostSummaryResponse>>> GetByAuthorAsync(
        Guid authorId, int page, int pageSize, CancellationToken ct = default)
    {
        var (items, total) = await uow.Posts.GetByAuthorAsync(authorId, page, pageSize, ct);
        return await ToPagedResultAsync(items, total, page, pageSize, ct);
    }

    public async Task<Result<PagedResponse<PostSummaryResponse>>> SearchAsync(
        string query, int page, int pageSize, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(query))
            return new ValidationError("Поисковый запрос не может быть пустым", []);

        var (items, total) = await uow.Posts.SearchAsync(query.Trim(), page, pageSize, ct);
        return await ToPagedResultAsync(items, total, page, pageSize, ct);
    }

    public async Task<Result<PagedResponse<PostSummaryResponse>>> GetDraftsByAuthorAsync(
        Guid authorId, CancellationToken ct = default)
    {
        var drafts = await uow.Posts.GetDraftsByAuthorAsync(authorId, ct);
        return await ToPagedResultAsync(drafts, drafts.Count, 1, drafts.Count, ct);
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
            Id = Guid.NewGuid(),
            Title = request.Title.Trim(),
            Slug = slug,
            Excerpt = request.Excerpt?.Trim(),
            CoverImageUrl = request.CoverImageUrl,
            CategoryId = request.CategoryId,
            AuthorId = authorId,
            Status = PostStatus.Draft,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            ReadTimeMinutes = CalculateReadTime(request.Blocks),
        };

        post.PostTags = tags.Select(t => new PostTag { PostId = post.Id, TagId = t.Id }).ToList();
        post.Blocks = MapBlocks(request.Blocks, post.Id);

        await uow.Posts.AddAsync(post, ct);
        await uow.SaveChangesAsync(ct);

        var created = await uow.Posts.GetFullAsync(post.Id, ct);
        return await MapToDetailAsync(created!, null, ct);
    }

    public async Task<Result<PostDetailResponse>> UpdateAsync(
        Guid postId, Guid currentUserId, UpdatePostRequest request, CancellationToken ct = default)
    {
        // Загружаем пост С тегами (нужны для удаления через Change Tracker)
        // но БЕЗ блоков (они удаляются через ExecuteDeleteAsync вне tracker)
        var post = await uow.Posts.GetForUpdateAsync(postId, ct);
        if (post is null)
            return Errors.Post.NotFound(postId.ToString());

        if (post.AuthorId != currentUserId)
            return Errors.Post.NotAuthor();

        var category = await uow.Categories.GetByIdAsync(request.CategoryId, ct);
        if (category is null)
            return Errors.Category.NotFound(request.CategoryId.ToString());

        // Удаляем старую обложку из Minio если изменилась
        if (post.CoverImageUrl is not null && post.CoverImageUrl != request.CoverImageUrl)
        {
            var oldObjectName = ExtractObjectName(post.CoverImageUrl);
            if (oldObjectName is not null)
                await storageService.DeleteAsync(oldObjectName, ct);
        }

        var slug = post.Title == request.Title
            ? post.Slug
            : await GenerateUniqueSlugAsync(request.Title, postId, ct);

        var tags = await ResolveTagsAsync(request.Tags, ct);

        await uow.ExecuteInTransactionAsync(async () =>
        {
            // Блоки — bulk delete вне Change Tracker, затем добавляем как новые
            await uow.ContentBlocks.RemoveAllByPostIdAsync(postId, ct);

            // Теги — Clear() помечает старые как Deleted в Change Tracker,
            // затем добавляем новые как Added. EF сделает DELETE + INSERT без дублей.
            post.PostTags.Clear();

            post.Title = request.Title.Trim();
            post.Slug = slug;
            post.Excerpt = request.Excerpt?.Trim();
            post.CoverImageUrl = request.CoverImageUrl;
            post.CategoryId = request.CategoryId;
            post.UpdatedAt = DateTime.UtcNow;
            post.ReadTimeMinutes = CalculateReadTime(request.Blocks);

            uow.Posts.Update(post);
            await uow.SaveChangesAsync(ct); // применяем удаление тегов и update поста

            // Добавляем новые блоки и теги уже после удаления старых
            var newBlocks = MapBlocks(request.Blocks, postId);
            foreach (var block in newBlocks)
                await uow.ContentBlocks.AddAsync(block, ct);

            foreach (var tag in tags)
                post.PostTags.Add(new PostTag { PostId = postId, TagId = tag.Id });

            await uow.SaveChangesAsync(ct); // применяем новые блоки и теги
        }, ct);

        var updated = await uow.Posts.GetFullAsync(postId, ct);
        return await MapToDetailAsync(updated!, null, ct);
    }

    public async Task<Result> PublishAsync(
        Guid postId, Guid currentUserId, CancellationToken ct = default)
    {
        var post = await uow.Posts.GetByIdAsync(postId, ct);
        if (post is null) return Errors.Post.NotFound(postId.ToString());
        if (post.AuthorId != currentUserId) return Errors.Post.NotAuthor();
        if (post.Status == PostStatus.Published) return Errors.Post.AlreadyPublished();

        post.Status = PostStatus.Published;
        post.PublishedAt = DateTime.UtcNow;
        post.UpdatedAt = DateTime.UtcNow;

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

        post.Status = PostStatus.Draft;
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
                Id = Guid.NewGuid(),
                PostId = postId,
                UserId = userId,
                Type = type,
                CreatedAt = DateTime.UtcNow,
            }, ct);
        }

        await uow.SaveChangesAsync(ct);

        // Уведомляем автора поста о реакции (только при добавлении новой)
        var isAdding = !(existing is not null && existing.Type == type);
        if (isAdding)
        {
            var reactor = await uow.Users.GetByIdAsync(userId, ct);
            if (reactor is not null && post.AuthorId != userId)
            {
                var reactorName = $"{reactor.FirstName} {reactor.LastName}".Trim();
                _ = notifications.SendPostReactionAsync(
                    post.AuthorId,
                    reactorName,
                    reactor.AvatarObjectName,
                    type.ToString(),
                    post.Title,
                    post.Slug,
                    ct);
            }
        }

        return Result.Success();
    }

    private async Task<string> GenerateUniqueSlugAsync(
        string title, Guid? excludeId, CancellationToken ct)
    {
        var baseSlug = GenerateSlug(title);
        var slug = baseSlug;
        var counter = 1;

        while (await uow.Posts.SlugExistsAsync(slug, excludeId, ct))
            slug = $"{baseSlug}-{counter++}";

        return slug;
    }

    private static string GenerateSlug(string title)
    {
        var translitMap = new Dictionary<char, string>
        {
            ['а'] = "a", ['б'] = "b", ['в'] = "v", ['г'] = "g", ['д'] = "d", ['е'] = "e", ['ё'] = "yo",
            ['ж'] = "zh", ['з'] = "z", ['и'] = "i", ['й'] = "y", ['к'] = "k", ['л'] = "l", ['м'] = "m",
            ['н'] = "n", ['о'] = "o", ['п'] = "p", ['р'] = "r", ['с'] = "s", ['т'] = "t", ['у'] = "u",
            ['ф'] = "f", ['х'] = "kh", ['ц'] = "ts", ['ч'] = "ch", ['ш'] = "sh", ['щ'] = "shch",
            ['ъ'] = "", ['ы'] = "y", ['ь'] = "", ['э'] = "e", ['ю'] = "yu", ['я'] = "ya",
        };

        var lower = title.ToLower().Trim();
        var result = new System.Text.StringBuilder();

        foreach (var c in lower)
        {
            if (translitMap.TryGetValue(c, out var t)) result.Append(t);
            else if (char.IsLetterOrDigit(c)) result.Append(c);
            else if (c == ' ' || c == '-') result.Append('-');
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
                Id = Guid.NewGuid(),
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
            Id = Guid.NewGuid(),
            PostId = postId,
            Type = r.Type,
            Order = r.Order == 0 ? i : r.Order,
            Content = r.Content,
            Language = r.Language,
            ImageUrl = r.ImageUrl,
            ImageCaption = r.ImageCaption,
            CalloutType = r.CalloutType,
        }).ToList();

    private static int CalculateReadTime(List<CreateContentBlockRequest> blocks)
    {
        const int wordsPerMinute = 200;
        var words = blocks
            .Where(b => b.Content is not null)
            .Sum(b => b.Content!.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length);
        return Math.Max(1, (int)Math.Ceiling((double)words / wordsPerMinute));
    }

    private async Task<Result<PagedResponse<PostSummaryResponse>>> ToPagedResultAsync(
        IReadOnlyList<Post> items, int total, int page, int pageSize, CancellationToken ct)
    {
        var postIds = items.Select(p => p.Id).ToList();

        // Считаем просмотры и комментарии одним запросом для всех постов страницы
        var viewCounts = await uow.PostViews.GetCountsByPostsAsync(postIds, ct);
        var commentCounts = await uow.Comments.GetCountsByPostsAsync(postIds, ct);

        var summaries = items.Select(p => MapToSummary(
            p,
            viewCounts.GetValueOrDefault(p.Id, 0),
            commentCounts.GetValueOrDefault(p.Id, 0)
        )).ToList();

        var totalPages = (int)Math.Ceiling((double)total / pageSize);
        return new PagedResponse<PostSummaryResponse>(summaries, total, page, pageSize, totalPages);
    }

    private static PostSummaryResponse MapToSummary(Post p, int viewCount = 0, int commentCount = 0) => new(
        p.Id, p.Title, p.Slug, p.Excerpt, p.CoverImageUrl,
        p.ReadTimeMinutes, p.Status, p.PublishedAt, p.CreatedAt,
        new AuthorDto(p.Author.UserId, p.Author.UserName ?? "", $"{p.Author.FirstName} {p.Author.LastName}".Trim(),
            p.Author.AvatarObjectName),
        new CategoryDto(p.Category.Id, p.Category.Name, p.Category.Slug, p.Category.Color, p.Category.IconUrl),
        p.PostTags.Select(pt => pt.Tag.Name).ToList(),
        ViewCount: viewCount,
        CommentCount: commentCount,
        Reactions: p.Reactions
            .GroupBy(r => r.Type)
            .ToDictionary(g => g.Key.ToString(), g => g.Count()));

    private async Task<PostDetailResponse> MapToDetailAsync(Post p, Guid? currentUserId, CancellationToken ct)
    {
        var myReaction = currentUserId.HasValue
            ? p.Reactions.FirstOrDefault(r => r.UserId == currentUserId.Value)?.Type.ToString()
            : null;

        var reactions = p.Reactions
            .GroupBy(r => r.Type)
            .ToDictionary(g => g.Key.ToString(), g => g.Count());

        // Считаем просмотры отдельным запросом — Include(p.Views) тяжёл для больших статей
        var viewCount = await uow.PostViews.GetCountByPostAsync(p.Id, ct);
        var commentCount = await uow.Comments.GetCountByPostAsync(p.Id, ct);

        return new PostDetailResponse(
            p.Id, p.Title, p.Slug, p.Excerpt, p.CoverImageUrl,
            p.ReadTimeMinutes, p.Status, p.PublishedAt, p.CreatedAt,
            new AuthorDto(p.Author.UserId, p.Author.UserName ?? "", $"{p.Author.FirstName} {p.Author.LastName}".Trim(),
                p.Author.AvatarObjectName),
            new CategoryDto(p.Category.Id, p.Category.Name, p.Category.Slug, p.Category.Color, p.Category.IconUrl),
            p.PostTags.Select(pt => pt.Tag.Name).ToList(),
            p.Blocks.OrderBy(b => b.Order).Select(b => new ContentBlockDto(
                b.Id, b.Type, b.Order, b.Content, b.Language,
                b.ImageUrl, b.ImageCaption, b.CalloutType)).ToList(),
            reactions,
            myReaction,
            ViewCount: viewCount,
            CommentCount: commentCount);
    }

    // Извлекаем objectName из публичного URL
    // http://host/files/covers/uuid_file.jpg → covers/uuid_file.jpg
    private static string? ExtractObjectName(string url)
    {
        const string marker = "/files/";
        var idx = url.IndexOf(marker, StringComparison.Ordinal);
        return idx >= 0 ? url[(idx + marker.Length)..] : null;
    }
}