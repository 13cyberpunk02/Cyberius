namespace Cyberius.Domain.Shared;

public static class Errors
{
    public static NotFoundError NotFound(string entity, string id)
        => new($"Сущность {entity} c id '{id}' не найдено");

    public static NotFoundError NotFound(string entity, Guid id)
        => new($"Сущность {entity} c id '{id}' не найдено");

    public static ValidationError Validation(string message, params string[] details)
        => new(message, details);

    public static ConflictError Conflict(string message)
        => new(message);

    public static BadRequestError BadRequest(string message)
        => new(message);

    public static ForbiddenError Forbidden(string message = "Access forbidden")
        => new(message);

    public static UnauthorizedError Unauthorized(string message = "Access denied")
        => new(message);

    public static InternalError Internal(string message, Exception? ex = null)
        => new(message, ex);
}