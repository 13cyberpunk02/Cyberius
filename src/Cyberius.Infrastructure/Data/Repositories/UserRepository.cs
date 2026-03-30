using Cyberius.Domain.Entities;
using Cyberius.Domain.Interfaces;
using Cyberius.Infrastructure.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace Cyberius.Infrastructure.Data.Repositories;

public class UserRepository(AppDbContext db) : GenericRepository<User>(db), IUserRepository
{
    private readonly AppDbContext _db = db;

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default) => 
        await _db.Users
            .Include(ur => ur.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);

    public async Task<User?> GetUserWithRolesByIdAsync(Guid userId, CancellationToken cancellationToken = default) =>
        await _db.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(x => x.UserId == userId, cancellationToken);
}