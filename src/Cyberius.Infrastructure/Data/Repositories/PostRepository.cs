using Cyberius.Domain.Entities;
using Cyberius.Domain.Entities.Enums;
using Cyberius.Domain.Interfaces;
using Cyberius.Infrastructure.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace Cyberius.Infrastructure.Data.Repositories;

public class PostRepository(AppDbContext db)
    : GenericRepository<Post>(db), IPostRepository
{
    private readonly AppDbContext _db = db;

    public async Task<Post?> GetBySlugAsync(string slug, CancellationToken ct = default) =>
        await _db.Posts
            .Include(p => p.Author)
            .Include(p => p.Category)
            .Include(p => p.PostTags).ThenInclude(pt => pt.Tag)
            .FirstOrDefaultAsync(p => p.Slug == slug, ct);
 
    public async Task<Post?> GetForUpdateAsync(Guid id, CancellationToken ct = default) =>
        await _db.Posts
            .Include(p => p.PostTags)  // нужны чтобы EF мог сделать diff (удалить старые)
            .FirstOrDefaultAsync(p => p.Id == id, ct);
 
    public async Task<Post?> GetWithBlocksAsync(Guid id, CancellationToken ct = default) =>
        await _db.Posts
            .Include(p => p.Author)
            .Include(p => p.Category)
            .Include(p => p.PostTags).ThenInclude(pt => pt.Tag)
            .Include(p => p.Blocks.OrderBy(b => b.Order))
            .FirstOrDefaultAsync(p => p.Id == id, ct);
 
    public async Task<Post?> GetFullAsync(Guid id, CancellationToken ct = default) =>
        await _db.Posts
            .Include(p => p.Author)
            .Include(p => p.Category)
            .Include(p => p.PostTags).ThenInclude(pt => pt.Tag)
            .Include(p => p.Blocks.OrderBy(b => b.Order))
            .Include(p => p.Reactions)
            .Include(p => p.Comments.Where(c => c.ParentCommentId == null && !c.IsDeleted))
                .ThenInclude(c => c.Author)
            .Include(p => p.Comments)
                .ThenInclude(c => c.Replies.Where(r => !r.IsDeleted))
                .ThenInclude(r => r.Author)
            .FirstOrDefaultAsync(p => p.Id == id, ct);
 
    public async Task<(IReadOnlyList<Post> Items, int TotalCount)> GetPublishedPagedAsync(
        int page, int pageSize, CancellationToken ct = default)
    {
        var query = _db.Posts
            .Where(p => p.Status == PostStatus.Published)
            .Include(p => p.Author)
            .Include(p => p.Category)
            .Include(p => p.PostTags).ThenInclude(pt => pt.Tag)
            .Include(p => p.Reactions)
            .OrderByDescending(p => p.PublishedAt);
 
        var total = await query.CountAsync(ct);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .AsNoTracking()
            .ToListAsync(ct);
 
        return (items, total);
    }
 
    public async Task<(IReadOnlyList<Post> Items, int TotalCount)> GetByCategoryAsync(
        Guid categoryId, int page, int pageSize, CancellationToken ct = default)
    {
        var query = _db.Posts
            .Where(p => p.Status == PostStatus.Published && p.CategoryId == categoryId)
            .Include(p => p.Author)
            .Include(p => p.Category)
            .Include(p => p.PostTags).ThenInclude(pt => pt.Tag)
            .Include(p => p.Reactions)
            .OrderByDescending(p => p.PublishedAt);
 
        var total = await query.CountAsync(ct);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .AsNoTracking()
            .ToListAsync(ct);
 
        return (items, total);
    }
 
    public async Task<(IReadOnlyList<Post> Items, int TotalCount)> GetByTagAsync(
        string tagSlug, int page, int pageSize, CancellationToken ct = default)
    {
        var query = _db.Posts
            .Where(p => p.Status == PostStatus.Published
                     && p.PostTags.Any(pt => pt.Tag.Slug == tagSlug))
            .Include(p => p.Author)
            .Include(p => p.Category)
            .Include(p => p.PostTags).ThenInclude(pt => pt.Tag)
            .Include(p => p.Reactions)
            .OrderByDescending(p => p.PublishedAt);
 
        var total = await query.CountAsync(ct);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .AsNoTracking()
            .ToListAsync(ct);
 
        return (items, total);
    }
 
    public async Task<(IReadOnlyList<Post> Items, int TotalCount)> GetByAuthorAsync(
        Guid authorId, int page, int pageSize, CancellationToken ct = default)
    {
        var query = _db.Posts
            .Where(p => p.AuthorId == authorId && p.Status == PostStatus.Published)
            .Include(p => p.Author)
            .Include(p => p.Category)
            .Include(p => p.PostTags).ThenInclude(pt => pt.Tag)
            .Include(p => p.Reactions)
            .OrderByDescending(p => p.PublishedAt);
 
        var total = await query.CountAsync(ct);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .AsNoTracking()
            .ToListAsync(ct);
 
        return (items, total);
    }
 
    public async Task<(IReadOnlyList<Post> Items, int TotalCount)> SearchAsync(
        string query, int page, int pageSize, CancellationToken ct = default)
    {
        var q = query.Trim();
 
        var dbQuery = _db.Posts
            .Where(p => p.Status == PostStatus.Published
                     && (EF.Functions.ToTsVector("russian", p.Title + " " + (p.Excerpt ?? "") + " " + (p.Excerpt ?? ""))
                             .Matches(EF.Functions.PlainToTsQuery("russian", q))
                         || p.Title.ToLower().Contains(q.ToLower())
                         || (p.Excerpt != null && p.Excerpt.ToLower().Contains(q.ToLower()))
                         || p.PostTags.Any(pt => pt.Tag.Name.ToLower().Contains(q.ToLower()))))
            .Include(p => p.Author)
            .Include(p => p.Category)
            .Include(p => p.PostTags).ThenInclude(pt => pt.Tag)
            .Include(p => p.Reactions)
            .OrderByDescending(p =>
                EF.Functions.ToTsVector("russian", p.Title + " " + (p.Excerpt ?? ""))
                    .Rank(EF.Functions.PlainToTsQuery("russian", q)))
            .ThenByDescending(p => p.PublishedAt);
 
        var total = await dbQuery.CountAsync(ct);
        var items = await dbQuery
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .AsNoTracking()
            .ToListAsync(ct);
 
        return (items, total);
    }
 
    public async Task<IReadOnlyList<Post>> GetRelatedAsync(
        Guid postId, int count = 4, CancellationToken ct = default)
    {
        var post = await _db.Posts
            .Include(p => p.PostTags)
            .FirstOrDefaultAsync(p => p.Id == postId, ct);
 
        if (post is null) return [];
 
        var tagIds = post.PostTags.Select(pt => pt.TagId).ToList();
 
        return await _db.Posts
            .Where(p => p.Id != postId
                     && p.Status == PostStatus.Published
                     && (p.CategoryId == post.CategoryId
                      || p.PostTags.Any(pt => tagIds.Contains(pt.TagId))))
            .Include(p => p.Author)
            .Include(p => p.Category)
            .Include(p => p.PostTags).ThenInclude(pt => pt.Tag)
            .Include(p => p.Reactions)
            .OrderByDescending(p => p.PostTags.Count(pt => tagIds.Contains(pt.TagId)))
            .Take(count)
            .AsNoTracking()
            .ToListAsync(ct);
    }
 
    public async Task<bool> SlugExistsAsync(
        string slug, Guid? excludePostId = null, CancellationToken ct = default) =>
        await _db.Posts.AnyAsync(p =>
            p.Slug == slug && (excludePostId == null || p.Id != excludePostId), ct);
 
    public async Task<IReadOnlyList<Post>> GetDraftsByAuthorAsync(
        Guid authorId, CancellationToken ct = default) =>
        await _db.Posts
            .Where(p => p.AuthorId == authorId && p.Status == PostStatus.Draft)
            .Include(p => p.Author)
            .Include(p => p.Category)
            .Include(p => p.PostTags).ThenInclude(pt => pt.Tag)
            .Include(p => p.Reactions)
            .OrderByDescending(p => p.UpdatedAt)
            .AsNoTracking()
            .ToListAsync(ct);
}