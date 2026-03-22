using Cyberius.Domain.Interfaces;
using Cyberius.Infrastructure.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace Cyberius.Infrastructure.Data.Repositories;

public class UnitOfWork(AppDbContext db) : IUnitOfWork
{
    public IUserRepository         Users         => field is null ? field ??= new  UserRepository(db) : field;
    public IRoleRepository         Roles         => field is null ? field ??= new  RoleRepository(db) : field;
    public IUserRoleRepository     UserRoles     => field is null ? field ??= new  UserRoleRepository(db) : field;
    public IRefreshTokenRepository RefreshTokens => field is null ? field ??= new RefreshTokenRepository(db) : field;
    public Task<int> SaveChangesAsync(CancellationToken ct = default)
        => db.SaveChangesAsync(ct);

    public async Task ExecuteInTransactionAsync(
        Func<Task> operation,
        CancellationToken ct = default)
    {
        var strategy = db.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            await using var tx = await db.Database.BeginTransactionAsync(ct);
            try
            {
                await operation();
                await db.SaveChangesAsync(ct);
                await tx.CommitAsync(ct);
            }
            catch
            {
                await tx.RollbackAsync(ct);
                throw;
            }
        });
    }
    
    public ValueTask DisposeAsync() => db.DisposeAsync();
}