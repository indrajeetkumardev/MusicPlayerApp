using Microsoft.EntityFrameworkCore;
using MusicBaseApp.Models;

namespace MusicBaseApp.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Song> Songs { get; set; }
    }
}
