using Cyberius.Domain.Entities;
using Cyberius.Domain.Entities.Enums;
using Cyberius.Domain.Interfaces;
using Cyberius.Infrastructure.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace Cyberius.Infrastructure.Data.Repositories;

public class PostReactionRepository(AppDbContext db)
    : GenericRepository<PostReaction>(db), IPostReactionRepository
{
    private readonly AppDbContext _db = db;

    public async Task<PostReaction?> GetByPostAndUserAsync(
        Guid postId, Guid userId, CancellationToken ct = default) =>
        await _db.PostReactions
            .FirstOrDefaultAsync(r => r.PostId == postId && r.UserId == userId, ct);
 
    public async Task<Dictionary<ReactionType, int>> GetCountsByPostAsync(
        Guid postId, CancellationToken ct = default) =>
        await _db.PostReactions
            .Where(r => r.PostId == postId)
            .GroupBy(r => r.Type)
            .Select(g => new { Type = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Type, x => x.Count, ct);
 
    public async Task RemoveByPostAndUserAsync(
        Guid postId, Guid userId, CancellationToken ct = default) =>
        await _db.PostReactions
            .Where(r => r.PostId == postId && r.UserId == userId)
            .ExecuteDeleteAsync(ct);
}