using Cyberius.Domain.Entities.Enums;

namespace Cyberius.Domain.Entities;

public class PostReaction
{
    public Guid Id { get; set; }
    public Guid PostId { get; set; }
    public Post Post { get; set; } = null!;
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    public ReactionType Type { get; set; }
    public DateTime CreatedAt { get; set; }
}