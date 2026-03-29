using System.IdentityModel.Tokens.Jwt;
using Cyberius.Application.Features.Authentication.DTOs;
using Cyberius.Application.Features.Authentication.Interfaces;
using Cyberius.Application.Features.JWT;
using Cyberius.Domain.Entities;
using Cyberius.Domain.Interfaces;
using Cyberius.Domain.Shared;

namespace Cyberius.Application.Features.Authentication.Services;

public class AuthenticationService(IUnitOfWork uof, IJwtService jwtService) : IAuthenticationService
{
    public async Task<Result<LoginResponse>> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var user = await uof.Users.GetByEmailAsync(request.Email, cancellationToken);
        if(user is null)
            return Errors.NotFound(nameof(User), "");

        var isPasswordCorrect = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);
        if (!isPasswordCorrect)
            return Errors.Unauthorized("Введены неверные данные");
        
        var accessToken = jwtService.GenerateJwtToken(user);
        var refreshToken = jwtService.GenerateRefreshToken();

        var refreshTokenEntity = await uof.RefreshTokens.GetRefreshTokenByUserId(user.UserId, cancellationToken);
        if (refreshTokenEntity is null)
        {
            var newRefreshToken = new RefreshToken
            {
                Token = refreshToken,
                User = user
            };
            await uof.RefreshTokens.AddAsync(newRefreshToken, cancellationToken);
        }
        else
        {
            refreshTokenEntity.Token = refreshToken;
            refreshTokenEntity.User = user;
            uof.RefreshTokens.Update(refreshTokenEntity);
        }
        
        await uof.SaveChangesAsync(cancellationToken);
        return Result<LoginResponse>.Success(new LoginResponse(accessToken, refreshToken));
    }

    public async Task<Result<LoginResponse>> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        var userExists = await uof.Users.GetByEmailAsync(request.Email, cancellationToken);
        if (userExists is not null)
            return Errors.Conflict("Пользователь с такой эл. почтой уже зарегистрирован");

        var newUser = new User
        {
            Email = request.Email,
            UserName = request.UserName,
            FirstName = request.FirstName,
            LastName = request.LastName,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            DateOfBirth = request.DateOfBirth,
            JoinedDate = DateTime.UtcNow
        };

        await uof.Users.AddAsync(newUser, cancellationToken);
        var role = await uof.Roles.GetByNameAsync("User", cancellationToken);

        var userRoleEntity = new UserRole
        {
            RoleId = role.RoleId,
            UserId = newUser.UserId
        };

        await uof.UserRoles.AddAsync(userRoleEntity, cancellationToken);
        await uof.SaveChangesAsync(cancellationToken);

        var user = await uof.Users.GetByIdAsync(newUser.UserId, cancellationToken);
        if (user is null)
            return Errors.NotFound(nameof(User), newUser.UserId.ToString());
        
        var accessToken = jwtService.GenerateJwtToken(user);
        var refreshToken = jwtService.GenerateRefreshToken();

        var refreshTokenEntity = new RefreshToken
        {
            Token = refreshToken,
            User = user
        };

        await uof.RefreshTokens.AddAsync(refreshTokenEntity, cancellationToken);
        await uof.SaveChangesAsync(cancellationToken);
        
        return Result<LoginResponse>.Success(new LoginResponse(accessToken, refreshToken));
    }

    public async Task<Result<LoginResponse>> RefreshTokenAsync(RefreshTokenRequest request, CancellationToken cancellationToken = default)
    {
        var principal = jwtService.GetPrincipalFromExpiredToken(request.AccessToken);
        if(principal is null)
            return Errors.BadRequest("Токен доступа не валидный");
        
        var userIdClaim = principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
        if (string.IsNullOrWhiteSpace(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            return Errors.BadRequest("Токен доступа не валидный");
        
        var user = await uof.Users.GetByIdAsync(userId, cancellationToken);
        if (user is null)
            return Errors.BadRequest("Токен доступа не валидный");
        
        var refreshTokenEntity = await uof.RefreshTokens.GetRefreshTokenByUserId(user.UserId, cancellationToken);
        if(refreshTokenEntity is null || refreshTokenEntity.Token != request.RefreshToken)
            return Errors.BadRequest("Токен доступа не валидный");
        
        var newAccessToken = jwtService.GenerateJwtToken(user);
        var newRefreshToken = jwtService.GenerateRefreshToken();
        
        refreshTokenEntity.Token = newRefreshToken;
        refreshTokenEntity.User = user;
        uof.RefreshTokens.Update(refreshTokenEntity);
        
        await uof.SaveChangesAsync(cancellationToken);
        
        return Result<LoginResponse>.Success(new LoginResponse(newAccessToken, newRefreshToken));
    }

    public async Task<Result<string>> LogoutAsync(Guid requestUserId, Guid userId, CancellationToken cancellationToken = default)
    {
        if (requestUserId != userId)
            return Errors.BadRequest(
                "ID авторизованного пользователя и ID пользователя отправившего запрос на выход из системы не совпадают");

        var user = await uof.Users.GetByIdAsync(userId, cancellationToken);
        if(user is null)
            return Errors.NotFound(nameof(User), userId.ToString());
        
        var refreshTokenEntity = await uof.RefreshTokens.GetRefreshTokenByUserId(user.UserId,  cancellationToken);
        if (refreshTokenEntity is null)
            return Errors.BadRequest("Вы и не были авторизованы, чтобы выйти из системы");

        refreshTokenEntity.Token = string.Empty;
        uof.RefreshTokens.Update(refreshTokenEntity);
        await uof.SaveChangesAsync(cancellationToken);
        return Result<string>.Success("Вы теперь не авторизованы в системе");
    }
}