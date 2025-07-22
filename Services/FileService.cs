
using FileHandler.Data;
using FileHandler.Models.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace FileHandler.Services
{
    public class FileService : IFileService
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private static readonly List<string> AllowedExtensions = new() { ".jpg", ".jpeg", ".png" }; //p
        private const long MaxFileSize = 5 * 1024 * 1024; // know the value....put to appp setting....

        public FileService(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }
        public async Task<object?> GetFileAsync(Guid documentId)
        {
            var file = await _context.StoredFiles.FirstOrDefaultAsync(f => f.DocumentId == documentId);

            if (file == null)
                return null;

            var base64 = Convert.ToBase64String(file.Content);

            return new
            {
                file.FileName,
                file.Extension,
                file.CreatedAt,
                file.CreatedBy,
                base64
            };


        }

        public async Task<Guid> UploadFileAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("File is required.");

            if (file.Length > MaxFileSize)
                throw new ArgumentException("File size exceeds 5MB limit.");

            // Read the allowed extensions from configuration
            var allowedExtensions = _configuration.GetSection("AllowedFileExtensions").Get<List<string>>();

            var extension = Path.GetExtension(file.FileName).ToLower();
            if (!allowedExtensions.Contains(extension))
                throw new ArgumentException($"Only {string.Join(", ", allowedExtensions)} files are allowed.");


            using var ms = new MemoryStream();
            await file.CopyToAsync(ms);

            var storedFile = new StoredFile
            {
                Content = ms.ToArray(),
                FileName = Path.GetFileNameWithoutExtension(file.FileName),
                Extension = extension,
                CreatedBy = ""
            };
            _context.StoredFiles.Add(storedFile);
            await _context.SaveChangesAsync();

            return storedFile.DocumentId;

        }
        public async Task<Guid> UpdateFileAsync(Guid documentId, IFormFile newFile)
        {
            // 1. Validate the new file (same as in the upload method)
            if (newFile == null || newFile.Length == 0)
                throw new ArgumentException("File is required.");

            if (newFile.Length > MaxFileSize)
                throw new ArgumentException("File size exceeds 5MB limit.");

            var extension = Path.GetExtension(newFile.FileName).ToLower();
            if (!AllowedExtensions.Contains(extension))
                throw new ArgumentException("Only .jpg, .jpeg, and .png files are allowed.");

            // 2. Find the existing file in the database by DocumentId
            var existingFile = await _context.StoredFiles.FirstOrDefaultAsync(f => f.DocumentId == documentId);

            if (existingFile == null)
                throw new ArgumentException("File not found.");

            // 3. Read the new file's content and prepare to update
            using var ms = new MemoryStream();
            await newFile.CopyToAsync(ms);

            // 4. Update the existing file's properties
            existingFile.Content = ms.ToArray(); // Update file content
            existingFile.FileName = Path.GetFileNameWithoutExtension(newFile.FileName); // Update the file name if needed
            existingFile.Extension = extension; // Update the extension (optional)
            existingFile.CreatedAt = DateTime.UtcNow; // Update the timestamp if required

            // 5. Save the changes to the database
            await _context.SaveChangesAsync();

            // 6. Return the DocumentId of the updated file
            return existingFile.DocumentId;
        }


    }
}
