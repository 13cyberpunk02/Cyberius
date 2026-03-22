using Cyberius.Domain.Entities;

namespace Cyberius.Domain.Interfaces;

public interface IUnitOfWork : IAsyncDisposable
{
    IUserRepository      Users     { get; }
    IRoleRepository      Roles     { get; }
    IUserRoleRepository  UserRoles { get; }

    Task<int> SaveChangesAsync(CancellationToken ct = default);

    Task ExecuteInTransactionAsync(
        Func<Task> operation,
        CancellationToken ct = default);
}