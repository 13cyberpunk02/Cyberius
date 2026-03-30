using Cyberius.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cyberius.Infrastructure.Data.Configurations;

public class UserRoleConfiguration : IEntityTypeConfiguration<UserRole>
{
    private static readonly Guid _userId = Guid.Parse("08340629-1479-4F74-91CF-798DB48BE6DD");
    private static readonly Guid _roleId = Guid.Parse("83CF0C0F-D889-4332-8F7F-258CB9A737A4");

    private static UserRole _userRole = new()
    {   
        UserId = _userId,
        RoleId = _roleId
    };
    
    public void Configure(EntityTypeBuilder<UserRole> builder)
    {
        builder.ToTable("UserRoles");
        
        builder.HasKey(ur => new { ur.UserId, ur.RoleId });
        
        builder.HasOne(ur => ur.User)
            .WithMany(u => u.UserRoles)
            .HasForeignKey(ur => ur.UserId);
        
        builder.HasOne(ur => ur.Role)
            .WithMany(r => r.UserRoles)
            .HasForeignKey(ur => ur.RoleId);
        
        builder.HasData(_userRole);
    }
}