namespace weather_api_blazor.Components.Models;

public class StationInfo
{
    public string Id { get; init; } = "";
    public string Name { get; init; } = "";
    public string State { get; init; } = "";
    public double Latitude { get; init; }
    public double Longitude { get; init; }
}