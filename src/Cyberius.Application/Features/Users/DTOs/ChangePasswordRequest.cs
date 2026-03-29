namespace Cyberius.Application.Features.Users.DTOs;

public record ChangePasswordRequest(string OldPassword, string NewPassword, string ConfirmPassword);