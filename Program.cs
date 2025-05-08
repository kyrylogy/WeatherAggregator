using weather_api_blazor.Components;
using weather_api_blazor.Components.Services;
using weather_api_blazor.Components.Models;

// …

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient<OpenWeatherService>();
builder.Services.AddHttpClient<GeoSphereService>();

builder.Services.AddTransient<IWeatherService, OpenWeatherService>();
builder.Services.AddTransient<IWeatherService, GeoSphereService>();
// builder.Services.AddTransient<IWeatherService, OikolabService>();

builder.Services.AddTransient<AggregatorService>();

builder.Services
    .AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();

app.UseHttpsRedirection();
app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();