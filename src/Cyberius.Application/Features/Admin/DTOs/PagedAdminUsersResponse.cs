namespace Cyberius.Application.Features.Admin.DTOs;

public record PagedAdminUsersResponse(
    List<AdminUserResponse> Items,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages
);