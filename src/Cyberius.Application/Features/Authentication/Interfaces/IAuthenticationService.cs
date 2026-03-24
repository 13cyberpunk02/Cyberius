using Cyberius.Application.Features.Authentication.DTOs;
using Cyberius.Domain.Shared;

namespace Cyberius.Application.Features.Authentication.Interfaces;

public interface IAuthenticationService
{
    Task<Result<LoginResponse>> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
    Task<Result<LoginResponse>> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);
}