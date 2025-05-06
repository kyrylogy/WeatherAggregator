namespace weather_api_blazor.Components.Services;

using System;
using System.Collections.Generic;

public class RegionStations
{
    public string Territory { get; init; } = string.Empty;
    public IReadOnlyList<string> CityStationIds { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> MountainStationIds { get; init; } = Array.Empty<string>();
}

public static class StationMapping
{
    /// <summary>
    /// Mapping of Austrian states to their city- and mountain-station IDs.
    /// </summary>
    public static readonly Dictionary<string, RegionStations> Regions
        = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Vienna"] = new RegionStations
        {
            Territory         = "Vienna",
            CityStationIds    = new[] { "11035" },   // Wien / Hohe Warte :contentReference[oaicite:7]{index=7}
            MountainStationIds= Array.Empty<string>()
        },
        ["Salzburg"] = new RegionStations
        {
            Territory         = "Salzburg",
            CityStationIds    = new[] { "11150" },   // Salzburg-Flughafen :contentReference[oaicite:8]{index=8}
            MountainStationIds= new[] { "15402" }    // Rauris :contentReference[oaicite:9]{index=9}
        },
        ["Tyrol"] = new RegionStations
        {
            Territory         = "Tyrol",
            CityStationIds    = new[] { "11121" },   // Innsbruck Airport (common WMO) 
            MountainStationIds= new[] { "15411", "15344" } // Sonnblick, Kals :contentReference[oaicite:10]{index=10}
        },
        ["Carinthia"] = new RegionStations
        {
            Territory         = "Carinthia",
            CityStationIds    = new[] { "11121"},
            MountainStationIds= Array.Empty<string>()
        },
        ["Styria"] = new RegionStations
        {
            Territory         = "Styria",
            CityStationIds    = new[] { "11121"},
            MountainStationIds= Array.Empty<string>()
        },
        // … add the remaining states …
    };
}
