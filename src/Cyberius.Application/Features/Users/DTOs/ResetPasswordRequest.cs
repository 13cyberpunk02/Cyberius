namespace Cyberius.Application.Features.Users.DTOs;

public record ResetPasswordRequest(string Token, string NewPassword);