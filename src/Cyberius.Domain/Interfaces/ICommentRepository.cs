using Cyberius.Domain.Entities;

namespace Cyberius.Domain.Interfaces;

public interface ICommentRepository: IGenericRepository<Comment>
{
    Task<(IReadOnlyList<Comment> Items, int TotalCount)> GetByPostIdAsync(
        Guid postId, int page, int pageSize, CancellationToken ct = default);
 
    Task<IReadOnlyList<Comment>> GetRepliesAsync(
        Guid parentCommentId, CancellationToken ct = default);
 
    Task<(IReadOnlyList<Comment> Items, int TotalCount)> GetByAuthorAsync(
        Guid authorId, int page, int pageSize, CancellationToken ct = default);
 
    Task<Comment?> GetWithAuthorAsync(Guid id, CancellationToken ct = default);
 
    Task<int> GetCountByPostAsync(Guid postId, CancellationToken ct = default);
 
    // Bulk — один запрос для всей страницы постов
    Task<Dictionary<Guid, int>> GetCountsByPostsAsync(
        IEnumerable<Guid> postIds, CancellationToken ct = default);
}