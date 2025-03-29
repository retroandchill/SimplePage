using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Retro.SimplePage.Sample.Model;

namespace Retro.SimplePage.Requests.Tests;

public class PaginatedParameterTest {
  
  private WebApplicationFactory<Program> _factory;
  private HttpClient _client;

  [SetUp]
  public void Setup() {
    _factory = new WebApplicationFactory<Program>();
    _client = _factory.CreateClient();
  }
  
  [TearDown]
  public void TearDown() {
    _client.Dispose();
    _factory.Dispose();
  }
  
  [Test]
  public async Task TestWeatherRequest() {
    var result = await _client.GetAsync("/WeatherForecast?page=1&size=25");
    Assert.That(result.IsSuccessStatusCode);

    var page = await JsonSerializer.DeserializeAsync<Page<WeatherForecast>>(await result.Content.ReadAsStreamAsync());
    Assert.That(page, Is.Not.Null);
    
    Assert.That(page, Has.Count.EqualTo(25));
    Assert.That(page.TotalPages, Is.EqualTo(10000 / 25));
  }
  
  [Test]
  public async Task TestWeatherRequestDefaultPageSize() {
    var result = await _client.GetAsync("/WeatherForecast");
    Assert.That(result.IsSuccessStatusCode);

    var page = await JsonSerializer.DeserializeAsync<Page<WeatherForecast>>(await result.Content.ReadAsStreamAsync());
    Assert.That(page, Is.Not.Null);
    
    Assert.That(page, Has.Count.EqualTo(10));
    Assert.That(page.TotalPages, Is.EqualTo(10000 / 10));
  }
  
}