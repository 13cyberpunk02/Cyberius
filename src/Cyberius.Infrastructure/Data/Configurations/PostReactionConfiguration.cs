using Cyberius.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cyberius.Infrastructure.Data.Configurations;

public class PostReactionConfiguration : IEntityTypeConfiguration<PostReaction>
{
    public void Configure(EntityTypeBuilder<PostReaction> builder)
    {
        builder.HasKey(r => r.Id);
 
        builder.Property(r => r.Type)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();
 
        builder.Property(r => r.CreatedAt)
            .HasDefaultValueSql("NOW()")
            .IsRequired();
 
        // Один пользователь — одна реакция одного типа на статью
        // Позволяет поставить Like И Fire одновременно, но не два Like
        builder.HasIndex(r => new { r.PostId, r.UserId, r.Type })
            .IsUnique();
 
        // Быстрая выборка реакций статьи
        builder.HasIndex(r => r.PostId);
 
        builder.HasOne(r => r.Post)
            .WithMany(p => p.Reactions)
            .HasForeignKey(r => r.PostId)
            .OnDelete(DeleteBehavior.Cascade);
 
        builder.HasOne(r => r.User)
            .WithMany()
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Cascade); // удаляем юзера — удаляем его реакции
    }
}