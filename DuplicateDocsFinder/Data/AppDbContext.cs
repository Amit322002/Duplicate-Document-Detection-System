using DuplicateDocsFinder.Entity;
using Microsoft.EntityFrameworkCore;

namespace DuplicateDocsFinder.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<Document> Documents { get; set; }
    }
}