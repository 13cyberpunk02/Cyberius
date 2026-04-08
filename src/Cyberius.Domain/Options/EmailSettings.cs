namespace Cyberius.Domain.Options;

public class EmailSettings
{
    public const string SectionName = "EmailSettings";
    
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; } = 587;
    public bool UseSsl { get; set; } = true;
    public string From { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}