using FileHandler.Models.Domain;
using Microsoft.EntityFrameworkCore;

namespace FileHandler.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions options) : base(options)
        {
        }
        public DbSet<StoredFile> StoredFiles { get; set; }
    }
    
}
