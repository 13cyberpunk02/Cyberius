namespace Cyberius.Domain.Entities;

public class NewsletterSubscriber
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public string Email { get; set; } = string.Empty;
    public string UnsubToken { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime SubscribedAt { get; set; } = DateTime.UtcNow;
}