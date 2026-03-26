using Cyberius.Application.Features.Users.DTOs;

namespace Cyberius.Application.Features.Users.Interfaces;

public interface IUserService
{
    Task<Result<UserResponse>> Me(Guid userId, CancellationToken cancellationToken = default);
}