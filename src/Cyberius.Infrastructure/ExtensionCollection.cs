using Cyberius.Domain.Interfaces;
using Cyberius.Domain.Options;
using Cyberius.Infrastructure.Data.Context;
using Cyberius.Infrastructure.Data.Repositories;
using Cyberius.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Minio;

namespace Cyberius.Infrastructure;

public static class ExtensionCollection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services, 
        IConfiguration configuration,
        Action<DatabaseOption>? configureOptions = null)
    {
        services.Configure<DatabaseOption>(configuration.GetSection(DatabaseOption.SectionName));
        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
        
        services.AddDbContext<AppDbContext>((serviceProvider, options) =>
        {
            var databaseOptions = serviceProvider.GetRequiredService<IOptionsMonitor<DatabaseOption>>().CurrentValue;
            options.UseNpgsql(databaseOptions.ConnectionString, sqlOptions =>
            {
                sqlOptions.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName);
            });
        });
        
        services
            .AddOptions<MinioOptions>()
            .BindConfiguration(MinioOptions.SectionName)
            .ValidateOnStart();
        services.AddSingleton<IMinioClient>(sp =>
        {
            var opts = sp.GetRequiredService<IOptions<MinioOptions>>().Value;

            return new MinioClient()
                .WithEndpoint(opts.Endpoint)
                .WithCredentials(opts.AccessKey, opts.SecretKey)
                .WithSSL(opts.UseSSL)
                .Build();
        });

        services.AddScoped<IStorageService, MinioStorageService>();
        
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        return services;
    }
}