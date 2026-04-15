using System.Text.Json;

namespace WeatherApp;

public class OpenWeatherService
{
    private readonly HttpClient _httpClient = new();
    private readonly AppConfig _config;

    public OpenWeatherService(AppConfig config)
    {
        _config = config;
    }

    public async Task<List<WeatherForecastItem>> GetForecastAsync(string city, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_config.ApiKey) || _config.ApiKey.StartsWith("PUT_YOUR", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Configure your OpenWeather API key in WeatherApp/appsettings.json.");
        }

        var uri =
            $"{_config.BaseUrl}?q={Uri.EscapeDataString(city)}&appid={_config.ApiKey}&units={_config.Unit}&lang={_config.Lang}";
        var json = await _httpClient.GetStringAsync(uri, cancellationToken);

        using var document = JsonDocument.Parse(json);
        if (!document.RootElement.TryGetProperty("list", out var list))
        {
            return [];
        }

        var result = new List<WeatherForecastItem>();
        foreach (var item in list.EnumerateArray())
        {
            var weather = item.GetProperty("weather")[0];
            result.Add(new WeatherForecastItem
            {
                DateTime = DateTime.Parse(item.GetProperty("dt_txt").GetString() ?? DateTime.UtcNow.ToString("s")),
                Temperature = item.GetProperty("main").GetProperty("temp").GetDouble(),
                Humidity = item.GetProperty("main").GetProperty("humidity").GetInt32(),
                CloudCoverage = item.GetProperty("clouds").GetProperty("all").GetInt32(),
                WindSpeed = item.GetProperty("wind").GetProperty("speed").GetDouble(),
                Description = weather.GetProperty("description").GetString() ?? string.Empty,
                IconCode = weather.GetProperty("icon").GetString() ?? string.Empty
            });
        }

        return result;
    }
}
