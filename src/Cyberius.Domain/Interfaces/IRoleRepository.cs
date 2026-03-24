using Cyberius.Domain.Entities;

namespace Cyberius.Domain.Interfaces;

public interface IRoleRepository : IGenericRepository<Role>
{
    Task<Role?> GetByNameAsync(string roleName, CancellationToken cancellationToken);
}