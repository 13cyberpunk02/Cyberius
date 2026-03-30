namespace Cyberius.Domain.Entities;

public class PostView
{
    public Guid Id { get; set; }
    public Guid PostId { get; set; }
    public Post Post { get; set; } = null!;
    public Guid? UserId { get; set; }
    public string? IpHash { get; set; }
    public string? UserAgent { get; set; }
    public DateTime ViewedAt { get; set; }
}