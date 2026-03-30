using Cyberius.Domain.Entities;
using Cyberius.Domain.Entities.Enums;

namespace Cyberius.Domain.Interfaces;

public interface IPostReactionRepository : IGenericRepository<PostReaction>
{
    // Реакция конкретного юзера на статью (null = не реагировал)
    Task<PostReaction?> GetByPostAndUserAsync(
        Guid postId,
        Guid userId,
        CancellationToken ct = default);
 
    // Счётчики всех реакций на статью: { Like: 12, Fire: 5, Heart: 3 }
    Task<Dictionary<ReactionType, int>> GetCountsByPostAsync(
        Guid postId,
        CancellationToken ct = default);
 
    // Удалить реакцию юзера на статью (при повторном нажатии)
    Task RemoveByPostAndUserAsync(
        Guid postId,
        Guid userId,
        CancellationToken ct = default);
}