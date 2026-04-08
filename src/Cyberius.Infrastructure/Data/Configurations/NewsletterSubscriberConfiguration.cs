using Cyberius.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cyberius.Infrastructure.Data.Configurations;

public class NewsletterSubscriberConfiguration : IEntityTypeConfiguration<NewsletterSubscriber>
{
    public void Configure(EntityTypeBuilder<NewsletterSubscriber> builder)
    {
        builder.HasKey(s => s.Id);
 
        builder.Property(s => s.Email)
            .HasMaxLength(256)
            .IsRequired();
 
        builder.HasIndex(s => s.Email).IsUnique();
        builder.HasIndex(s => s.UnsubToken).IsUnique();
 
        builder.Property(s => s.UnsubToken).HasMaxLength(256).IsRequired();
        builder.Property(s => s.IsActive).HasDefaultValue(true);
        builder.Property(s => s.SubscribedAt).HasDefaultValueSql("NOW()");
    }
}