using System.Globalization;
using System.Security.Claims;
using Cyberius.Domain.Entities;
using Cyberius.Domain.Options;
using Microsoft.Extensions.Options;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace Cyberius.Application.Features.JWT;

public class JwtService(IOptionsMonitor<JwtOptions> jwtOptions) : IJwtService
{
    private readonly JwtOptions _jwtOptions = jwtOptions.CurrentValue;
    
    public string GenerateJwtToken(User user)
    {
        List<Claim> claims =
        [
            new (ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new (JwtRegisteredClaimNames.Email, user.Email),
            new (JwtRegisteredClaimNames.Name, user.UserName),
            new(JwtRegisteredClaimNames.Exp, 
                DateTime.UtcNow.AddMinutes(_jwtOptions.AccessTokenLifetimeInMinutes).ToString(CultureInfo.CurrentCulture))
        ];
        claims.AddRange(user.UserRoles.Select(ur => new Claim(ClaimTypes.Role, ur.Role.Name)));
        
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.SecretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha512);

        var token = new JwtSecurityToken(
            issuer: _jwtOptions.Issuer,
            audience: _jwtOptions.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_jwtOptions.AccessTokenLifetimeInMinutes),
            signingCredentials: credentials);
        
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string GenerateRefreshToken()
    {
        var randomNumber = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }

    public ClaimsPrincipal? GetPrincipalFromExpiredToken(string accessToken)
    {
        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = true,
            ValidateIssuer = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = _jwtOptions.Issuer,
            ValidAudience = _jwtOptions.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.SecretKey)),
            ValidateLifetime = false 
        };
        
        var tokenHandler = new JwtSecurityTokenHandler();
            
        try
        {
            var principal = tokenHandler.ValidateToken(accessToken, tokenValidationParameters, out var securityToken);
                
            if (securityToken is not JwtSecurityToken jwtSecurityToken 
                || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha512, StringComparison.InvariantCultureIgnoreCase))
            {
                return null;
            }

            return principal;
        }
        catch(Exception ex)
        {
            // Временно — чтобы увидеть реальную причину
            Console.WriteLine($"GetPrincipalFromExpiredToken failed: {ex.GetType().Name}: {ex.Message}");
            return null;
        }
    }
}