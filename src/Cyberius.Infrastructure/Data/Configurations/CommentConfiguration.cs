using Cyberius.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cyberius.Infrastructure.Data.Configurations;

public class CommentConfiguration : IEntityTypeConfiguration<Comment>
{
    public void Configure(EntityTypeBuilder<Comment> builder)
    {
        builder.HasKey(c => c.Id);
 
        builder.Property(c => c.Content)
            .HasMaxLength(2000)
            .IsRequired();
 
        builder.Property(c => c.IsEdited)
            .HasDefaultValue(false)
            .IsRequired();
 
        builder.Property(c => c.IsDeleted)
            .HasDefaultValue(false)
            .IsRequired();
 
        builder.Property(c => c.CreatedAt)
            .HasDefaultValueSql("NOW()")
            .IsRequired();
 
        builder.Property(c => c.UpdatedAt)
            .HasDefaultValueSql("NOW()")
            .IsRequired();
 
        // Быстрая выборка комментариев статьи
        builder.HasIndex(c => new { c.PostId, c.CreatedAt });
 
        // Быстрая выборка ответов на комментарий
        builder.HasIndex(c => c.ParentCommentId);
 
        // ── Отношения ──────────────────────────────────────────────
 
        builder.HasOne(c => c.Author)
            .WithMany()
            .HasForeignKey(c => c.AuthorId)
            .OnDelete(DeleteBehavior.Restrict); // автора нельзя удалить пока есть комментарии
 
        // Self-referencing: ответы на комментарий
        builder.HasOne(c => c.ParentComment)
            .WithMany(c => c.Replies)
            .HasForeignKey(c => c.ParentCommentId)
            .OnDelete(DeleteBehavior.Restrict) // не каскадируем — оставляем ответы при удалении родителя
            .IsRequired(false);
 
        builder.HasMany(c => c.Reactions)
            .WithOne(r => r.Comment)
            .HasForeignKey(r => r.CommentId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}