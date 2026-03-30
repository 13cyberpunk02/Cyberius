using Cyberius.Domain.Entities;
using Cyberius.Domain.Entities.Enums;
using Cyberius.Domain.Interfaces;
using Cyberius.Infrastructure.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace Cyberius.Infrastructure.Data.Repositories;

public class TagRepository(AppDbContext db)
    : GenericRepository<Tag>(db), ITagRepository
{
    private readonly AppDbContext _db = db;

    public async Task<Tag?> GetBySlugAsync(
        string slug, CancellationToken ct = default) =>
        await _db.Tags
            .FirstOrDefaultAsync(t => t.Slug == slug, ct);
 
    public async Task<IReadOnlyList<Tag>> GetByNamesAsync(
        IEnumerable<string> names, CancellationToken ct = default)
    {
        var lower = names.Select(n => n.ToLower()).ToList();
        return await _db.Tags
            .Where(t => lower.Contains(t.Name.ToLower()))
            .ToListAsync(ct);
    }
 
    public async Task<IReadOnlyList<(Tag Tag, int PostCount)>> GetPopularAsync(
        int count = 20, CancellationToken ct = default)
    {
        var result = await _db.Tags
            .Select(t => new
            {
                Tag       = t,
                PostCount = t.PostTags.Count(pt => pt.Post.Status == PostStatus.Published),
            })
            .Where(x => x.PostCount > 0)
            .OrderByDescending(x => x.PostCount)
            .Take(count)
            .AsNoTracking()
            .ToListAsync(ct);
 
        return result
            .Select(x => (x.Tag, x.PostCount))
            .ToList();
    }
 
    public async Task<bool> SlugExistsAsync(
        string slug, Guid? excludeId = null, CancellationToken ct = default) =>
        await _db.Tags.AnyAsync(t =>
            t.Slug == slug && (excludeId == null || t.Id != excludeId), ct);
}