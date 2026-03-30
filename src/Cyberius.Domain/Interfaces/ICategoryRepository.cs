using Cyberius.Domain.Entities;

namespace Cyberius.Domain.Interfaces;

public interface ICategoryRepository: IGenericRepository<Category>
{
    // По slug для URL (/posts?category=csharp)
    Task<Category?> GetBySlugAsync(
        string slug,
        CancellationToken ct = default);
 
    // Все категории с количеством опубликованных статей
    Task<IReadOnlyList<(Category Category, int PostCount)>> GetWithPostCountAsync(
        CancellationToken ct = default);
 
    // Проверка уникальности slug
    Task<bool> SlugExistsAsync(
        string slug,
        Guid? excludeId = null,
        CancellationToken ct = default);
}