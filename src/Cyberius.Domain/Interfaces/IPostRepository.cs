using Cyberius.Domain.Entities;

namespace Cyberius.Domain.Interfaces;

public interface IPostRepository : IGenericRepository<Post>
{
        // Получить по slug (для публичного URL /posts/csharp-13-features)
    Task<Post?> GetBySlugAsync(string slug, CancellationToken ct = default);
 
    // Получить с блоками контента (для страницы статьи)
    Task<Post?> GetWithBlocksAsync(Guid id, CancellationToken ct = default);
 
    // Получить с блоками + комментарии верхнего уровня + реакции
    Task<Post?> GetFullAsync(Guid id, CancellationToken ct = default);
 
    // Опубликованные статьи с пагинацией
    Task<(IReadOnlyList<Post> Items, int TotalCount)> GetPublishedPagedAsync(
        int page,
        int pageSize,
        CancellationToken ct = default);
 
    // Статьи по категории
    Task<(IReadOnlyList<Post> Items, int TotalCount)> GetByCategoryAsync(
        Guid categoryId,
        int page,
        int pageSize,
        CancellationToken ct = default);
 
    // Статьи по тегу
    Task<(IReadOnlyList<Post> Items, int TotalCount)> GetByTagAsync(
        string tagSlug,
        int page,
        int pageSize,
        CancellationToken ct = default);
 
    // Статьи автора
    Task<(IReadOnlyList<Post> Items, int TotalCount)> GetByAuthorAsync(
        Guid authorId,
        int page,
        int pageSize,
        CancellationToken ct = default);
 
    // Полнотекстовый поиск по заголовку и описанию
    Task<(IReadOnlyList<Post> Items, int TotalCount)> SearchAsync(
        string query,
        int page,
        int pageSize,
        CancellationToken ct = default);
 
    // Похожие статьи (по категории и тегам, исключая текущую)
    Task<IReadOnlyList<Post>> GetRelatedAsync(
        Guid postId,
        int count = 4,
        CancellationToken ct = default);
 
    // Проверка уникальности slug (при создании/редактировании)
    Task<bool> SlugExistsAsync(
        string slug,
        Guid? excludePostId = null,
        CancellationToken ct = default);
 
    // Черновики конкретного автора
    Task<IReadOnlyList<Post>> GetDraftsByAuthorAsync(
        Guid authorId,
        CancellationToken ct = default);
}