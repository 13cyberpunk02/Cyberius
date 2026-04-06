namespace Cyberius.Application.Features.Blog.Posts.Models;

public record NeighboursDto(NeighbourItem? Previous, NeighbourItem? Next);

public record NeighbourItem(Guid Id, string Title, string Slug);