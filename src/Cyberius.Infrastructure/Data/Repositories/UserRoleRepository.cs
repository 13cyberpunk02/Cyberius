using Cyberius.Domain.Entities;
using Cyberius.Domain.Interfaces;
using Cyberius.Infrastructure.Data.Context;

namespace Cyberius.Infrastructure.Data.Repositories;

public class UserRoleRepository(AppDbContext db) : GenericRepository<UserRole>(db), IUserRoleRepository
{
    
}