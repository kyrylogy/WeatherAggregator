using weather_api_blazor.Components.Models;

namespace weather_api_blazor.Components.Services;

public interface IWeatherService
{
    Task<WeatherResult> GetWeatherAsync(string city);
}