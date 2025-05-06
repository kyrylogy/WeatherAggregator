namespace weather_api_blazor.Components.Services;

using weather_api_blazor.Components.Models;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;


public class GeoSphereService : IWeatherService
    {
        private readonly HttpClient _httpClient;

        public GeoSphereService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<WeatherResult> GetWeatherAsync(string stationId)
        {
            // GeoSphere Austria API endpoint for current temperature data
            var url = $"https://dataset.api.hub.geosphere.at/v1/station/current/tawes-v1-10min?parameters=TL&station_ids={stationId}";

            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var data = JsonSerializer.Deserialize<JsonElement>(json);

            // Extract temperature value
            var temperature = data
                .GetProperty("data")
                .EnumerateArray()
                .First()
                .GetProperty("coordinates")
                .EnumerateArray()
                .First()
                .GetProperty("dates")
                .EnumerateArray()
                .First()
                .GetProperty("value")
                .GetDouble();

            return new WeatherResult
            {
                Source = "GeoSphere Austria",
                City = stationId, // You might want to map station IDs to city names
                TemperatureCelsius = (float)temperature,
                Condition = "N/A" // GeoSphere API may not provide condition descriptions
            };
        }
    }
