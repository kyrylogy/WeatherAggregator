using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using weather_api_blazor.Components.Models;

namespace weather_api_blazor.Components.Services;

public class GeoSphereService : IWeatherService
{
    private readonly HttpClient _httpClient;
    private readonly Random _rng = new();

    public GeoSphereService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    /// <summary>
    /// Finds and returns the first station whose ID or Name matches the query (case-insensitive).
    /// Scans the "stations" array in the metadata JSON on each invocation.
    /// </summary>
    public async Task<StationInfo?> FindStationAsync(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
            return null;

        var metaUrl = "https://dataset.api.hub.geosphere.at/v1/station/current/tawes-v1-10min/metadata";
        using var response = await _httpClient.GetAsync(metaUrl);
        response.EnsureSuccessStatusCode();

        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var root = doc.RootElement;
        if (!root.TryGetProperty("stations", out var stationsElement) ||
            stationsElement.ValueKind != JsonValueKind.Array)
        {
            throw new InvalidOperationException("Unexpected metadata format: 'stations' array missing or invalid.");
        }

        var normalized = query.Trim();
        foreach (var element in stationsElement.EnumerateArray())
        {
            if (!element.TryGetProperty("id", out var idProp) ||
                !element.TryGetProperty("name", out var nameProp))
                continue;

            var id = idProp.GetString()!;
            var name = nameProp.GetString()!;
            var state = element.TryGetProperty("state", out var stateProp) ? stateProp.GetString()! : string.Empty;
            var lat = element.TryGetProperty("lat", out var latProp) ? latProp.GetDouble() : 0;
            var lon = element.TryGetProperty("lon", out var lonProp) ? lonProp.GetDouble() : 0;

            if (string.Equals(id, normalized, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(name, normalized, StringComparison.OrdinalIgnoreCase))
            {
                return new StationInfo
                {
                    Id = id,
                    Name = name,
                    State = state,
                    Latitude = lat,
                    Longitude = lon
                };
            }
        }

        return null;
    }

    /// <summary>
    /// Picks a random station from the "stations" array and returns its info.
    /// </summary>
    public async Task<StationInfo> GetRandomStationAsync()
    {
        var metaUrl = "https://dataset.api.hub.geosphere.at/v1/station/current/tawes-v1-10min/metadata";
        using var response = await _httpClient.GetAsync(metaUrl);
        response.EnsureSuccessStatusCode();

        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var root = doc.RootElement;
        if (!root.TryGetProperty("stations", out var stationsElement) ||
            stationsElement.ValueKind != JsonValueKind.Array)
        {
            throw new InvalidOperationException("Unexpected metadata format: 'stations' array missing or invalid.");
        }

        var array = stationsElement.EnumerateArray().ToArray();
        if (array.Length == 0)
            throw new InvalidOperationException("No stations available.");

        var element = array[_rng.Next(array.Length)];
        var id = element.GetProperty("id").GetString()!;
        var name = element.GetProperty("name").GetString()!;
        var state = element.TryGetProperty("state", out var stateProp) ? stateProp.GetString()! : string.Empty;
        var lat = element.TryGetProperty("lat", out var latProp) ? latProp.GetDouble() : 0;
        var lon = element.TryGetProperty("lon", out var lonProp) ? lonProp.GetDouble() : 0;

        return new StationInfo
        {
            Id = id,
            Name = name,
            State = state,
            Latitude = lat,
            Longitude = lon
        };
    }

    /// <summary>
    /// Fetches current temperature for the given station ID.
    /// </summary>
    public async Task<WeatherResult> GetWeatherAsync(string stationId)
    {
        if (string.IsNullOrWhiteSpace(stationId))
            throw new ArgumentException("Station ID must be provided.", nameof(stationId));

        // keep your template for querying multiple parameters
        var parameterList = new[] { "TL", "TP", "RF", "P", "PRED", "DD", "FFAM", "FFX", "SO" };
        var parametersQuery = string.Join(",", parameterList);

        var url = $"https://dataset.api.hub.geosphere.at/v1/station/current/tawes-v1-10min"
                  + $"?station_ids={stationId}"
                  + $"&parameters={parametersQuery}";

        using var response = await _httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();

        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var root = doc.RootElement;

        // GeoSphere returns a GeoJSON FeatureCollection, not "data"
        var feature = root
            .GetProperty("features")
            .EnumerateArray()
            .FirstOrDefault();
        if (feature.ValueKind == JsonValueKind.Undefined)
            throw new InvalidOperationException($"No data returned for station '{stationId}'");

        var parameters = feature
            .GetProperty("properties")
            .GetProperty("parameters");

// Temperature
        var tempValue = parameters.GetProperty("TL").GetProperty("data").EnumerateArray().First().GetDouble();

// Humidity (RH)
        var humidityValue = parameters.TryGetProperty("RF", out var rf) && rf.TryGetProperty("data", out var rfData)
            ? rfData.EnumerateArray().FirstOrDefault().GetDouble()
            : double.NaN;

// Wind Speed (FFAM)
        var windSpeedValue = parameters.TryGetProperty("FFAM", out var ffam) && ffam.TryGetProperty("data", out var ffamData)
            ? ffamData.EnumerateArray().FirstOrDefault().GetDouble()
            : double.NaN;

// Cloudiness - GeoSphere does not provide it directly in standard TAWES dataset
        var cloudiness = double.NaN;

        return new WeatherResult
        {
            Source             = "GeoSphere Austria",
            City               = stationId,
            TemperatureCelsius = (float)tempValue,
            Humidity           = (float)(double.IsNaN(humidityValue) ? 0 : humidityValue),
            WindSpeed          = (float)(double.IsNaN(windSpeedValue) ? 0 : windSpeedValue),
            Cloudiness         = (float)(double.IsNaN(cloudiness) ? 0 : cloudiness)
        };
    }


    /// <summary>
    /// Picks a random station and fetches its current temperature.
    /// </summary>
    public async Task<WeatherResult> GetRandomWeatherAsync()
    {
        var station = await GetRandomStationAsync();
        return await GetWeatherAsync(station.Id);
    }
}
