namespace Cyberius.Domain.Entities;

public class Role
{
    public Guid RoleId { get; set; } = Guid.CreateVersion7();
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    
    public virtual ICollection<UserRole> UserRoles { get; set; } = [];
}