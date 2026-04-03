using Cyberius.Domain.Entities;
using Cyberius.Domain.Interfaces;
using Cyberius.Infrastructure.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace Cyberius.Infrastructure.Data.Repositories;

public class CommentRepository(AppDbContext db)
    : GenericRepository<Comment>(db), ICommentRepository
{
    private readonly AppDbContext _db = db;

    public async Task<(IReadOnlyList<Comment> Items, int TotalCount)> GetByPostIdAsync(
        Guid postId, int page, int pageSize, CancellationToken ct = default)
    {
        // Только корневые НЕ удалённые комментарии (или удалённые но с ответами)
        var query = _db.Comments
            .Where(c => c.PostId == postId
                        && c.ParentCommentId == null
                        && (!c.IsDeleted || c.Replies.Any(r => !r.IsDeleted)))
            .Include(c => c.Author)
            .Include(c => c.Reactions)
            .Include(c => c.Replies.Where(r => !r.IsDeleted).OrderBy(r => r.CreatedAt))
            .ThenInclude(r => r.Author)
            .Include(c => c.Replies)
            .ThenInclude(r => r.Reactions)
            .OrderByDescending(c => c.CreatedAt);

        var total = await query.CountAsync(ct);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .AsNoTracking()
            .ToListAsync(ct);

        return (items, total);
    }

    public async Task<IReadOnlyList<Comment>> GetRepliesAsync(
        Guid parentCommentId, CancellationToken ct = default) =>
        await _db.Comments
            .Where(c => c.ParentCommentId == parentCommentId && !c.IsDeleted)
            .Include(c => c.Author)
            .Include(c => c.Reactions)
            .OrderBy(c => c.CreatedAt)
            .AsNoTracking()
            .ToListAsync(ct);

    public async Task<(IReadOnlyList<Comment> Items, int TotalCount)> GetByAuthorAsync(
        Guid authorId, int page, int pageSize, CancellationToken ct = default)
    {
        var query = _db.Comments
            .Where(c => c.AuthorId == authorId && !c.IsDeleted)
            .Include(c => c.Post)
            .OrderByDescending(c => c.CreatedAt);

        var total = await query.CountAsync(ct);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .AsNoTracking()
            .ToListAsync(ct);

        return (items, total);
    }

    public async Task<Comment?> GetWithAuthorAsync(Guid id, CancellationToken ct = default) =>
        await _db.Comments
            .Include(c => c.Author)
            .Include(c => c.Reactions)
            .Include(c => c.Replies.Where(r => !r.IsDeleted).OrderBy(r => r.CreatedAt))
            .ThenInclude(r => r.Author)
            .Include(c => c.Replies)
            .ThenInclude(r => r.Reactions)
            .FirstOrDefaultAsync(c => c.Id == id, ct);

    public async Task<int> GetCountByPostAsync(
        Guid postId, CancellationToken ct = default) =>
        await _db.Comments
            .CountAsync(c => c.PostId == postId && !c.IsDeleted, ct);

    public async Task<Dictionary<Guid, int>> GetCountsByPostsAsync(
        IEnumerable<Guid> postIds, CancellationToken ct = default)
    {
        var ids = postIds.ToList();
        return await _db.Comments
            .Where(c => ids.Contains(c.PostId) && !c.IsDeleted)
            .GroupBy(c => c.PostId)
            .Select(g => new { PostId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.PostId, x => x.Count, ct);
    }
}