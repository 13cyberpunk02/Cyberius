using Cyberius.Domain.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Cyberius.Infrastructure.Data.Context;

public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("Environments/AppEnv.json", optional: false, reloadOnChange: true)
            .Build();

        var databaseOptions = new DatabaseOption();
        configuration.GetSection(DatabaseOption.SectionName).Bind(databaseOptions);
        
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder.UseNpgsql(
            databaseOptions.ConnectionString,
            sqlOptions =>
            {
                sqlOptions.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName);
            });
        return new AppDbContext(optionsBuilder.Options);
    }
}