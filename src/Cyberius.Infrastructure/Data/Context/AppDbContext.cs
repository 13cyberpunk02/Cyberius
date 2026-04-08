using System.Reflection;
using Cyberius.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Cyberius.Infrastructure.Data.Context;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<UserRole> UserRoles { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    public DbSet<Post> Posts { get; set; }
    public DbSet<PostReaction> PostReactions { get; set; }
    public DbSet<PostTag> PostTags { get; set; }
    public DbSet<PostView> PostViews { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Comment> Comments { get; set; }
    public DbSet<CommentReaction> CommentReactions { get; set; }
    public DbSet<ContentBlock> ContentBlocks { get; set; }
    public DbSet<Tag> Tags { get; set; }
    public DbSet<EmailToken> EmailTokens { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}   