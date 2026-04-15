using MySql.Data.MySqlClient;

namespace LibraryManagerApp;

public class BookRepository
{
    private readonly string _connectionString;

    public BookRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public List<Book> GetAll()
    {
        const string sql = """
            SELECT id, title, author, isbn, publication_year, genre, shelf_section, shelf_number, is_available
            FROM books
            ORDER BY title;
            """;

        return ExecuteReader(sql);
    }

    public List<Book> Search(string? title, string? author, string? genre, string? isbn)
    {
        const string sql = """
            SELECT id, title, author, isbn, publication_year, genre, shelf_section, shelf_number, is_available
            FROM books
            WHERE (@title IS NULL OR title LIKE CONCAT('%', @title, '%'))
              AND (@author IS NULL OR author LIKE CONCAT('%', @author, '%'))
              AND (@genre IS NULL OR genre LIKE CONCAT('%', @genre, '%'))
              AND (@isbn IS NULL OR isbn LIKE CONCAT('%', @isbn, '%'))
            ORDER BY title;
            """;

        return ExecuteReader(sql, cmd =>
        {
            cmd.Parameters.AddWithValue("@title", Normalize(title));
            cmd.Parameters.AddWithValue("@author", Normalize(author));
            cmd.Parameters.AddWithValue("@genre", Normalize(genre));
            cmd.Parameters.AddWithValue("@isbn", Normalize(isbn));
        });
    }

    public void Add(Book book)
    {
        const string sql = """
            INSERT INTO books (title, author, isbn, publication_year, genre, shelf_section, shelf_number, is_available)
            VALUES (@title, @author, @isbn, @publicationYear, @genre, @shelfSection, @shelfNumber, @isAvailable);
            """;

        ExecuteNonQuery(sql, cmd => FillBookParameters(cmd, book));
    }

    public void Update(Book book)
    {
        const string sql = """
            UPDATE books
            SET title = @title,
                author = @author,
                isbn = @isbn,
                publication_year = @publicationYear,
                genre = @genre,
                shelf_section = @shelfSection,
                shelf_number = @shelfNumber,
                is_available = @isAvailable
            WHERE id = @id;
            """;

        ExecuteNonQuery(sql, cmd =>
        {
            FillBookParameters(cmd, book);
            cmd.Parameters.AddWithValue("@id", book.Id);
        });
    }

    public void Delete(int id)
    {
        const string sql = "DELETE FROM books WHERE id = @id;";
        ExecuteNonQuery(sql, cmd => cmd.Parameters.AddWithValue("@id", id));
    }

    private List<Book> ExecuteReader(string sql, Action<MySqlCommand>? bind = null)
    {
        var books = new List<Book>();

        using var connection = new MySqlConnection(_connectionString);
        connection.Open();

        using var command = new MySqlCommand(sql, connection);
        bind?.Invoke(command);

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            books.Add(new Book
            {
                Id = reader.GetInt32("id"),
                Title = reader.GetString("title"),
                Author = reader.GetString("author"),
                Isbn = reader.GetString("isbn"),
                PublicationYear = reader.GetInt32("publication_year"),
                Genre = reader.GetString("genre"),
                ShelfSection = reader.GetString("shelf_section"),
                ShelfNumber = reader.GetString("shelf_number"),
                IsAvailable = reader.GetBoolean("is_available")
            });
        }

        return books;
    }

    private void ExecuteNonQuery(string sql, Action<MySqlCommand> bind)
    {
        using var connection = new MySqlConnection(_connectionString);
        connection.Open();

        using var command = new MySqlCommand(sql, connection);
        bind(command);
        command.ExecuteNonQuery();
    }

    private static string? Normalize(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private static void FillBookParameters(MySqlCommand cmd, Book book)
    {
        cmd.Parameters.AddWithValue("@title", book.Title);
        cmd.Parameters.AddWithValue("@author", book.Author);
        cmd.Parameters.AddWithValue("@isbn", book.Isbn);
        cmd.Parameters.AddWithValue("@publicationYear", book.PublicationYear);
        cmd.Parameters.AddWithValue("@genre", book.Genre);
        cmd.Parameters.AddWithValue("@shelfSection", book.ShelfSection);
        cmd.Parameters.AddWithValue("@shelfNumber", book.ShelfNumber);
        cmd.Parameters.AddWithValue("@isAvailable", book.IsAvailable);
    }
}
