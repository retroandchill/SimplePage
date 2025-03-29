namespace Retro.SimplePage.Tests.SampleDb.Entities;

public class Post {
  public int Id { get; set; }
  
  public int BlogId { get; set; }
  public Blog Blog { get; set; } = null!;
}