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

    public static class Post
    {
        public static NotFoundError NotFound(string id)
            => new($"Статья с id '{id}' не найдена");

        public static NotFoundError NotFoundBySlug(string slug)
            => new($"Статья со slug '{slug}' не найдена");

        public static ConflictError SlugAlreadyExists(string slug)
            => new($"Статья со slug '{slug}' уже существует");

        public static BadRequestError AlreadyPublished()
            => new("Статья уже опубликована");

        public static ForbiddenError NotAuthor()
            => new("Только автор может редактировать статью");
    }

    public static class Comment
    {
        public static NotFoundError NotFound(string id)
            => new($"Комментарий с id '{id}' не найден");

        public static ForbiddenError NotAuthor()
            => new("Только автор может редактировать комментарий");

        public static BadRequestError CannotReplyToReply()
            => new("Нельзя ответить на ответ — только на комментарий верхнего уровня");

        public static BadRequestError PostMismatch()
            => new("Родительский комментарий принадлежит другой статье");
    }

    public static class Category
    {
        public static NotFoundError NotFound(string id)
            => new($"Категория с id '{id}' не найдена");

        public static ConflictError SlugAlreadyExists(string slug)
            => new($"Категория со slug '{slug}' уже существует");

        public static ConflictError NameAlreadyExists(string name)
            => new($"Категория с названием '{name}' уже существует");

        public static ConflictError HasPosts()
            => new("Нельзя удалить категорию, в которой есть статьи");
    }

    public static class Tag
    {
        public static NotFoundError NotFound(string id)
            => new($"Тег с id '{id}' не найден");

        public static ConflictError SlugAlreadyExists(string slug)
            => new($"Тег со slug '{slug}' уже существует");
    }

    public static class Reaction
    {
        public static BadRequestError InvalidType(string type)
            => new($"Тип реакции '{type}' не поддерживается");
    }
}