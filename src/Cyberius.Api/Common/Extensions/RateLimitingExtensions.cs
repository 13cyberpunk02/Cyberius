using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;

namespace Cyberius.Api.Common.Extensions;

public static class RateLimitingExtensions
{
    // Имена политик
    public const string Public     = "public";      // для анонимных GET
    public const string Mutations  = "mutations";   // POST/PUT/DELETE
    public const string Comments   = "comments";    // создание комментариев
    public const string Reactions  = "reactions";   // реакции
    public const string Views      = "views";       // трекинг просмотров
 
    public static IServiceCollection AddBlogRateLimiting(
        this IServiceCollection services) =>
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
 
            // Публичные запросы: 60 req/мин с окном 1 мин
            options.AddFixedWindowLimiter(Public, o =>
            {
                o.PermitLimit         = 60;
                o.Window              = TimeSpan.FromMinutes(1);
                o.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                o.QueueLimit          = 0;
            });
 
            // Мутации (создание/редактирование): 20 req/мин
            options.AddFixedWindowLimiter(Mutations, o =>
            {
                o.PermitLimit         = 20;
                o.Window              = TimeSpan.FromMinutes(1);
                o.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                o.QueueLimit          = 0;
            });
 
            // Комментарии: не больше 5 в минуту — защита от спама
            options.AddFixedWindowLimiter(Comments, o =>
            {
                o.PermitLimit         = 5;
                o.Window              = TimeSpan.FromMinutes(1);
                o.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                o.QueueLimit          = 0;
            });
 
            // Реакции: 30 в минуту — можно кликать быстро
            options.AddFixedWindowLimiter(Reactions, o =>
            {
                o.PermitLimit         = 30;
                o.Window              = TimeSpan.FromMinutes(1);
                o.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                o.QueueLimit          = 0;
            });
 
            // Трекинг просмотров: 20 в минуту
            options.AddFixedWindowLimiter(Views, o =>
            {
                o.PermitLimit         = 20;
                o.Window              = TimeSpan.FromMinutes(1);
                o.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                o.QueueLimit          = 0;
            });
        });
 
    public static IApplicationBuilder UseBlogRateLimiting(
        this IApplicationBuilder app) =>
        app.UseRateLimiter();
}