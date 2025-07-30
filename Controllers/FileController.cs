using FileHandler.Services;
using Microsoft.AspNetCore.Mvc;

namespace FileHandler.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FileController : ControllerBase
    {
        private readonly IFileService _fileService;

        public FileController(IFileService fileService)
        {
            _fileService = fileService;
        }

        [HttpPost("upload")]
        public async Task<IActionResult> Upload([FromForm] IFormFile file)
        {
            try
            {
                var documentId = await _fileService.UploadFileAsync(file);
                return Ok(new { documentId });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        [Route("{documentId:Guid}")]
        public async Task<IActionResult> GetFile(Guid documentId)
        {
            var cacheKey = $"file:{documentId}";
            
            // Check cache first
            var cachedFile = await _cacheService.GetAsync(cacheKey);
            if (cachedFile != null)
            {
                return Ok(cachedFile); // Cache hit
            }
            
            // Cache miss - get from database
            var result = await _fileService.GetFileAsync(documentId);
            if (result == null)
                return NotFound("File not found.");
                
            // Store in cache for 1 hour
            var resultJson = System.Text.Json.JsonSerializer.Serialize(result);
            await _cacheService.SetAsync(cacheKey, resultJson, TimeSpan.FromHours(1));

            return Ok(result);
        }

        [HttpPut]
        [Route("{documentId:Guid}")]
        public async Task<IActionResult> EditFile(Guid documentId, IFormFile newFile)
        {
            if (newFile == null || newFile.Length == 0)
                return BadRequest("No file uploaded.");

            try
            {
                // Call the UpdateFileAsync method to update the file and get the DocumentId
                var updatedDocumentId = await _fileService.UpdateFileAsync(documentId, newFile);

                // Return the DocumentId of the updated file
                return Ok(new { DocumentId = updatedDocumentId, Message = "File updated successfully." });
            }
            catch (ArgumentException ex)
            {
                // Handle specific exceptions like file validation errors
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                // Handle general exceptions
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

    }
}
