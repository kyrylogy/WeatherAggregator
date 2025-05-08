namespace weather_api_blazor.Components.Models;

public class WeatherResult
{
    public string Source { get; set; } = string.Empty;      
    public string City { get; set; } = string.Empty;          
    public float TemperatureCelsius { get; set; }            
    public float Cloudiness { get; set; } 
    public float Humidity { get; set; } 
    public float WindSpeed { get; set; } 
}



