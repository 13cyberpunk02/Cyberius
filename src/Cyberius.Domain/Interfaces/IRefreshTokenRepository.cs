using Cyberius.Domain.Entities;

namespace Cyberius.Domain.Interfaces;

public interface IRefreshTokenRepository : IGenericRepository<RefreshToken>
{
    Task<RefreshToken?> GetRefreshTokenByUserId(Guid userId, CancellationToken cancellationToken = default);
    Task RemoveByUserAsync(Guid userId, CancellationToken cancellationToken = default);
}