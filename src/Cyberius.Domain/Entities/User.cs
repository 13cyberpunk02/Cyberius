namespace Cyberius.Domain.Entities;

public class User
{
    public Guid UserId { get; set; } = Guid.CreateVersion7();
    public string Email { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string? PasswordHash { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? AvatarObjectName { get; set; }
    public DateOnly DateOfBirth { get; set; }
    public DateTime JoinedDate { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;
    public bool IsDeleted { get; set; } = false;

    public virtual ICollection<UserRole> UserRoles { get; set; } = [];
    public virtual RefreshToken? RefreshToken { get; set; }
}