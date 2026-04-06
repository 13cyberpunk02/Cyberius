using Cyberius.Domain.Entities;
using Cyberius.Domain.Interfaces;
using Cyberius.Infrastructure.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace Cyberius.Infrastructure.Data.Repositories;

public class UserRoleRepository(AppDbContext db) : GenericRepository<UserRole>(db), IUserRoleRepository
{
    private readonly AppDbContext _db = db;

    public async Task<IReadOnlyList<string>> GetRolesByUserAsync(Guid userId, CancellationToken ct = default)
        => await _db.UserRoles
            .Where(ur => ur.UserId == userId)
            .Include(ur => ur.Role)
            .Select(ur => ur.Role.Name) 
            .ToListAsync(ct);

    public async Task RemoveAllByUserAsync(Guid userId, CancellationToken ct = default)
    {
        var userRoles = await _db.UserRoles
            .Where(ur => ur.UserId == userId)
            .ToListAsync(ct);

        _db.UserRoles.RemoveRange(userRoles);
    }
}