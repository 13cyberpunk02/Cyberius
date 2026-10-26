namespace Cyberius.Domain.Options;

public sealed class AppOptions
{
    public const string SectionName = "AppSettings";

    public string FrontendUrl { get; init; } = string.Empty;

    /// Адрес сайта без слеша на конце: https://example.com
    public string BaseUrl => FrontendUrl.TrimEnd('/');
}