using Cyberius.Api.Common;
using Cyberius.Api.Common.BackgroundServices;
using Cyberius.Api.Common.Extensions;
using Cyberius.Api.Hubs;
using Cyberius.Application;
using Cyberius.Domain.Options;
using Cyberius.Infrastructure;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddJsonFile("./Environments/AppEnv.json", optional: true, reloadOnChange: true);
builder.Services.Configure<EmailSettings>(
    builder.Configuration.GetSection(EmailSettings.SectionName));
builder.Logging.ClearProviders();
builder.Logging.AddSimpleConsole(options =>
{
    options.UseUtcTimestamp = true;
    options.TimestampFormat = "dd.MM.yyyy HH:mm:ss.fff ";
});
builder.Services.Configure<Microsoft.AspNetCore.Http.Json.JsonOptions>(options =>
{
    options.SerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
});
builder.Services.AddOpenApiExtension();
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();

builder.Services.MapAllServices(builder.Configuration);
builder.Services.AddHostedService<OrphanedFilesCleanupService>();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplication();
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options.WithTheme(ScalarTheme.DeepSpace);
        options.WithTitle("Cyberius Api");
    });
}
app.UseCors("CorsPolicy");
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.UseBlogRateLimiting();
app.UseOutputCache();
app.MapHub<NotificationHub>("/hubs/notifications");
app.MapAllEndpoints();

app.Run();