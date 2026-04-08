namespace Cyberius.Domain.Interfaces;

public interface IStorageService
{
    /// <summary>Загружает файл и возвращает имя объекта в хранилище</summary>
    Task<Result<string>> UploadAsync(
        Stream   stream,
        string   fileName,
        string   contentType,
        CancellationToken ct = default);

    /// <summary>Возвращает URL для доступа к файлу</summary>
    Task<Result<Stream>> GetFileStreamAsync(
        string objectName,
        CancellationToken ct = default);

    /// <summary>Удаляет файл из хранилища</summary>
    Task<Result> DeleteAsync(
        string objectName,
        CancellationToken ct = default);
    
    string GetPublicUrl(string objectName);
    
    /// <summary>Список всех objectName в хранилище</summary>
    Task<IReadOnlyList<string>> ListAllObjectNamesAsync(CancellationToken ct = default);
 
    /// <summary>Возраст файла (время с момента создания)</summary>
    Task<TimeSpan> GetObjectAgeAsync(string objectName, CancellationToken ct = default);
}