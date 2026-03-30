using Cyberius.Domain.Entities.Enums;

namespace Cyberius.Domain.Entities;

public class ContentBlock
{
    public Guid Id { get; set; }
    public Guid PostId { get; set; }
    public Post Post { get; set; } = null!;

    public BlockType Type { get; set; }
    public int Order { get; set; }
    
    public string? Content { get; set; }     // текст / код / цитата / заголовок
    public string? Language { get; set; }    // для Code: "csharp", "typescript"
    public string? ImageUrl { get; set; }    // для Image: путь в Minio
    public string? ImageCaption { get; set; }// подпись под картинкой
    public string? CalloutType { get; set; } // "info" | "warning" | "tip"
    public string? Metadata { get; set; }    // JSON для расширений
}