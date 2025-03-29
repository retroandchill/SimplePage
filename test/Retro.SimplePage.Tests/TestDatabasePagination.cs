using Microsoft.EntityFrameworkCore;
using Retro.SimplePage.EntityFrameworkCore;
using Retro.SimplePage.Tests.SampleDb;
using Retro.SimplePage.Tests.SampleDb.Entities;

namespace Retro.SimplePage.Tests;

public class TestDatabasePagination {
  
  private TestDbContext _context;
  
  [SetUp]
  public async Task Setup() {
    _context = new TestDbContext();
    await _context.Database.EnsureCreatedAsync();

    _context.Blogs.AddRange(Enumerable.Range(0, 100)
        .Select(x => new Blog {
            Posts = Enumerable.Range(0, 10).Select(y => new Post()).ToList()
        }));
    await _context.SaveChangesAsync();
  }
  
  [TearDown]
  public void TearDown() {
    _context.Dispose();
  }

  [Test]
  public void TestSynchronousPagination() {
    var posts = _context.Posts
        .OrderBy(x => x.Id)
        .ToPage(1, 10);
    
    Assert.That(posts, Has.Count.EqualTo(10));
  }
  
  [Test]
  public void TestSynchronousPaginationWithComplexJoin() {
    var posts = _context.Blogs
        .Include(x => x.Posts)
        .ToPage(new Pageable(3, 10));
    
    Assert.That(posts, Has.Count.EqualTo(10));
  }
  
  [Test]
  public void TestSynchronousPaginationUnpaged() {
    var posts = _context.Posts
        .OrderBy(x => x.Id)
        .ToPage(Pageable.Unpaged);
    
    Assert.That(posts, Has.Count.EqualTo(1000));
  }
  
  [Test]
  public async Task TestAsynchronousPagination() {
    var posts = await _context.Posts
        .OrderBy(x => x.Id)
        .ToPageAsync(1, 10);
    
    Assert.That(posts, Has.Count.EqualTo(10));
  }
  
  [Test]
  public async Task TestAsynchronousPaginationWithComplexJoin() {
    var posts = await _context.Blogs
        .Include(x => x.Posts).ToPageAsync(new Pageable(3, 10));
    
    Assert.That(posts, Has.Count.EqualTo(10));
  }
  
  [Test]
  public async Task TestAsynchronousPaginationUnpaged() {
    var posts = await _context.Posts
        .OrderBy(x => x.Id)
        .ToPageAsync(Pageable.Unpaged);
    
    Assert.That(posts, Has.Count.EqualTo(1000));
  }
  
}