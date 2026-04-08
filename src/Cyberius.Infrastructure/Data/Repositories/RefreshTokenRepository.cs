using Cyberius.Domain.Entities;
using Cyberius.Domain.Interfaces;
using Cyberius.Infrastructure.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace Cyberius.Infrastructure.Data.Repositories;

public class RefreshTokenRepository(AppDbContext db) : GenericRepository<RefreshToken>(db), IRefreshTokenRepository
{
    public async Task<RefreshToken?>
        GetRefreshTokenByUserId(Guid userId, CancellationToken cancellationToken = default) =>
        await db.RefreshTokens.SingleOrDefaultAsync(rt => rt.User.UserId == userId, cancellationToken);

    public async Task RemoveByUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var token = await db.RefreshTokens
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(t => t.User.UserId == userId, cancellationToken);

        if (token is not null)
            db.RefreshTokens.Remove(token);
    }
}