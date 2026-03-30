using Cyberius.Domain.Entities;
using Cyberius.Domain.Entities.Enums;
using Cyberius.Domain.Interfaces;
using Cyberius.Infrastructure.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace Cyberius.Infrastructure.Data.Repositories;

public class CategoryRepository(AppDbContext db)
    : GenericRepository<Category>(db), ICategoryRepository
{
    private readonly AppDbContext _db = db;

    public async Task<Category?> GetBySlugAsync(
        string slug, CancellationToken ct = default) =>
        await _db.Categories
            .FirstOrDefaultAsync(c => c.Slug == slug, ct);
 
    public async Task<IReadOnlyList<(Category Category, int PostCount)>> GetWithPostCountAsync(
        CancellationToken ct = default)
    {
        var result = await _db.Categories
            .Select(c => new
            {
                Category   = c,
                PostCount  = c.Posts.Count(p => p.Status == PostStatus.Published),
            })
            .OrderBy(x => x.Category.Name)
            .AsNoTracking()
            .ToListAsync(ct);
 
        return result
            .Select(x => (x.Category, x.PostCount))
            .ToList();
    }
 
    public async Task<bool> SlugExistsAsync(
        string slug, Guid? excludeId = null, CancellationToken ct = default) =>
        await _db.Categories.AnyAsync(c =>
            c.Slug == slug && (excludeId == null || c.Id != excludeId), ct);
}