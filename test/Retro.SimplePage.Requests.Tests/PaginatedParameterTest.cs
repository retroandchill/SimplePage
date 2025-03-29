using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Retro.SimplePage.Sample.Model;
using Swashbuckle.AspNetCore.Swagger;

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
  
  [Test]
  public void SwaggerDocumentation_ShouldIncludePageableParameters() {
    var swaggerProvider = _factory.Services.GetRequiredService<ISwaggerProvider>();
    var swaggerJson = swaggerProvider.GetSwagger("v1");
    Assert.That(swaggerJson, Is.Not.Null);

    var operation = swaggerJson.Paths["/WeatherForecast"]?
        .Operations[OperationType.Get];
    Assert.That(operation, Is.Not.Null, "Operation not found for /WeatherForecast");

    // Assert: Check parameters
    var pageParameter = operation.Parameters.FirstOrDefault(p => p.Name == "page");
    var sizeParameter = operation.Parameters.FirstOrDefault(p => p.Name == "size");

    Assert.Multiple(() => {
      Assert.That(pageParameter, Is.Not.Null, "The 'page' parameter is missing in the OpenAPI schema.");
      Assert.That(sizeParameter, Is.Not.Null, "The 'size' parameter is missing in the OpenAPI schema.");
    });

    Assert.Multiple(() => {
      Assert.That(pageParameter.Schema.Type, Is.EqualTo("integer"), "Page parameter is not of type integer.");
      Assert.That(pageParameter.Schema.Default, Is.InstanceOf<OpenApiInteger>(), "Page parameter is not of type integer.");
      Assert.That(((OpenApiInteger) pageParameter.Schema.Default).Value, Is.EqualTo(1), "Page parameter default value is incorrect.");
      Assert.That(pageParameter.Schema.Minimum, Is.EqualTo(1), "Page parameter minimum value is incorrect.");

      Assert.That(sizeParameter.Schema.Type, Is.EqualTo("integer"), "Size parameter is not of type integer.");
      Assert.That(sizeParameter.Schema.Default, Is.InstanceOf<OpenApiInteger>(), "Size parameter is not of type integer.");
      Assert.That(((OpenApiInteger) sizeParameter.Schema.Default).Value, Is.EqualTo(10), "Size parameter default value is incorrect.");
      Assert.That(sizeParameter.Schema.Minimum, Is.EqualTo(1), "Size parameter minimum value is incorrect.");
      Assert.That(sizeParameter.Schema.Maximum, Is.EqualTo(100), "Size parameter maximum value is incorrect.");
    });
  }
  
  [Test]
  public void SwaggerDocumentation_CreatedPageType() {
    var swaggerProvider = _factory.Services.GetRequiredService<ISwaggerProvider>();
    var swaggerJson = swaggerProvider.GetSwagger("v1");
    Assert.That(swaggerJson, Is.Not.Null);
  
    var type = swaggerJson.Components.Schemas["WeatherForecastPage"];
    Assert.That(type, Is.Not.Null, "WeatherForecastPage type not found");
  
    // Assert: Check parameters
    Assert.That(type.Properties, Does.ContainKey("pageNumber"), "PageNumber property is missing.");
    Assert.That(type.Properties["pageNumber"].Type, Is.EqualTo("integer"), "PageNumber property is not of type integer.");
  
    Assert.That(type.Properties, Does.ContainKey("pageSize"), "PageSize property is missing.");
    Assert.That(type.Properties["pageSize"].Type, Is.EqualTo("integer"), "PageSize property is not of type integer.");
  
    Assert.That(type.Properties, Does.ContainKey("totalPages"), "TotalPages property is missing.");
    Assert.That(type.Properties["totalPages"].Type, Is.EqualTo("integer"), "TotalPages property is not of type integer.");
  
    Assert.That(type.Properties.ContainsKey("count"), "Count property is missing.");
    Assert.That(type.Properties["count"].Type, Is.EqualTo("integer"), "count property is not of type integer.");
  
    Assert.That(type.Properties.ContainsKey("items"), "Items property is missing.");
    Assert.That(type.Properties["items"].Type, Is.EqualTo("array"), "Items property is not of type array.");
  }
  
}