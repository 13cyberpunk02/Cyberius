using Cyberius.Application.Features.Blog.Tags.Models;

namespace Cyberius.Application.Features.Blog.Interfaces;

public interface ITagService
{
    Task<Result<List<TagResponse>>> GetPopularAsync(int count = 20, CancellationToken ct = default);
    Task<Result<TagResponse>> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Result<TagResponse>> CreateAsync(CreateTagRequest request, CancellationToken ct = default);
    Task<Result> DeleteAsync(Guid id, CancellationToken ct = default);
}