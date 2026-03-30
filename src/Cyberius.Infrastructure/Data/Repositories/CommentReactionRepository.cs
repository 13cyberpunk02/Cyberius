using Cyberius.Domain.Entities;
using Cyberius.Domain.Entities.Enums;
using Cyberius.Domain.Interfaces;
using Cyberius.Infrastructure.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace Cyberius.Infrastructure.Data.Repositories;

public class CommentReactionRepository(AppDbContext db)
    : GenericRepository<CommentReaction>(db), ICommentReactionRepository
{
    private readonly AppDbContext _db = db;

    public async Task<CommentReaction?> GetByCommentAndUserAsync(
        Guid commentId, Guid userId, CancellationToken ct = default) =>
        await _db.CommentReactions
            .FirstOrDefaultAsync(r => r.CommentId == commentId && r.UserId == userId, ct);
 
    public async Task<Dictionary<ReactionType, int>> GetCountsByCommentAsync(
        Guid commentId, CancellationToken ct = default) =>
        await _db.CommentReactions
            .Where(r => r.CommentId == commentId)
            .GroupBy(r => r.Type)
            .Select(g => new { Type = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Type, x => x.Count, ct);
 
    public async Task<Dictionary<Guid, Dictionary<ReactionType, int>>> GetCountsByCommentsAsync(
        IEnumerable<Guid> commentIds, CancellationToken ct = default)
    {
        var ids = commentIds.ToList();
 
        var raw = await _db.CommentReactions
            .Where(r => ids.Contains(r.CommentId))
            .GroupBy(r => new { r.CommentId, r.Type })
            .Select(g => new
            {
                g.Key.CommentId,
                g.Key.Type,
                Count = g.Count(),
            })
            .ToListAsync(ct);
 
        return raw
            .GroupBy(x => x.CommentId)
            .ToDictionary(
                g => g.Key,
                g => g.ToDictionary(x => x.Type, x => x.Count));
    }
 
    public async Task RemoveByCommentAndUserAsync(
        Guid commentId, Guid userId, CancellationToken ct = default) =>
        await _db.CommentReactions
            .Where(r => r.CommentId == commentId && r.UserId == userId)
            .ExecuteDeleteAsync(ct);
}