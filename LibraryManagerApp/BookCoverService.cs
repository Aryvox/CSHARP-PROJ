namespace LibraryManagerApp;

public class BookCoverService
{
    private readonly HttpClient _httpClient = new();

    public async Task<Image?> TryGetCoverByIsbnAsync(string isbn, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(isbn))
        {
            return null;
        }

        var cleanedIsbn = isbn.Replace("-", string.Empty).Trim();
        var url = $"https://covers.openlibrary.org/b/isbn/{Uri.EscapeDataString(cleanedIsbn)}-L.jpg?default=false";

        try
        {
            using var response = await _httpClient.GetAsync(url, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            await using var memoryStream = new MemoryStream();
            await stream.CopyToAsync(memoryStream, cancellationToken);
            if (memoryStream.Length == 0)
            {
                return null;
            }

            memoryStream.Position = 0;
            return Image.FromStream(memoryStream);
        }
        catch
        {
            return null;
        }
    }
}
