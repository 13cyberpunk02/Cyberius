using System.Linq.Expressions;
using Cyberius.Domain.Interfaces;
using Cyberius.Infrastructure.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace Cyberius.Infrastructure.Data.Repositories;

public class GenericRepository<T>(AppDbContext db) : IGenericRepository<T> where T : class
{
    private readonly DbSet<T> _set = db.Set<T>();
    
    public Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => _set.FindAsync([id], ct).AsTask();

    public async Task<IReadOnlyList<T>> GetAllAsync(CancellationToken ct = default)
        => await _set.AsNoTracking().ToListAsync(ct);

    public IAsyncEnumerable<T> StreamAllAsync(CancellationToken ct = default)
        => _set.AsNoTracking().AsAsyncEnumerable();

    public async Task<IReadOnlyList<T>> FindAsync(
        Expression<Func<T, bool>> predicate,
        CancellationToken ct = default)
        => await _set.AsNoTracking().Where(predicate).ToListAsync(ct);

    public Task AddAsync(T entity, CancellationToken ct = default)
        => _set.AddAsync(entity, ct).AsTask();

    public void Update(T entity) => _set.Update(entity);

    public void Remove(T entity) => _set.Remove(entity);
}