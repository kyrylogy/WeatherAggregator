using weather_api_blazor.Components;
using weather_api_blazor.Components.Services;
using weather_api_blazor.Components.Models;

// …

var builder = WebApplication.CreateBuilder(args);

// 1️⃣ Register each service as a typed HTTP client (no BaseAddress configuration)
//    The AddHttpClient<TClient>() overload wires up an HttpClient for injection, 
//    managed by IHttpClientFactory to avoid socket exhaustion and DNS issues. :contentReference[oaicite:0]{index=0}
builder.Services.AddHttpClient<OpenWeatherService>();
builder.Services.AddHttpClient<GeoSphereService>();


// 2️⃣ Register each implementation as IWeatherService
//    When you inject IEnumerable<IWeatherService>, all three implementations will be resolved. :contentReference[oaicite:1]{index=1}
builder.Services.AddTransient<IWeatherService, OpenWeatherService>();
builder.Services.AddTransient<IWeatherService, GeoSphereService>();
// builder.Services.AddTransient<IWeatherService, OikolabService>();

// 3️⃣ Register the AggregatorService
builder.Services.AddTransient<AggregatorService>();

// 4️⃣ Blazor component services
builder.Services
    .AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();

// … existing middleware …

app.UseHttpsRedirection();
app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();