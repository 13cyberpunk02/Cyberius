using FluentValidation;

namespace Cyberius.Api.Common.Filters;

public class RequestValidationFilter<TRequest>(ILogger<RequestValidationFilter<TRequest>> logger, IValidator<TRequest>? validator = null) : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var requestName = typeof(TRequest).FullName;

        if (validator is null)
        {
            logger.LogInformation("{Request}: Валидатор не настроен для данной модели данных.", requestName);
            return await next(context);
        }
        
        logger.LogInformation("{Request}: Валидация...", requestName);

        var request = context.Arguments.OfType<TRequest>().First();
        var validationResult = await validator.ValidateAsync(request, context.HttpContext.RequestAborted);
        if (!validationResult.IsValid)
        {
            logger.LogWarning("{Request}: Ошибка валидации.", requestName);
            return Results.ValidationProblem(validationResult.ToDictionary());
        }
        
        logger.LogInformation("{Request}: Успешная валидация.", requestName);
        return await next(context);
    }
}