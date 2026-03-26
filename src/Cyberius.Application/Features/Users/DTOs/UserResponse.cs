namespace Cyberius.Application.Features.Users.DTOs;

public record UserResponse(
    Guid UserId, 
    string Username,
    string Email,
    string FirstName, 
    string LastName,
    string? AvatarUrl,
    DateTime DateOfBirth,
    DateTime JoinedDate,
    List<string>? Roles);