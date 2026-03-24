using Cyberius.Domain.Entities;
using Cyberius.Domain.Interfaces;
using Cyberius.Infrastructure.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace Cyberius.Infrastructure.Data.Repositories;

public class RoleRepository(AppDbContext db) : GenericRepository<Role>(db), IRoleRepository
{
    private readonly AppDbContext _db = db;

    public async Task<Role?> GetByNameAsync(string roleName, CancellationToken cancellationToken) =>
        await _db.Roles.FirstOrDefaultAsync(r => r.Name == roleName, cancellationToken);
}