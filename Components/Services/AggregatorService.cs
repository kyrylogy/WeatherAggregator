namespace weather_api_blazor.Components.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using weather_api_blazor.Components.Models;


public class AggregatorService
    {
        private readonly IEnumerable<IWeatherService> _weatherServices;

        public AggregatorService(IEnumerable<IWeatherService> weatherServices)
        {
            _weatherServices = weatherServices;
        }

        /// <summary>
        /// Fetches weather from all registered services in parallel for the given location key (e.g. city or station code).
        /// </summary>
        public async Task<List<WeatherResult>> GetAggregatedWeatherAsync(string locationKey)
        {
            // Kick off all requests in parallel
            var tasks = _weatherServices
                .Select(svc => svc.GetWeatherAsync(locationKey));

            // Wait for them all
            var results = await Task.WhenAll(tasks);

            // Return as a list
            return results.ToList();
        }
    }
