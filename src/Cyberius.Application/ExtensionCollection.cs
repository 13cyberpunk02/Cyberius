using System.Reflection;
using Cyberius.Application.Features.Authentication.Interfaces;
using Cyberius.Application.Features.Authentication.Services;
using Cyberius.Application.Features.Blog;
using Cyberius.Application.Features.JWT;
using Cyberius.Application.Features.Users.Interfaces;
using Cyberius.Application.Features.Users.Services;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace Cyberius.Application;

public static class ExtensionCollection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<IAuthenticationService, AuthenticationService>();
        services.AddScoped<IUserService, UserService>();
        services.AddBlogServices();
        
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
        return services;
    }
}