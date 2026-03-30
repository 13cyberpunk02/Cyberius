using Cyberius.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cyberius.Infrastructure.Data.Configurations;

public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.HasKey(c => c.Id);
 
        builder.Property(c => c.Name)
            .HasMaxLength(100)
            .IsRequired();
 
        builder.Property(c => c.Slug)
            .HasMaxLength(120)
            .IsRequired();
 
        builder.Property(c => c.IconUrl)
            .HasMaxLength(1000);
 
        // HEX цвет: "#0ca2e7"
        builder.Property(c => c.Color)
            .HasMaxLength(7);
 
        builder.HasIndex(c => c.Slug)
            .IsUnique();
 
        builder.HasIndex(c => c.Name)
            .IsUnique();
    }
}