namespace Cyberius.Domain.Shared;

public abstract record Error(string Code, string Message);

public sealed record NotFoundError(string Message)
    : Error(ErrorTypes.NotFound, Message);

public sealed record ValidationError(string Message, IReadOnlyList<string> Details)
    : Error(ErrorTypes.Validation, Message);

public sealed record ConflictError(string Message)
    : Error(ErrorTypes.Conflict, Message);

public sealed record BadRequestError(string Message)
    : Error(ErrorTypes.BadRequest, Message);

public sealed record ForbiddenError(string Message)
    : Error(ErrorTypes.Forbidden, Message);

public sealed record UnauthorizedError(string Message)
    : Error(ErrorTypes.Unauthorized, Message);

public sealed record InternalError(string Message, Exception? Exception = null)
    : Error(ErrorTypes.InternalServerError, Message);