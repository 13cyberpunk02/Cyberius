using Cyberius.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cyberius.Infrastructure.Data.Configurations;


public class ContentBlockConfiguration : IEntityTypeConfiguration<ContentBlock>
{
    public void Configure(EntityTypeBuilder<ContentBlock> builder)
    {
        builder.HasKey(b => b.Id);
 
        builder.Property(b => b.Type)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();
 
        builder.Property(b => b.Order)
            .IsRequired();
 
        // Content — до 10 000 символов (большой код-блок)
        builder.Property(b => b.Content)
            .HasMaxLength(10_000);
 
        builder.Property(b => b.Language)
            .HasMaxLength(50); // "csharp", "typescript", "bash"
 
        builder.Property(b => b.ImageUrl)
            .HasMaxLength(1000);
 
        builder.Property(b => b.ImageCaption)
            .HasMaxLength(300);
 
        builder.Property(b => b.CalloutType)
            .HasMaxLength(20); // "info", "warning", "tip", "danger"
 
        builder.Property(b => b.Metadata)
            .HasMaxLength(2000)
            .HasColumnType("jsonb"); // Postgres — храним как JSONB для индексации
 
        // Быстрая выборка блоков статьи в нужном порядке
        builder.HasIndex(b => new { b.PostId, b.Order });
    }
}