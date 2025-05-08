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
        /// Fetches weather from all registered services in parallel for the given location key  by city or station id
        /// </summary>
        public async Task<List<WeatherResult>> GetAggregatedWeatherAsync(string locationKey)
        {
            var tasks = _weatherServices
                .Select(svc => svc.GetWeatherAsync(locationKey));

            // start calls one by one and get in-flight processes
            var results = await Task.WhenAll(tasks);
            return results.ToList();
        }
    }
