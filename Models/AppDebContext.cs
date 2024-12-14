using Microsoft.EntityFrameworkCore;

public class AppDbContext : DbContext
{
    public DbSet<HeaderPrediction> HeaderPredictions { get; set; } = null!;

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
    }
}
