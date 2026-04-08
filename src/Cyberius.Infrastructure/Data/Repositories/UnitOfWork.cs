using Cyberius.Domain.Interfaces;
using Cyberius.Infrastructure.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace Cyberius.Infrastructure.Data.Repositories;

public class UnitOfWork(AppDbContext db) : IUnitOfWork
{
    public IUserRepository Users => field is null ? field ??= new UserRepository(db) : field;
    public IRoleRepository Roles => field is null ? field ??= new RoleRepository(db) : field;
    public IUserRoleRepository UserRoles => field is null ? field ??= new UserRoleRepository(db) : field;
    public IRefreshTokenRepository RefreshTokens => field is null ? field ??= new RefreshTokenRepository(db) : field;
    public ICategoryRepository Categories => field is null ? field ??= new CategoryRepository(db) : field;
    public ICommentReactionRepository CommentReactions => field is null ? field ??= new CommentReactionRepository(db) : field;
    public ICommentRepository Comments => field is null ? field ??= new CommentRepository(db) : field;
    public IContentBlockRepository ContentBlocks => field is null ? field ??= new ContentBlockRepository(db) : field;
    public IPostReactionRepository PostReactions => field is null ? field ??= new PostReactionRepository(db) : field;
    public IPostRepository Posts => field is null ? field ??= new PostRepository(db) : field;
    public IPostViewRepository PostViews => field is null ? field ??= new PostViewRepository(db) : field;
    public ITagRepository Tags => field is null ? field ??= new TagRepository(db) : field;
    public IPostTagRepository PostTags => field is null ? field ??= new PostTagRepository(db) : field;
    public IEmailTokenRepository EmailTokens => field is null ? field ??= new EmailTokenRepository(db) : field;
    public INewsletterRepository Newsletters => field is null ? field ??= new NewsletterRepository(db) : field;

    public Task<int> SaveChangesAsync(CancellationToken ct = default)
        => db.SaveChangesAsync(ct);

    public async Task ExecuteInTransactionAsync(
        Func<Task> operation,
        CancellationToken ct = default)
    {
        var strategy = db.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            await using var tx = await db.Database.BeginTransactionAsync(ct);
            try
            {
                await operation();
                await db.SaveChangesAsync(ct);
                await tx.CommitAsync(ct);
            }
            catch
            {
                await tx.RollbackAsync(ct);
                throw;
            }
        });
    }

    public ValueTask DisposeAsync() => db.DisposeAsync();
}