namespace Cyberius.Application.Features.Authentication.DTOs;

public record LoginResponse(string AccessToken, string RefreshToken);