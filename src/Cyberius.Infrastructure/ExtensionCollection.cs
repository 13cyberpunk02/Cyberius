using Cyberius.Domain.Interfaces;
using Cyberius.Domain.Options;
using Cyberius.Infrastructure.Data.Context;
using Cyberius.Infrastructure.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

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
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        return services;
    }
}