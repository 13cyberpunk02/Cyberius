using Cyberius.Domain.Entities;
using Cyberius.Domain.Interfaces;
using Cyberius.Infrastructure.Data.Context;

namespace Cyberius.Infrastructure.Data.Repositories;

public class RoleRepository(AppDbContext db) : GenericRepository<Role>(db), IRoleRepository
{
    
}