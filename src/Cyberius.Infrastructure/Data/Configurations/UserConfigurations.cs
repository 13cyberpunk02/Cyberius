using Cyberius.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cyberius.Infrastructure.Data.Configurations;

public class UserConfigurations : IEntityTypeConfiguration<User>
{
    private static readonly Guid _userId = Guid.Parse("08340629-1479-4F74-91CF-798DB48BE6DD");
    private static readonly Guid _roleId = Guid.Parse("83CF0C0F-D889-4332-8F7F-258CB9A737A4");
    
    private static readonly User _userSeed = new User
    {
        UserId = Guid.Parse("08340629-1479-4F74-91CF-798DB48BE6DD"),
        DateOfBirth = DateOnly.Parse("1992-02-13"),
        IsActive = true,
        Email = "admin@domain.ru",
        FirstName = "Admin",
        LastName = "Admin",
        UserName = "admin",
        JoinedDate = DateTime.Parse("2026-03-30").ToUniversalTime(),
        PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!"),
    };
    


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
        
        builder.HasData(_userSeed);
    }
}