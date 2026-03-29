using Cyberius.Application.Features.Users.DTOs;
using Microsoft.AspNetCore.Http;

namespace Cyberius.Application.Features.Users.Interfaces;

public interface IUserService
{
    Task<Result<UserResponse>> Me(Guid userId, CancellationToken cancellationToken = default);

    Task<Result<UserResponse>> UpdateUserAsync(Guid userId, IFormFile? avatar, UpdateUserRequest request,
        CancellationToken cancellationToken = default);
    
    Task<Result<string>> ChangePasswordAsync(Guid userId, ChangePasswordRequest request, CancellationToken cancellationToken = default);
}