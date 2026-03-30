using Cyberius.Domain.Entities;

namespace Cyberius.Domain.Interfaces;

public interface ITagRepository: IGenericRepository<Tag>
{
    // По slug
    Task<Tag?> GetBySlugAsync(
        string slug,
        CancellationToken ct = default);
 
    // Найти по списку имён (при создании/редактировании статьи)
    Task<IReadOnlyList<Tag>> GetByNamesAsync(
        IEnumerable<string> names,
        CancellationToken ct = default);
 
    // Популярные теги — по количеству использований
    Task<IReadOnlyList<(Tag Tag, int PostCount)>> GetPopularAsync(
        int count = 20,
        CancellationToken ct = default);
 
    // Проверка уникальности slug
    Task<bool> SlugExistsAsync(
        string slug,
        Guid? excludeId = null,
        CancellationToken ct = default);
}
