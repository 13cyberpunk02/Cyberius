using Cyberius.Domain.Entities;
using Cyberius.Domain.Entities.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cyberius.Infrastructure.Data.Configurations;

public class PostConfiguration : IEntityTypeConfiguration<Post>
{
    public void Configure(EntityTypeBuilder<Post> builder)
    {
        builder.HasKey(p => p.Id);
 
        builder.Property(p => p.Title)
            .HasMaxLength(300)
            .IsRequired();
 
        builder.Property(p => p.Slug)
            .HasMaxLength(320)
            .IsRequired();
 
        builder.Property(p => p.Excerpt)
            .HasMaxLength(500);
 
        builder.Property(p => p.CoverImageUrl)
            .HasMaxLength(1000);
 
        builder.Property(p => p.ReadTimeMinutes)
            .HasDefaultValue(1);
 
        builder.Property(p => p.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .HasDefaultValue(PostStatus.Draft)
            .IsRequired();
 
        builder.Property(p => p.CreatedAt)
            .HasDefaultValueSql("NOW()")
            .IsRequired();
 
        builder.Property(p => p.UpdatedAt)
            .HasDefaultValueSql("NOW()")
            .IsRequired();
 
        // Уникальный slug — основа красивых URL
        builder.HasIndex(p => p.Slug)
            .IsUnique();
 
        // Быстрая выборка опубликованных + сортировка по дате
        builder.HasIndex(p => new { p.Status, p.PublishedAt });
 
        // Поиск статей автора
        builder.HasIndex(p => p.AuthorId);
 
        // Поиск статей категории
        builder.HasIndex(p => p.CategoryId);
 
        // ── Отношения ──────────────────────────────────────────────
 
        builder.HasOne(p => p.Author)
            .WithMany()
            .HasForeignKey(p => p.AuthorId)
            .OnDelete(DeleteBehavior.Restrict); // автора нельзя удалить пока есть статьи
 
        builder.HasOne(p => p.Category)
            .WithMany(c => c.Posts)
            .HasForeignKey(p => p.CategoryId)
            .OnDelete(DeleteBehavior.Restrict); // категорию нельзя удалить пока есть статьи
 
        builder.HasMany(p => p.Blocks)
            .WithOne(b => b.Post)
            .HasForeignKey(b => b.PostId)
            .OnDelete(DeleteBehavior.Cascade); // удаляем статью — удаляем все блоки
 
        builder.HasMany(p => p.PostTags)
            .WithOne(pt => pt.Post)
            .HasForeignKey(pt => pt.PostId)
            .OnDelete(DeleteBehavior.Cascade);
 
        builder.HasMany(p => p.Comments)
            .WithOne(c => c.Post)
            .HasForeignKey(c => c.PostId)
            .OnDelete(DeleteBehavior.Cascade);
 
        builder.HasMany(p => p.Reactions)
            .WithOne(r => r.Post)
            .HasForeignKey(r => r.PostId)
            .OnDelete(DeleteBehavior.Cascade);
 
        builder.HasMany(p => p.Views)
            .WithOne(v => v.Post)
            .HasForeignKey(v => v.PostId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}