using Cyberius.Domain.Entities;

namespace Cyberius.Domain.Interfaces;

public interface IPostViewRepository : IGenericRepository<PostView>
{
    // Количество просмотров статьи
    Task<int> GetCountByPostAsync(
        Guid postId,
        CancellationToken ct = default);
 
    // Количество просмотров для списка постов — один запрос вместо N
    Task<Dictionary<Guid, int>> GetCountsByPostsAsync(
        IEnumerable<Guid> postIds,
        CancellationToken ct = default);
 
    // Проверка — смотрел ли этот юзер/IP статью
    // (для дедупликации: не считать повторные просмотры)
    Task<bool> HasViewedAsync(
        Guid postId,
        Guid? userId,
        string? ipHash,
        TimeSpan window,              // окно дедупликации, напр. 24 часа
        CancellationToken ct = default);
 
    // Статистика просмотров по дням (для графика в панели автора)
    Task<IReadOnlyList<(DateOnly Date, int Count)>> GetDailyStatsAsync(
        Guid postId,
        DateOnly from,
        DateOnly to,
        CancellationToken ct = default);
}