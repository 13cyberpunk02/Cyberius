using Cyberius.Domain.Entities;

namespace Cyberius.Domain.Interfaces;

public interface IUserRepository : IGenericRepository<User>
{
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<User?> GetUserWithRolesByIdAsync(Guid userId, CancellationToken cancellationToken = default);
}