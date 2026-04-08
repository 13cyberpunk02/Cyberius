using Cyberius.Domain.Entities;

namespace Cyberius.Domain.Interfaces;

public interface IUnitOfWork : IAsyncDisposable
{
    IUserRepository Users { get; }
    IRoleRepository Roles { get; }
    IUserRoleRepository UserRoles { get; }
    IRefreshTokenRepository RefreshTokens { get; }
    ICategoryRepository Categories { get; }
    ICommentReactionRepository CommentReactions { get; }
    ICommentRepository Comments { get; }
    IContentBlockRepository ContentBlocks { get; }
    IPostReactionRepository PostReactions { get; }
    IPostRepository Posts { get; }
    IPostViewRepository PostViews { get; }
    ITagRepository Tags { get; }
    IPostTagRepository PostTags { get; }
    IEmailTokenRepository EmailTokens { get; }
    INewsletterRepository Newsletters { get; }

    Task<int> SaveChangesAsync(CancellationToken ct = default);

    Task ExecuteInTransactionAsync(
        Func<Task> operation,
        CancellationToken ct = default);
}