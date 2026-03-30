using Cyberius.Domain.Entities.Enums;

namespace Cyberius.Domain.Entities;

public class CommentReaction
{
    public Guid Id { get; set; }
    public Guid CommentId { get; set; }
    public Comment Comment { get; set; } = null!;
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    public ReactionType Type { get; set; }
    public DateTime CreatedAt { get; set; }
}