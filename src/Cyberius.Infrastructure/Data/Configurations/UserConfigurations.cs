using Cyberius.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cyberius.Infrastructure.Data.Configurations;

public class UserConfigurations : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");
        
        builder.HasKey(x => x.UserId);
        builder.Property(u => u.UserId)
            .ValueGeneratedOnAdd()
            .IsRequired();
        builder.HasIndex(x => x.UserId).IsUnique();
        builder.HasIndex(x => x.Email).IsUnique();
        builder.HasIndex(x => x.UserName).IsUnique();
        
        builder.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(100);
        
        builder.Property(u => u.UserName)
            .IsRequired()
            .HasMaxLength(100);
        
        builder.Property(u => u.FirstName)
            .IsRequired()
            .HasMaxLength(100);
        
        builder.Property(u => u.LastName)
            .IsRequired()
            .HasMaxLength(100);
        
        builder.Property(u => u.DateOfBirth)
            .IsRequired()
            .HasColumnType("date");
        
        builder.Property(u => u.JoinedDate)
            .IsRequired();
        
        builder.HasOne(u => u.RefreshToken)
            .WithOne(rt => rt.User)
            .HasForeignKey<RefreshToken>("UserId")
            .OnDelete(DeleteBehavior.Cascade);
    }
}