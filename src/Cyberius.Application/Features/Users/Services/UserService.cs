using Cyberius.Application.Features.Users.DTOs;
using Cyberius.Application.Features.Users.Interfaces;
using Cyberius.Domain.Entities;
using Cyberius.Domain.Interfaces;
using Cyberius.Domain.Shared;

namespace Cyberius.Application.Features.Users.Services;

public class UserService(IUnitOfWork uow) : IUserService
{
    public async Task<Result<UserResponse>> Me(Guid userId, CancellationToken cancellationToken = default)
    {
        if (userId.Equals(Guid.Empty))
            return Errors.BadRequest("ID пользователя не передано");

        var user = await uow.Users.GetUserWithRolesByIdAsync(userId, cancellationToken);
        if (user is null)
            return Errors.NotFound(nameof(User), userId.ToString());
        
        var roles = user.UserRoles.Select(ur => ur.Role.Name).ToList();

        return Result<UserResponse>.Success(new UserResponse(
            user.UserId,
            user.UserName,
            user.Email,
            user.FirstName,
            user.LastName,
            user.AvatarUrl,
            user.DateOfBirth,
            user.JoinedDate,
            roles));
    }
}