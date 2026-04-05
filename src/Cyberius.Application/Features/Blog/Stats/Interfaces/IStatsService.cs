using Cyberius.Application.Features.Blog.Stats.DTOs;

namespace Cyberius.Application.Features.Blog.Stats.Interfaces;

public interface IStatsService
{
    Task<Result<AuthorStatsResponse>> GetAuthorStatsAsync(Guid authorId, CancellationToken cancel);
}