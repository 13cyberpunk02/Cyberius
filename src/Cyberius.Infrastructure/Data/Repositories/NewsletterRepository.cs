using Cyberius.Domain.Entities;
using Cyberius.Domain.Interfaces;
using Cyberius.Infrastructure.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace Cyberius.Infrastructure.Data.Repositories;

public class NewsletterRepository(AppDbContext db)
    : GenericRepository<NewsletterSubscriber>(db), INewsletterRepository
{
    private readonly AppDbContext _db = db;

    public async Task<NewsletterSubscriber?> GetByEmailAsync(
        string email, CancellationToken ct = default) =>
        await _db.NewsletterSubscribers
            .FirstOrDefaultAsync(s => s.Email == email.ToLower(), ct);
 
    public async Task<NewsletterSubscriber?> GetByTokenAsync(
        string token, CancellationToken ct = default) =>
        await _db.NewsletterSubscribers
            .FirstOrDefaultAsync(s => s.UnsubToken == token, ct);
 
    public async Task<IReadOnlyList<NewsletterSubscriber>> GetAllActiveAsync(
        CancellationToken ct = default) =>
        await _db.NewsletterSubscribers
            .Where(s => s.IsActive)
            .ToListAsync(ct);
}