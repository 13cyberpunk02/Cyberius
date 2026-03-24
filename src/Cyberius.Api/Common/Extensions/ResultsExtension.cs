using Cyberius.Domain.Shared;
using Microsoft.AspNetCore.Mvc;

namespace Cyberius.Api.Common.Extensions;

public static class ResultsExtension
{
    public static IResult ToHttpResponse<T>(this Result<T> result)
        => result.IsSuccess
            ? Results.Ok(result.Value)
            : result.Error.ToHttpError();

    // Async варианты
    public static async Task<IResult> ToHttpResponseAsync<T>(
        this Task<Result<T>> resultTask)
        => (await resultTask).ToHttpResponse();

    private static IResult ToHttpError(this Error error)
        => error.Code switch
        {
            ErrorTypes.Validation or ErrorTypes.BadRequest => Results.BadRequest(error.ToProblem()),
            ErrorTypes.NotFound => Results.NotFound(error.ToProblem()),
            ErrorTypes.Conflict => Results.Conflict(error.ToProblem()),
            ErrorTypes.Forbidden => Results.Forbid(),
            ErrorTypes.Unauthorized => Results.Unauthorized(),
            ErrorTypes.InternalServerError => Results.InternalServerError(error.ToProblem()),
            _ => Results.Problem(
                title: error.Code,
                detail: error.Message,
                statusCode: StatusCodes.Status500InternalServerError)
        };

    private static ProblemDetails ToProblem(this Error error)
        => error switch
        {
            ValidationError e => new ValidationProblemDetails(
                new Dictionary<string, string[]>
                {
                    ["errors"] = [.. e.Details]
                })
            {
                Title  = e.Code,
                Detail = e.Message
            },
            _ => new ProblemDetails
            {
                Title  = error.Code,
                Detail = error.Message
            }
        };
}