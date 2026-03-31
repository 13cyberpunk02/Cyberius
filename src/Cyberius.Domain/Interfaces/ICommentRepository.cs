using Cyberius.Domain.Entities;

namespace Cyberius.Domain.Interfaces;

public interface ICommentRepository: IGenericRepository<Comment>
{
    // Корневые комментарии статьи (без родителя) + их ответы, с пагинацией
    Task<(IReadOnlyList<Comment> Items, int TotalCount)> GetByPostIdAsync(
        Guid postId,
        int page,
        int pageSize,
        CancellationToken ct = default);
 
    // Ответы на конкретный комментарий
    Task<IReadOnlyList<Comment>> GetRepliesAsync(
        Guid parentCommentId,
        CancellationToken ct = default);
 
    // Комментарии пользователя (для страницы профиля)
    Task<(IReadOnlyList<Comment> Items, int TotalCount)> GetByAuthorAsync(
        Guid authorId,
        int page,
        int pageSize,
        CancellationToken ct = default);
 
    // Получить с автором и реакциями — для ответа после создания/обновления
    Task<Comment?> GetWithAuthorAsync(Guid id, CancellationToken ct = default);
 
    // Количество комментариев статьи (для карточки)
    Task<int> GetCountByPostAsync(
        Guid postId,
        CancellationToken ct = default);
}