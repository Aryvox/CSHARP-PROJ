using System.Text.Json;

namespace WeatherApp;

public class FavoritesService
{
    private readonly string _filePath;

    public FavoritesService()
    {
        _filePath = Path.Combine(AppContext.BaseDirectory, "favorites.json");
    }

    public List<string> LoadFavorites()
    {
        if (!File.Exists(_filePath))
        {
            return [];
        }

        var json = File.ReadAllText(_filePath);
        var items = JsonSerializer.Deserialize<List<string>>(json);
        return items?.Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(x => x).ToList() ?? [];
    }

    public void SaveFavorites(List<string> favorites)
    {
        var content = JsonSerializer.Serialize(favorites.OrderBy(x => x), new JsonSerializerOptions
        {
            WriteIndented = true
        });

        File.WriteAllText(_filePath, content);
    }
}
