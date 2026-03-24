namespace Cyberius.Application.Features.Authentication.DTOs;

public record RegisterRequest(
    string Email, 
    string UserName, 
    string Password,
    string ConfirmPassword,
    string FirstName,
    string LastName,
    DateTime DateOfBirth);