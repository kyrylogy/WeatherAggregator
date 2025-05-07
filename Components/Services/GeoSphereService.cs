using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using weather_api_blazor.Components.Models;

namespace weather_api_blazor.Components.Services;

public class GeoSphereService : IWeatherService
{
    private readonly HttpClient _httpClient;
    private List<StationInfo>? _stationsCache;
    private readonly Random _rng = new();

    public GeoSphereService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    /// <summary>
    /// Loads and caches station metadata (ids, names, coords).
    /// </summary>
    private async Task EnsureStationsLoadedAsync()
    {
        if (_stationsCache != null)
            return;

        var metaUrl = "https://dataset.api.hub.geosphere.at/v1/station/current/tawes-v1-10min/metadata";
        using var response = await _httpClient.GetAsync(metaUrl);
        response.EnsureSuccessStatusCode();

        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var root = doc.RootElement;

        var ids = root.GetProperty("station_ids").EnumerateArray();
        var names = root.GetProperty("station_names").EnumerateArray();
        var coords = root.GetProperty("coordinates").EnumerateArray();

        var stations = new List<StationInfo>();
        using var idEnum = ids.GetEnumerator();
        using var nameEnum = names.GetEnumerator();
        using var coordEnum = coords.GetEnumerator();

        while (idEnum.MoveNext() && nameEnum.MoveNext() && coordEnum.MoveNext())
        {
            var coordArr = coordEnum.Current.EnumerateArray().ToArray();
            stations.Add(new StationInfo
            {
                Id = idEnum.Current.GetString()!,
                Name = nameEnum.Current.GetString()!,
                Longitude = coordArr[0].GetDouble(),
                Latitude = coordArr[1].GetDouble(),
            });
        }

        _stationsCache = stations;
    }

    /// <summary>
    /// Searches cached stations by name.
    /// </summary>
    public async Task<IEnumerable<StationInfo>> SearchStationsAsync(string query)
    {
        await EnsureStationsLoadedAsync();
        var q = query.Trim().ToLowerInvariant();
        return _stationsCache!
            .Where(s => s.Name.Contains(q, StringComparison.OrdinalIgnoreCase))
            .Take(20);
    }

    /// <summary>
    /// Returns all loaded stations.
    /// </summary>
    public async Task<IReadOnlyList<StationInfo>> GetAllStationsAsync()
    {
        await EnsureStationsLoadedAsync();
        return _stationsCache!;
    }

    /// <summary>
    /// Picks a random station from the cache.
    /// </summary>
    public async Task<StationInfo> GetRandomStationAsync()
    {
        await EnsureStationsLoadedAsync();
        var list = _stationsCache!;
        if (list.Count == 0)
            throw new InvalidOperationException("No stations available.");
        return list[_rng.Next(list.Count)];
    }

    /// <summary>
    /// Fetches current temperature for the given station ID.
    /// </summary>
    public async Task<WeatherResult> GetWeatherAsync(string stationId)
    {
        var url = $"https://dataset.api.hub.geosphere.at/v1/station/current/tawes-v1-10min?parameters=TL&station_ids={stationId}";
        using var response = await _httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();

        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var firstEntry = doc.RootElement.GetProperty("data").EnumerateArray().First();
        var coordEntry = firstEntry.GetProperty("coordinates").EnumerateArray().First();
        var dateEntry = coordEntry.GetProperty("dates").EnumerateArray().First();
        var tempValue = dateEntry.GetProperty("value").GetDouble();

        return new WeatherResult
        {
            Source = "GeoSphere Austria",
            City = stationId,
            TemperatureCelsius = (float)tempValue,
            Condition = "N/A"
        };
    }
}
