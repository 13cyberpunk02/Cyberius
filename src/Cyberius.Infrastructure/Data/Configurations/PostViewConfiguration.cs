using Cyberius.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cyberius.Infrastructure.Data.Configurations;

public class PostViewConfiguration : IEntityTypeConfiguration<PostView>
{
    public void Configure(EntityTypeBuilder<PostView> builder)
    {
        builder.HasKey(v => v.Id);
 
        builder.Property(v => v.IpHash)
            .HasMaxLength(64) // SHA-256 hex = 64 символа
            .IsRequired(false);
 
        builder.Property(v => v.UserAgent)
            .HasMaxLength(500)
            .IsRequired(false);
 
        builder.Property(v => v.ViewedAt)
            .HasDefaultValueSql("NOW()")
            .IsRequired();
 
        // Дедупликация и аналитика по времени
        builder.HasIndex(v => new { v.PostId, v.ViewedAt });
 
        // Дедупликация для авторизованных
        builder.HasIndex(v => new { v.PostId, v.UserId, v.ViewedAt })
            .HasFilter("\"UserId\" IS NOT NULL"); // частичный индекс — только для не-null
 
        // Дедупликация для анонимных по IP
        builder.HasIndex(v => new { v.PostId, v.IpHash, v.ViewedAt })
            .HasFilter("\"IpHash\" IS NOT NULL");
 
        builder.HasOne(v => v.Post)
            .WithMany(p => p.Views)
            .HasForeignKey(v => v.PostId)
            .OnDelete(DeleteBehavior.Cascade);
 
        builder.HasOne(v => v.User)
            .WithMany()
            .HasForeignKey(v => v.UserId)
            .OnDelete(DeleteBehavior.SetNull) // удаляем юзера — обнуляем UserId, просмотры остаются
            .IsRequired(false);
    }
}
