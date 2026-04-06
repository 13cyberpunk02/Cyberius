using Cyberius.Application.Features.Admin.DTOs;
using Cyberius.Application.Features.Admin.Interfaces;
using Cyberius.Domain.Entities;
using Cyberius.Domain.Interfaces;
using Cyberius.Domain.Shared;

namespace Cyberius.Application.Features.Admin.Services;

public class AdminUserService(IUnitOfWork uow) : IAdminService
{
    public async Task<Result<PagedAdminUsersResponse>> GetAllAsync(
        int page, int pageSize, string? search, CancellationToken ct = default)
    {
        var (users, total) = await uow.Users.GetAllPagedAsync(page, pageSize, search, ct);

        var items = new List<AdminUserResponse>();
        foreach (var u in users)
        {
            var roles = await uow.UserRoles. GetRolesByUserAsync(u.UserId, ct);
            var postCount = await uow.Posts.CountByAuthorAsync(u.UserId, ct);

            items.Add(MapToDto(u, roles.ToList(), postCount));
        }

        return new PagedAdminUsersResponse(
            Items: items,
            TotalCount: total,
            Page: page,
            PageSize: pageSize,
            TotalPages: (int)Math.Ceiling((double)total / pageSize));
    }

    public async Task<Result<AdminUserResponse>> GetByIdAsync(
        Guid userId, CancellationToken ct = default)
    {
        var user = await uow.Users.GetByIdAsync(userId, ct);
        if (user is null)
            return Errors.NotFound(nameof(User), userId.ToString());

        var roles = await uow.UserRoles.GetRolesByUserAsync(userId, ct);
        var postCount = await uow.Posts.CountByAuthorAsync(userId, ct);

        return MapToDto(user, roles.ToList(), postCount);
    }

    public async Task<Result> ChangeRoleAsync(
        Guid currentUserId, Guid targetUserId, string roleName, CancellationToken ct = default)
    {
        if (currentUserId == targetUserId)
            return Errors.BadRequest("Нельзя изменить свою роль");

        var user = await uow.Users.GetByIdAsync(targetUserId, ct);
        if (user is null)
            return Errors.NotFound(nameof(User), targetUserId);

        var role = await uow.Roles.GetByNameAsync(roleName, ct);
        if (role is null)
            return Errors.BadRequest($"Роль '{roleName}' не найдена");

        await uow.UserRoles.RemoveAllByUserAsync(targetUserId, ct);
        await uow.UserRoles.AddAsync(new UserRole { UserId = targetUserId, RoleId = role.RoleId }, ct);
        await uow.SaveChangesAsync(ct);

        return Result.Success();
    }

    public async Task<Result<bool>> ToggleBlockAsync(
        Guid currentUserId, Guid targetUserId, CancellationToken ct = default)
    {
        if (currentUserId == targetUserId)
            return Errors.BadRequest("Нельзя заблокировать себя");

        var user = await uow.Users.GetByIdAsync(targetUserId, ct);
        if (user is null)
            return Errors.NotFound(nameof(User), targetUserId);

        user.IsActive = !user.IsActive;
        uow.Users.Update(user);
        await uow.SaveChangesAsync(ct);

        return user.IsActive;
    }

    public async Task<Result> DeleteAsync(
        Guid currentUserId, Guid targetUserId, CancellationToken ct = default)
    {
        if (currentUserId == targetUserId)
            return Errors.BadRequest("Нельзя удалить себя");

        var user = await uow.Users.GetByIdAsync(targetUserId, ct);
        if (user is null)
            return Errors.NotFound(nameof(User), targetUserId);

        uow.Users.Remove(user);
        await uow.SaveChangesAsync(ct);

        return Result.Success();
    }

    // ── Helpers ────────────────────────────────────────────────────
    private static AdminUserResponse MapToDto(
        User u, List<string> roles, int postCount) => new(
        Id: u.UserId,
        Email: u.Email,
        UserName: u.UserName,
        FirstName: u.FirstName,
        LastName: u.LastName,
        AvatarUrl: u.AvatarObjectName,
        IsActive: u.IsActive,
        JoinedDate: u.JoinedDate,
        Roles: roles,
        PostCount: postCount);
}