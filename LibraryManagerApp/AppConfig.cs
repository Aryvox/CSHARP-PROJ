using System.Text.Json;

namespace LibraryManagerApp;

public static class AppConfig
{
    private const string FileName = "appsettings.json";

    public static string GetConnectionString()
    {
        var fullPath = Path.Combine(AppContext.BaseDirectory, FileName);
        if (!File.Exists(fullPath))
        {
            throw new FileNotFoundException($"Configuration file not found: {fullPath}");
        }

        var json = File.ReadAllText(fullPath);
        using var document = JsonDocument.Parse(json);
        if (!document.RootElement.TryGetProperty("ConnectionStrings", out var connectionStrings) ||
            !connectionStrings.TryGetProperty("LibraryDb", out var value))
        {
            throw new InvalidOperationException("Connection string 'LibraryDb' is missing in appsettings.json.");
        }

        return value.GetString() ?? throw new InvalidOperationException("Connection string 'LibraryDb' is empty.");
    }
}
