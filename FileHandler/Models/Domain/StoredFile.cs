namespace FileHandler.Models.Domain
{
    public class StoredFile
    {
        public long Id { get; set; }
        public Guid DocumentId { get; set; } = Guid.NewGuid();
        public byte[] Content { get; set; }
        public string FileName { get; set; }
        public string Extension { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }
        public string CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }
    }
}
