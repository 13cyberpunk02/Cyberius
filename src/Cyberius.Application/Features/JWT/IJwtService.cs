using System.Security.Claims;
using Cyberius.Domain.Entities;

namespace Cyberius.Application.Features.JWT;

public interface IJwtService
{
    public string GenerateJwtToken(User user);
    public string GenerateRefreshToken();
    public ClaimsPrincipal? GetPrincipalFromExpiredToken(string accessToken);
}