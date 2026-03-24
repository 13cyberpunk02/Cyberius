namespace Cyberius.Application.Features.Authentication.DTOs;

public record RefreshTokenRequest(string AccessToken, string RefreshToken);