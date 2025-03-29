using Microsoft.AspNetCore.Mvc;
using Retro.SimplePage.Sample.Model;

namespace Retro.SimplePage.Sample.Controllers;

[ApiController]
[Route("[controller]")]
public class WeatherForecastController : ControllerBase {
  private static readonly string[] Summaries = [
      "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
  ];

  [HttpGet(Name = "GetWeatherForecast")]
  public Page<WeatherForecast> Get([FromQuery] Pageable pageable) {
    return Enumerable.Range(1, 10000)
        .Select(index => new WeatherForecast {
            Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            TemperatureC = Random.Shared.Next(-20, 55),
            Summary = Summaries[Random.Shared.Next(Summaries.Length)]
        })
        .ToPage(pageable);
  }
}