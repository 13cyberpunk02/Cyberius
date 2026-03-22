namespace Cyberius.Domain.Entities;

public class RefreshToken
{
    public Guid RefreshTokenId { get; set; } = Guid.CreateVersion7();
    public virtual User? User { get; set; }
    public string Token { get; set; } = string.Empty;
}