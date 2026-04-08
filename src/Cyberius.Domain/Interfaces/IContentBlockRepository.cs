using Cyberius.Domain.Entities;

namespace Cyberius.Domain.Interfaces;

public interface IContentBlockRepository: IGenericRepository<ContentBlock>
{
    // Все блоки статьи упорядоченные по Order
    Task<IReadOnlyList<ContentBlock>> GetByPostIdAsync(
        Guid postId,
        CancellationToken ct = default);
 
    // Максимальный Order для статьи (чтобы добавить блок в конец)
    Task<int> GetMaxOrderAsync(
        Guid postId,
        CancellationToken ct = default);
 
    // Удалить все блоки статьи (при полной перезаписи контента)
    Task RemoveAllByPostIdAsync(
        Guid postId,
        CancellationToken ct = default);
    
    Task<IReadOnlyList<string?>> GetAllImageObjectNamesAsync(CancellationToken ct = default);
}
