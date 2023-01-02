using System.Collections.Generic;
using System.Threading.Tasks;
using Globomantics.Core.ClientModel;

namespace Globomantics.Core.Services
{
    public interface IApiClient
    {
        Task<List<WeatherForecastModel>> GetWeatherForecastAsync();
        Task<List<City>> GetCitiesAsync();
    }
}
