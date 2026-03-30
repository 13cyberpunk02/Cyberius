using Cyberius.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cyberius.Infrastructure.Data.Configurations;

public class TagConfiguration : IEntityTypeConfiguration<Tag>
{
    public void Configure(EntityTypeBuilder<Tag> builder)
    {
        builder.HasKey(t => t.Id);
 
        builder.Property(t => t.Name)
            .HasMaxLength(100)
            .IsRequired();
 
        builder.Property(t => t.Slug)
            .HasMaxLength(120)
            .IsRequired();
 
        builder.HasIndex(t => t.Slug)
            .IsUnique();
 
        builder.HasIndex(t => t.Name)
            .IsUnique();
    }
}