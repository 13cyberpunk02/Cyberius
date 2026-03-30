using Cyberius.Application.Features.Blog.Interfaces;
using Cyberius.Application.Features.Blog.Tags.Models;
using Cyberius.Domain.Interfaces;
using Cyberius.Domain.Shared;

namespace Cyberius.Application.Features.Blog.Tags.Services;

public sealed class TagService(
    IUnitOfWork uow) : ITagService
{
    public async Task<Result<List<TagResponse>>> GetPopularAsync(
        int count = 20, CancellationToken ct = default)
    {
        var tags = await uow.Tags.GetPopularAsync(count, ct);
        return tags.Select(x => new TagResponse(x.Tag.Id, x.Tag.Name, x.Tag.Slug, x.PostCount)).ToList();
    }
 
    public async Task<Result<TagResponse>> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var tag = await uow.Tags.GetByIdAsync(id, ct);
        if (tag is null)
            return Errors.Tag.NotFound(id.ToString());
 
        return new TagResponse(tag.Id, tag.Name, tag.Slug, PostCount: 0);
    }
 
    public async Task<Result<TagResponse>> CreateAsync(
        CreateTagRequest request, CancellationToken ct = default)
    {
        if (await uow.Tags.SlugExistsAsync(request.Slug, ct: ct))
            return Errors.Tag.SlugAlreadyExists(request.Slug);
 
        var tag = new Domain.Entities.Tag
        {
            Id   = Guid.NewGuid(),
            Name = request.Name.Trim(),
            Slug = request.Slug.Trim().ToLower(),
        };
 
        await uow.Tags.AddAsync(tag, ct);
        await uow.SaveChangesAsync(ct);
 
        return new TagResponse(tag.Id, tag.Name, tag.Slug, PostCount: 0);
    }
 
    public async Task<Result> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var tag = await uow.Tags.GetByIdAsync(id, ct);
        if (tag is null)
            return Errors.Tag.NotFound(id.ToString());
 
        uow.Tags.Remove(tag);
        await uow.SaveChangesAsync(ct);
        return Result.Success();
    }
}