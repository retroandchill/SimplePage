using System.ComponentModel.DataAnnotations;

namespace Retro.SimplePage.Tests.SampleDb.Entities;

public class Blog {
  [Key]
  public int Id { get; set; }
  
  public ICollection<Post> Posts { get; init; } = new List<Post>();
}