namespace Cyberius.Application.Features.Users.DTOs;

public record UpdateUserRequest(
    string FirstName,
    string LastName,
    DateOnly DateOfBirth);