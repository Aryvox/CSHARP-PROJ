using System.Text.Json;

namespace WeatherApp;

public class AppConfig
{
    public required string ApiKey { get; init; }
    public required string BaseUrl { get; init; }
    public string Unit { get; init; } = "metric";
    public string Lang { get; init; } = "fr";

    public static AppConfig Load()
    {
        var fullPath = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
        if (!File.Exists(fullPath))
        {
            throw new FileNotFoundException($"Configuration file not found: {fullPath}");
        }

        var json = File.ReadAllText(fullPath);
        using var document = JsonDocument.Parse(json);
        if (!document.RootElement.TryGetProperty("OpenWeather", out var weather))
        {
            throw new InvalidOperationException("Missing 'OpenWeather' section in appsettings.json.");
        }

        return new AppConfig
        {
            ApiKey = weather.GetProperty("ApiKey").GetString() ?? string.Empty,
            BaseUrl = weather.GetProperty("BaseUrl").GetString() ?? string.Empty,
            Unit = weather.GetProperty("Unit").GetString() ?? "metric",
            Lang = weather.GetProperty("Lang").GetString() ?? "fr"
        };
    }
}
