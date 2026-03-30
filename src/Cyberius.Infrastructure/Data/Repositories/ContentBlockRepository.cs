using Cyberius.Domain.Entities;
using Cyberius.Domain.Interfaces;
using Cyberius.Infrastructure.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace Cyberius.Infrastructure.Data.Repositories;

public class ContentBlockRepository(AppDbContext db)
    : GenericRepository<ContentBlock>(db), IContentBlockRepository
{
    private readonly AppDbContext _db = db;

    public async Task<IReadOnlyList<ContentBlock>> GetByPostIdAsync(
        Guid postId, CancellationToken ct = default) =>
        await _db.ContentBlocks
            .Where(b => b.PostId == postId)
            .OrderBy(b => b.Order)
            .AsNoTracking()
            .ToListAsync(ct);
 
    public async Task<int> GetMaxOrderAsync(
        Guid postId, CancellationToken ct = default) =>
        await _db.ContentBlocks
            .Where(b => b.PostId == postId)
            .MaxAsync(b => (int?)b.Order, ct) ?? 0;
 
    public async Task RemoveAllByPostIdAsync(
        Guid postId, CancellationToken ct = default) =>
        await _db.ContentBlocks
            .Where(b => b.PostId == postId)
            .ExecuteDeleteAsync(ct);
}