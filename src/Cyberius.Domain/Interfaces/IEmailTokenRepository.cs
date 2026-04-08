using Cyberius.Domain.Entities;

namespace Cyberius.Domain.Interfaces;

public interface IEmailTokenRepository : IGenericRepository<EmailToken>
{
    Task<EmailToken?> GetByTokenAsync(string token, CancellationToken ct = default);
    Task RemoveAllByUserAsync(Guid userId, CancellationToken ct = default);
}