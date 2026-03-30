using Cyberius.Application.Features.Blog.Categories.Services;
using Cyberius.Application.Features.Blog.Comments.Services;
using Cyberius.Application.Features.Blog.Interfaces;
using Cyberius.Application.Features.Blog.Posts.Services;
using Cyberius.Application.Features.Blog.Tags.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Cyberius.Application.Features.Blog;

public static class BlogExtensions
{
    public static IServiceCollection AddBlogServices(this IServiceCollection services)
    {
        services.AddScoped<IPostService, PostService>();
        services.AddScoped<IPostViewService, PostViewService>();
        services.AddScoped<ICommentService, CommentService>();
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<ITagService, TagService>();
        
        return services;
    }
}