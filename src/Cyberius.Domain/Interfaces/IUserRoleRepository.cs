using Cyberius.Domain.Entities;

namespace Cyberius.Domain.Interfaces;

public interface IUserRoleRepository : IGenericRepository<UserRole>
{
    Task<IReadOnlyList<string>> GetRolesByUserAsync(Guid userId, CancellationToken ct = default);
    Task RemoveAllByUserAsync(Guid userId, CancellationToken ct = default);
}