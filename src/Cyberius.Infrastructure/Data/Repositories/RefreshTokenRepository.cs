using Cyberius.Domain.Entities;
using Cyberius.Domain.Interfaces;
using Cyberius.Infrastructure.Data.Context;

namespace Cyberius.Infrastructure.Data.Repositories;

public class RefreshTokenRepository(AppDbContext db) : GenericRepository<RefreshToken>(db), IRefreshTokenRepository 
{
    
}