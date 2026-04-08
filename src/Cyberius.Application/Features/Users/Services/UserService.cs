using Cyberius.Application.Features.Users.DTOs;
using Cyberius.Application.Features.Users.Interfaces;
using Cyberius.Domain.Entities;
using Cyberius.Domain.Interfaces;
using Cyberius.Domain.Shared;
using Microsoft.AspNetCore.Http;

namespace Cyberius.Application.Features.Users.Services;

public class UserService(IUnitOfWork uow, IStorageService storage) : IUserService
{
    public async Task<Result<UserResponse>> Me(Guid userId, CancellationToken cancellationToken = default)
    {
        if (userId.Equals(Guid.Empty))
            return Errors.BadRequest("ID пользователя не передано");

        var user = await uow.Users.GetUserWithRolesByIdAsync(userId, cancellationToken);
        if (user is null || user.IsDeleted || !user.IsActive)
            return Errors.NotFound(nameof(User), userId.ToString());
        
        var roles = user.UserRoles.Select(ur => ur.Role.Name).ToList();

        return Result<UserResponse>.Success(new UserResponse(
            user.UserId,
            user.UserName,
            user.Email,
            user.FirstName,
            user.LastName,
            user.AvatarObjectName,
            user.DateOfBirth,
            user.JoinedDate,
            roles));
    }

    public async Task<Result<UserResponse>> UpdateUserAsync(Guid userId, IFormFile? avatar, UpdateUserRequest request,
        CancellationToken cancellationToken = default)
    {
        var user = await uow.Users.GetUserWithRolesByIdAsync(userId, cancellationToken);
        if(user is null)
            return Errors.NotFound(nameof(User), userId);

        if (avatar is not null)
        {
            if(!string.IsNullOrEmpty(user.AvatarObjectName))
                await storage.DeleteAsync(user.AvatarObjectName, cancellationToken);
            
            await using var stream = avatar.OpenReadStream();
            
            var uploadResult = await storage.UploadAsync(
                stream,
                avatar.FileName,
                avatar.ContentType,
                cancellationToken);
            
            if(uploadResult.IsFailure)
                return uploadResult.Error;
            user.AvatarObjectName = uploadResult.Value;
        }
        user.FirstName = request.FirstName;
        user.LastName = request.LastName;
        user.DateOfBirth = request.DateOfBirth;
        
        uow.Users.Update(user);
        await uow.SaveChangesAsync(cancellationToken);
        
        return MapToResponse(user);
    }

    public async Task<Result<string>> ChangePasswordAsync(Guid userId, ChangePasswordRequest request, CancellationToken cancellationToken = default)
    {
        var user = await uow.Users.GetByIdAsync(userId, cancellationToken);
        if(user is null)
            return Errors.NotFound(nameof(User), userId.ToString());
        
        var isOldPasswordCorrect = BCrypt.Net.BCrypt.Verify(request.OldPassword, user.PasswordHash);
        if (!isOldPasswordCorrect)
            return Errors.BadRequest("Старый пароль не совпадает, проверьте правильно ли вы набрали ваш старый пароль");
        
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        uow.Users.Update(user);
        await uow.SaveChangesAsync(cancellationToken);
        return Result<string>.Success("Пароль успешно изменен");
    }

    private UserResponse MapToResponse(User user)
    {
        var avatarUrl = string.IsNullOrEmpty(user.AvatarObjectName)
            ? null
            : storage.GetPublicUrl(user.AvatarObjectName);

        return new UserResponse(
            user.UserId,
            user.UserName,
            user.Email,
            user.FirstName,
            user.LastName,
            avatarUrl,
            user.DateOfBirth,
            user.JoinedDate,
            user.UserRoles.Select(r => r.Role.Name).ToList());
    }
}