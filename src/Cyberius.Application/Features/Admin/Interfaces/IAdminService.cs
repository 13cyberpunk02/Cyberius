using Cyberius.Application.Features.Admin.DTOs;

namespace Cyberius.Application.Features.Admin.Interfaces;

public interface IAdminService
{
    Task<Result<PagedAdminUsersResponse>> GetAllAsync(
        int page, int pageSize, string? search,
        CancellationToken ct = default);
 
    Task<Result<AdminUserResponse>> GetByIdAsync(
        Guid userId,
        CancellationToken ct = default);
 
    Task<Result> ChangeRoleAsync(
        Guid currentUserId, Guid targetUserId, string roleName,
        CancellationToken ct = default);
 
    Task<Result<bool>> ToggleBlockAsync(
        Guid currentUserId, Guid targetUserId,
        CancellationToken ct = default);
 
    Task<Result> DeleteAsync(
        Guid currentUserId, Guid targetUserId,
        CancellationToken ct = default);
}