using Cyberius.Domain.Entities;
using Cyberius.Domain.Interfaces;
using Cyberius.Infrastructure.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace Cyberius.Infrastructure.Data.Repositories;

public class EmailTokenRepository(AppDbContext db)
    : GenericRepository<EmailToken>(db), IEmailTokenRepository
{
    private readonly AppDbContext _db = db;

    public async Task<EmailToken?> GetByTokenAsync(
        string token, CancellationToken ct = default) =>
        await _db.EmailTokens
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.Token == token, ct);
 
    public async Task RemoveAllByUserAsync(Guid userId, CancellationToken ct = default)
    {
        var tokens = await _db.EmailTokens
            .Where(t => t.UserId == userId)
            .ToListAsync(ct);
        _db.EmailTokens.RemoveRange(tokens);
    }
}