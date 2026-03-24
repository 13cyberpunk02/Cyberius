using Cyberius.Domain.Entities;
using Cyberius.Domain.Interfaces;
using Cyberius.Infrastructure.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace Cyberius.Infrastructure.Data.Repositories;

public class RefreshTokenRepository(AppDbContext db) : GenericRepository<RefreshToken>(db), IRefreshTokenRepository
{
    public async Task<RefreshToken?> GetRefreshTokenByUserId(Guid userId, CancellationToken cancellationToken = default) =>
        await db.RefreshTokens.SingleOrDefaultAsync(rt => rt.User.UserId == userId, cancellationToken);
}