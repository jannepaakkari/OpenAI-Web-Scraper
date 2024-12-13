using Microsoft.EntityFrameworkCore;
public class AppDbContext : DbContext
{
    public DbSet<HeaderPrediction>? HeaderPredictions { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite("Data Source=HeadersDatabase.db");
    }
}