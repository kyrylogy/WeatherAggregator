namespace weather_api_blazor.Components.Services;

using weather_api_blazor.Components.Models;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

public class OpenWeatherService : IWeatherService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;

    public OpenWeatherService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _apiKey = "508a964434294b627d11f8737d543193"; // Store in config or secrets in production
    }

    public async Task<WeatherResult> GetWeatherByCoordinatesAsync(double lat, double lon)
    {
        var url = $"https://api.openweathermap.org/data/2.5/weather?lat={lat}&lon={lon}&appid={_apiKey}&units=metric";

        var response = await _httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        var data = JsonSerializer.Deserialize<JsonElement>(json);

        var main = data.GetProperty("main");
        var weather = data.GetProperty("weather")[0];
        var wind = data.GetProperty("wind");
        var clouds = data.GetProperty("clouds");

        return new WeatherResult
        {
            Source = "OpenWeatherMap",
            City = $"{lat:F2}, {lon:F2}",
            TemperatureCelsius = (float)main.GetProperty("temp").GetDouble(),
            Humidity = (float)main.GetProperty("humidity").GetDouble(),
            WindSpeed = (float)wind.GetProperty("speed").GetDouble(),
            Cloudiness = (float)clouds.GetProperty("all").GetDouble(),
        };
    }

    // Optionally still support city-based fetch
    public async Task<WeatherResult> GetWeatherAsync(string city)
    {
        var url = $"https://api.openweathermap.org/data/2.5/weather?q={city}&appid={_apiKey}&units=metric";

        var response = await _httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        var data = JsonSerializer.Deserialize<JsonElement>(json);

        var main = data.GetProperty("main");
        var weather = data.GetProperty("weather")[0];
        var wind = data.GetProperty("wind");
        var clouds = data.GetProperty("clouds");

        return new WeatherResult
        {
            Source = "OpenWeatherMap",
            City = city,
            TemperatureCelsius = (float)main.GetProperty("temp").GetDouble(),
            Humidity = (float)main.GetProperty("humidity").GetDouble(),
            WindSpeed = (float)wind.GetProperty("speed").GetDouble(),
            Cloudiness = (float)clouds.GetProperty("all").GetDouble(),
        };
    }
}
