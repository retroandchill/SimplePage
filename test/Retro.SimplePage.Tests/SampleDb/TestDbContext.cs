using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Retro.SimplePage.Tests.SampleDb.Entities;

namespace Retro.SimplePage.Tests.SampleDb;

public class TestDbContext : DbContext {

  private SqliteConnection? _dbConnection;
  
  public DbSet<Blog> Blogs { get; set; }
  public DbSet<Post> Posts { get; set; }
  
  protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
    _dbConnection = new SqliteConnection("Filename=:memory:");
    _dbConnection.Open();
    optionsBuilder.UseSqlite(_dbConnection, b =>
            b.MinBatchSize(1)
                .MaxBatchSize(100))
        .UseSnakeCaseNamingConvention();
  }
  
  protected override void OnModelCreating(ModelBuilder modelBuilder) {
    modelBuilder.Entity<Blog>().HasMany(b => b.Posts)
        .WithOne(p => p.Blog)
        .HasForeignKey(p => p.BlogId)
        .OnDelete(DeleteBehavior.Cascade);
  }
  
}