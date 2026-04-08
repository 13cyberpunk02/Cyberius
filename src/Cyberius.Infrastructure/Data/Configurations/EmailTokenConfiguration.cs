using Cyberius.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cyberius.Infrastructure.Data.Configurations;

public class EmailTokenConfiguration : IEntityTypeConfiguration<EmailToken>
{
    public void Configure(EntityTypeBuilder<EmailToken> builder)
    {
        builder.HasKey(t => t.Id);
 
        builder.Property(t => t.Token)
            .HasMaxLength(256)
            .IsRequired();
 
        builder.HasIndex(t => t.Token)
            .IsUnique();
 
        builder.HasIndex(t => t.UserId);
 
        builder.Property(t => t.ExpiresAt).IsRequired();
        builder.Property(t => t.IsUsed).HasDefaultValue(false);
        builder.Property(t => t.CreatedAt)
            .HasDefaultValueSql("NOW()");
 
        builder.HasOne(t => t.User)
            .WithMany()
            .HasForeignKey(t => t.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}