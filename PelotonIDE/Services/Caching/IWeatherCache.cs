using PelotonIDE.DataContracts;
using System.Collections.Immutable;

namespace PelotonIDE.Services.Caching
{
    public interface IWeatherCache
    {
        ValueTask<IImmutableList<WeatherForecast>> GetForecast(CancellationToken token);
    }
}