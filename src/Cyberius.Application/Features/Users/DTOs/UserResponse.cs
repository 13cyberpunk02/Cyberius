namespace Cyberius.Application.Features.Users.DTOs;

public record UserResponse(
    Guid UserId, 
    string Username,
    string Email,
    string FirstName, 
    string LastName,
    string? AvatarUrl,
    DateOnly DateOfBirth,
    DateTime JoinedDate,
    List<string>? Roles);