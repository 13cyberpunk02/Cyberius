using Cyberius.Domain.Entities;

namespace Cyberius.Domain.Interfaces;

public interface INewsletterRepository : IGenericRepository<NewsletterSubscriber>
{
    Task<NewsletterSubscriber?> GetByEmailAsync(string email, CancellationToken ct = default);
    Task<NewsletterSubscriber?> GetByTokenAsync(string token, CancellationToken ct = default);
    Task<IReadOnlyList<NewsletterSubscriber>> GetAllActiveAsync(CancellationToken ct = default);
}