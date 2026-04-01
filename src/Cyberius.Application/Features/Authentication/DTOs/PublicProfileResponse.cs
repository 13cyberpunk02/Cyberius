namespace Cyberius.Application.Features.Authentication.DTOs;

public record PublicProfileResponse(
    Guid   Id,
    string Username,
    string FirstName,
    string LastName,
    string? AvatarUrl,
    string JoinedDate,
    List<string> Roles
    );