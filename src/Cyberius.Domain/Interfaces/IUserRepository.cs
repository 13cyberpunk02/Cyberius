using Cyberius.Domain.Entities;

namespace Cyberius.Domain.Interfaces;

public interface IUserRepository : IGenericRepository<User>
{
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<User?> GetUserWithRolesByIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<(IReadOnlyList<User> Items, int TotalCount)> GetAllPagedAsync(
        int page, int pageSize, string? search, CancellationToken ct = default);
}