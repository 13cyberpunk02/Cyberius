using Cyberius.Domain.Entities;
using Cyberius.Domain.Entities.Enums;

namespace Cyberius.Domain.Interfaces;

public interface ICommentReactionRepository : IGenericRepository<CommentReaction>
{
    // Реакция конкретного юзера на комментарий
    Task<CommentReaction?> GetByCommentAndUserAsync(
        Guid commentId,
        Guid userId,
        CancellationToken ct = default);
 
    // Счётчики реакций на комментарий: { Like: 8, Heart: 2 }
    Task<Dictionary<ReactionType, int>> GetCountsByCommentAsync(
        Guid commentId,
        CancellationToken ct = default);
 
    // Счётчики реакций сразу для нескольких комментариев
    // (чтобы не делать N запросов для каждого комментария на странице)
    Task<Dictionary<Guid, Dictionary<ReactionType, int>>> GetCountsByCommentsAsync(
        IEnumerable<Guid> commentIds,
        CancellationToken ct = default);
 
    // Удалить реакцию юзера на комментарий
    Task RemoveByCommentAndUserAsync(
        Guid commentId,
        Guid userId,
        CancellationToken ct = default);
}