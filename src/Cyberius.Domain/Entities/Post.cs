using Cyberius.Domain.Entities.Enums;

namespace Cyberius.Domain.Entities;

public class Post
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Excerpt { get; set; }
    public string? CoverImageUrl { get; set; }
    public int ReadTimeMinutes { get; set; }
    public PostStatus Status { get; set; }
    public DateTime? PublishedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // FK
    public Guid AuthorId { get; set; }
    public User Author { get; set; } = null!;
    public Guid CategoryId { get; set; }
    public Category Category { get; set; } = null!;

    // Relations
    public ICollection<ContentBlock> Blocks { get; set; } = [];
    public ICollection<PostTag> PostTags { get; set; } = [];
    public ICollection<Comment> Comments { get; set; } = [];
    public ICollection<PostReaction> Reactions { get; set; } = [];
    public ICollection<PostView> Views { get; set; } = [];
}