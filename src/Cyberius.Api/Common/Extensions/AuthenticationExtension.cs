using System.Text;
using Cyberius.Api.Notifications;
using Cyberius.Application.Features.Notifications.Interfaces;
using Cyberius.Domain.Options;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace Cyberius.Api.Common.Extensions;

public static class AuthenticationExtension
{
    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var jwtOption = new JwtOptions();
        configuration.GetSection(JwtOptions.SectionName).Bind(jwtOption);

        if (string.IsNullOrEmpty(jwtOption.SecretKey))
            throw new InvalidOperationException("JWT приватный ключ не задан.");
        if (jwtOption.SecretKey.Length < 32)
            throw new InvalidOperationException("JWT приватный ключ должен содержать хотя бы 32 символа");
        services.AddSignalR();
        services.AddScoped<INotificationService, SignalRNotificationService>();
        services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.SaveToken = true;
                options.RequireHttpsMetadata = false;

                options.TokenValidationParameters = new()
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ClockSkew = TimeSpan.Zero,

                    ValidIssuer = jwtOption.Issuer,
                    ValidAudience = jwtOption.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOption.SecretKey))
                };

                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var token = context.Request.Query["access_token"];
                        if (!string.IsNullOrEmpty(token) &&
                            context.HttpContext.Request.Path.StartsWithSegments("/hubs"))
                        {
                            context.Token = token;
                        }
                        return Task.CompletedTask;
                    }
                };
            });
        services.AddAuthorization();
        services.AddBlogRateLimiting();
        services.AddBlogOutputCache();
        return services;
    }
}