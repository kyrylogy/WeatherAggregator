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
            _apiKey = "508a964434294b627d11f8737d543193";
        }

        public async Task<WeatherResult> GetWeatherAsync(string city)
        {
            var url = $"https://api.openweathermap.org/data/2.5/weather?q={city}&appid={_apiKey}&units=metric";

            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var data = JsonSerializer.Deserialize<JsonElement>(json);

            return new WeatherResult
            {
                Source = "OpenWeatherMap",
                City = city,
                TemperatureCelsius = (float)data.GetProperty("main").GetProperty("temp").GetSingle(),
                Condition = data.GetProperty("weather")[0].GetProperty("description").GetString()
            };
        }
    }