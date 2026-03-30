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
        // Только корневые комментарии — ответы подтягиваются через Include
        var query = _db.Comments
            .Where(c => c.PostId == postId && c.ParentCommentId == null)
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
 
    public async Task<int> GetCountByPostAsync(
        Guid postId, CancellationToken ct = default) =>
        await _db.Comments
            .CountAsync(c => c.PostId == postId && !c.IsDeleted, ct);
}