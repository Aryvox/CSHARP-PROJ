namespace WeatherApp;

public class WeatherForecastItem
{
    public DateTime DateTime { get; set; }
    public double Temperature { get; set; }
    public int CloudCoverage { get; set; }
    public int Humidity { get; set; }
    public double WindSpeed { get; set; }
    public string Description { get; set; } = string.Empty;
    public string IconCode { get; set; } = string.Empty;
    public string IconUrl => $"https://openweathermap.org/img/wn/{IconCode}@2x.png";
}
