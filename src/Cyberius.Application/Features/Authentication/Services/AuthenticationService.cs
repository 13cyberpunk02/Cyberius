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
            return Errors.NotFound(nameof(request), "");

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
            DateOfBirth = request.DateOfBirth.ToUniversalTime(),
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
}