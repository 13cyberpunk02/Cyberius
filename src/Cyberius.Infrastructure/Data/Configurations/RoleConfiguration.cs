using Cyberius.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cyberius.Infrastructure.Data.Configurations;

public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    private readonly List<Role> _roles =
    [
        new Role
        {
            RoleId = Guid.Parse("83CF0C0F-D889-4332-8F7F-258CB9A737A4"),
            Name = "Admin",
            Description = "Admin role"
        },
        new Role
        {
            RoleId = Guid.Parse("56EEEE83-A801-43DC-983E-C7A28623B254"),
            Name = "Manager",
            Description = "Manager role"
        },
        new Role
        {
            RoleId = Guid.Parse("FBC57B00-43C3-498E-86B0-77B57778A296"),
            Name = "User",
            Description = "User role"
        }
    ];
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable("Roles");
        
        builder.HasKey(r => r.RoleId);
        builder.HasIndex(r => r.RoleId)
            .IsUnique();
        
        builder.HasIndex(r => r.Name)
            .IsUnique();
        builder.Property(r => r.Name)
            .IsRequired()
            .HasMaxLength(50);
        
        builder.Property(r => r.Description)
            .IsRequired()
            .HasMaxLength(200);
        builder.HasData(_roles);
    }
}