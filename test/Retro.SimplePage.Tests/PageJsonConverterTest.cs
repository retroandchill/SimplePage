using System.Text.Json;

namespace Retro.SimplePage.Tests;

public class PageJsonConverterTest {
  private static readonly JsonSerializerOptions Options = new();

  [Test]
  public void TestSerializePrimitivePage() {
    var page = new Page<int>([1, 2, 3, 4, 5], 5, 5, 10);
    var serialized = JsonSerializer.Serialize(page, Options);
    var deserialized = JsonSerializer.Deserialize<Page<int>>(serialized, Options);
    Assert.That(deserialized, Is.EqualTo(page));
  }
}