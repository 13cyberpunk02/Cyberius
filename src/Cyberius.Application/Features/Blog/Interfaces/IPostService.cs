using Cyberius.Application.Features.Blog.Posts.Models;
using Cyberius.Domain.Entities.Enums;

namespace Cyberius.Application.Features.Blog.Interfaces;

public interface IPostService
{
    Task<Result<PostDetailResponse>> GetByIdAsync(Guid id, Guid? currentUserId, CancellationToken ct = default);
    Task<Result<PostDetailResponse>> GetBySlugAsync(string slug, Guid? currentUserId, CancellationToken ct = default);

    Task<Result<PagedResponse<PostSummaryResponse>>> GetPublishedAsync(int page, int pageSize,
        CancellationToken ct = default);

    Task<Result<PagedResponse<PostSummaryResponse>>> GetByCategoryAsync(Guid categoryId, int page, int pageSize,
        CancellationToken ct = default);

    Task<Result<PagedResponse<PostSummaryResponse>>> GetByTagAsync(string tagSlug, int page, int pageSize,
        CancellationToken ct = default);

    Task<Result<PagedResponse<PostSummaryResponse>>> SearchAsync(string query, int page, int pageSize,
        CancellationToken ct = default);

    Task<Result<PagedResponse<PostSummaryResponse>>> GetByAuthorAsync(
        Guid authorId, int page, int pageSize, CancellationToken ct = default);
    
    Task<Result<PagedResponse<PostSummaryResponse>>> GetDraftsByAuthorAsync(Guid authorId,
        CancellationToken ct = default);

    Task<Result<PostDetailResponse>> CreateAsync(Guid authorId, CreatePostRequest request,
        CancellationToken ct = default);

    Task<Result<PostDetailResponse>> UpdateAsync(Guid postId, Guid currentUserId, UpdatePostRequest request,
        CancellationToken ct = default);

    Task<Result> PublishAsync(Guid postId, Guid currentUserId, CancellationToken ct = default);
    Task<Result> UnpublishAsync(Guid postId, Guid currentUserId, CancellationToken ct = default);
    Task<Result> DeleteAsync(Guid postId, Guid currentUserId, CancellationToken ct = default);
    Task<Result> ReactAsync(Guid postId, Guid userId, ReactionType type, CancellationToken ct = default);
}