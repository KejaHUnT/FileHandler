namespace FileHandler.Services
{
    public interface IFileService
    {
        Task<Guid> UploadFileAsync(IFormFile file);
        Task<object?> GetFileAsync(Guid documentId);
        Task<Guid> UpdateFileAsync(Guid documentId, IFormFile newFile);
    }
}
