namespace LibraryManagerApp;

public class Book
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string Isbn { get; set; } = string.Empty;
    public int PublicationYear { get; set; }
    public string Genre { get; set; } = string.Empty;
    public string ShelfSection { get; set; } = string.Empty;
    public string ShelfNumber { get; set; } = string.Empty;
    public bool IsAvailable { get; set; }
}
