using Cyberius.Application.Features.Blog.Comments.Models;
using Cyberius.Application.Features.Blog.Posts.Models;
using Cyberius.Domain.Entities.Enums;

namespace Cyberius.Application.Features.Blog.Interfaces;

public interface ICommentService
{
    Task<Result<PagedResponse<CommentResponse>>> GetByPostAsync(Guid postId, int page, int pageSize,
        Guid? currentUserId, CancellationToken ct = default);

    Task<Result<CommentResponse>> CreateAsync(Guid authorId, CreateCommentRequest request,
        CancellationToken ct = default);

    Task<Result<CommentResponse>> UpdateAsync(Guid commentId, Guid currentUserId, UpdateCommentRequest request,
        CancellationToken ct = default);

    Task<Result> DeleteAsync(Guid commentId, Guid currentUserId, CancellationToken ct = default);
    Task<Result> ReactAsync(Guid commentId, Guid userId, ReactionType type, CancellationToken ct = default);
}