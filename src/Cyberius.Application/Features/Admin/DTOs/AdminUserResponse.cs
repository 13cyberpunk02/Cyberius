namespace Cyberius.Application.Features.Admin.DTOs;

public record AdminUserResponse(
    Guid Id,
    string Email,
    string UserName,
    string FirstName,
    string LastName,
    string? AvatarUrl,
    bool IsActive,
    DateTime JoinedDate,
    List<string> Roles,
    int PostCount
);