namespace Retro.SimplePage.Tests;

public class TestCollectionPaging {
  [Test]
  public void TestPageToEndSynchronous() {
    var originalList = Enumerable.Range(1, 100).ToList();
    var asPages = originalList.AsPages(10).ToList();
    var newList = Enumerable.Range(0, 1)
        .PageToEnd((_, p) => asPages[p.PageNumber - 1], 10)
        .ToList();
    Assert.That(newList, Is.EqualTo(originalList));
  }
  
  [Test]
  public async Task TestPageToEndAsynchronous() {
    var originalList = Enumerable.Range(1, 100)
        .ToList();
    var asPages = originalList
        .AsPages(10)
        .Select(Task.FromResult)
        .ToList();
    var newList = await Enumerable.Range(0, 1)
        .PageToEndAsync((_, p) => asPages[p.PageNumber - 1], 10)
        .ToListAsync();
    Assert.That(newList, Is.EqualTo(originalList));
  }
  
  [Test]
  public async Task TestPageToEndAsynchronousWithCancellation() {
    var originalList = Enumerable.Range(1, 100)
        .ToList();
    var asPages = originalList
        .AsPages(10)
        .Select(Task.FromResult)
        .ToList();
    var newList = await Enumerable.Range(0, 1)
        .PageToEndAsync((_, p, _) => asPages[p.PageNumber - 1], 10)
        .ToListAsync();
    Assert.That(newList, Is.EqualTo(originalList));
  }

  [Test]
  public void ConvertSimpleListToPage() {
    List<int> list = [1, 2, 3, 4, 5, 6, 7, 8, 9, 10];
    var page = list.ToPage(1, 5);
    Assert.That(page, Is.EqualTo([1, 2, 3, 4, 5]));
    
    var page2 = list.ToPage(new PageRequest(2, 5));
    Assert.That(page2, Is.EqualTo([6, 7, 8, 9, 10]));
  }
  
  [Test]
  public void UnpagedCopiesList() {
    List<int> list = [1, 2, 3, 4, 5, 6, 7, 8, 9, 10];
    var page = list.ToPage(Pageable.Unpaged);
    Assert.That(page, Is.EqualTo(list));
  }
}