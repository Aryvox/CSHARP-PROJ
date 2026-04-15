namespace LibraryManagerApp;

public class MainForm : Form
{
    private BookRepository? _repository;
    private readonly BookCoverService _bookCoverService = new();
    private readonly BindingSource _bindingSource = new();
    private readonly List<Book> _currentBooks = [];
    private readonly ContextMenuStrip _rowMenu = new();

    private readonly TextBox _titleBox = new() { Width = 200 };
    private readonly TextBox _authorBox = new() { Width = 200 };
    private readonly TextBox _isbnBox = new() { Width = 140 };
    private readonly NumericUpDown _yearBox = new() { Minimum = 0, Maximum = 3000, Width = 90 };
    private readonly TextBox _genreBox = new() { Width = 140 };
    private readonly TextBox _sectionBox = new() { Width = 100 };
    private readonly TextBox _shelfBox = new() { Width = 100 };
    private readonly CheckBox _availabilityBox = new() { Text = "Disponible", AutoSize = true };
    private readonly Label _selectedLabel = new() { Text = "Aucun livre selectionne", AutoSize = true, Font = new Font("Segoe UI", 10F, FontStyle.Bold) };
    private readonly PictureBox _coverPreview = new() { Width = 180, Height = 240, SizeMode = PictureBoxSizeMode.StretchImage, BorderStyle = BorderStyle.FixedSingle };
    private SplitContainer? _mainSplit;
    private CancellationTokenSource? _coverCts;

    private readonly DataGridView _booksGrid = new()
    {
        Dock = DockStyle.Fill,
        ReadOnly = true,
        AllowUserToAddRows = false,
        AutoGenerateColumns = false,
        SelectionMode = DataGridViewSelectionMode.FullRowSelect,
        MultiSelect = false
    };

    public MainForm()
    {
        Text = "Gestion de Bibliotheque";
        Width = 1320;
        Height = 790;
        StartPosition = FormStartPosition.CenterScreen;
        Font = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point);
        BackColor = Color.WhiteSmoke;

        var topPanel = BuildTopPanel();
        var contentPanel = BuildContentPanel();

        Controls.Add(contentPanel);
        Controls.Add(topPanel);

        _booksGrid.DataSource = _bindingSource;
        _booksGrid.SelectionChanged += (_, _) => LoadSelectionInInputs();
        _booksGrid.CellContentClick += BooksGridOnCellContentClick;
        _booksGrid.CellDoubleClick += (_, _) => LoadSelectionInInputs();

        ConfigureGridStyle();
        ConfigureMenu();
        InitializeRepository();
        Shown += (_, _) =>
        {
            ConfigureSplitConstraints();
            ApplySafeSplitterDistance();
        };
        Resize += (_, _) => ApplySafeSplitterDistance();
    }

    private void InitializeRepository()
    {
        try
        {
            _repository = new BookRepository(AppConfig.GetConnectionString());
            LoadBooks();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Connexion a la bibliotheque indisponible.\n\n{ex.Message}\n\nL'interface reste ouverte, verifie MySQL et appsettings.json puis clique sur 'Tout afficher'.",
                "Configuration requise",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
            _selectedLabel.Text = "Connexion BDD non disponible";
        }
    }

    private Control BuildTopPanel()
    {
        var panel = new Panel { Dock = DockStyle.Top, Height = 220, Padding = new Padding(8) };
        var group = new GroupBox
        {
            Dock = DockStyle.Fill,
            Text = "Formulaire Livre",
            BackColor = Color.White,
            Padding = new Padding(10)
        };
        var layout = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            WrapContents = true,
            AutoScroll = true,
            Padding = new Padding(8)
        };

        layout.Controls.AddRange(
        [
            Labeled("Titre", _titleBox),
            Labeled("Auteur", _authorBox),
            Labeled("ISBN", _isbnBox),
            Labeled("Annee", _yearBox),
            Labeled("Genre", _genreBox),
            Labeled("Rayon", _sectionBox),
            Labeled("Etagere", _shelfBox),
            _availabilityBox,
            ActionButton("Ajouter", (_, _) => AddBook(), Color.FromArgb(40, 167, 69)),
            ActionButton("Modifier", (_, _) => UpdateBook(), Color.FromArgb(255, 159, 64)),
            ActionButton("Supprimer", (_, _) => DeleteBook(), Color.FromArgb(220, 53, 69)),
            ActionButton("Rechercher", (_, _) => SearchBooks(), Color.FromArgb(46, 117, 182)),
            ActionButton("Tout afficher", (_, _) => { ClearInputs(); LoadBooks(); }, Color.FromArgb(108, 117, 125))
        ]);

        group.Controls.Add(layout);
        panel.Controls.Add(group);
        return panel;
    }

    private Control BuildContentPanel()
    {
        _mainSplit = new SplitContainer
        {
            Dock = DockStyle.Fill,
            Orientation = Orientation.Vertical,
            BackColor = Color.WhiteSmoke,
            Panel1MinSize = 100,
            Panel2MinSize = 100
        };

        var listGroup = new GroupBox
        {
            Dock = DockStyle.Fill,
            Text = "Liste complete des livres",
            BackColor = Color.White,
            Padding = new Padding(8)
        };
        listGroup.Controls.Add(_booksGrid);

        var rightGroup = new GroupBox
        {
            Dock = DockStyle.Fill,
            Text = "Apercu visuel / action rapide",
            BackColor = Color.White,
            Padding = new Padding(12)
        };

        var detailsLayout = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            AutoScroll = true
        };

        detailsLayout.Controls.Add(_selectedLabel);
        detailsLayout.Controls.Add(new Label { Text = "Couverture (visuel automatique)", AutoSize = true, Margin = new Padding(0, 10, 0, 6) });
        detailsLayout.Controls.Add(_coverPreview);
        detailsLayout.Controls.Add(new Label
        {
            Text = "Astuce: clique sur \"...\" a droite d'une ligne\npour Modifier/Supprimer rapidement.",
            AutoSize = true,
            Margin = new Padding(0, 12, 0, 0),
            ForeColor = Color.DimGray
        });

        rightGroup.Controls.Add(detailsLayout);

        _mainSplit.Panel1.Controls.Add(listGroup);
        _mainSplit.Panel2.Controls.Add(rightGroup);
        return _mainSplit;
    }

    private void ApplySafeSplitterDistance()
    {
        if (_mainSplit is null)
        {
            return;
        }

        var total = _mainSplit.ClientSize.Width;
        var min = _mainSplit.Panel1MinSize;
        var max = total - _mainSplit.Panel2MinSize;
        if (max <= min)
        {
            return;
        }

        var target = (int)(total * 0.72);
        try
        {
            _mainSplit.SplitterDistance = Math.Clamp(target, min, max);
        }
        catch
        {
            // Ignore transient WinForms layout constraints during startup.
        }
    }

    private void ConfigureSplitConstraints()
    {
        if (_mainSplit is null)
        {
            return;
        }

        _mainSplit.Panel1MinSize = 600;
        _mainSplit.Panel2MinSize = 240;
    }

    private static Control Labeled(string labelText, Control control)
    {
        var panel = new Panel { Width = control.Width + 10, Height = 52 };
        var label = new Label { Text = labelText, AutoSize = true, Top = 0 };
        control.Top = 22;
        panel.Controls.Add(label);
        panel.Controls.Add(control);
        return panel;
    }

    private static Button ActionButton(string text, EventHandler handler, Color color)
    {
        var button = new Button
        {
            Text = text,
            Width = 125,
            Height = 38,
            Margin = new Padding(8, 18, 0, 0),
            FlatStyle = FlatStyle.Flat,
            BackColor = color,
            ForeColor = Color.White
        };
        button.FlatAppearance.BorderSize = 0;
        button.Click += handler;
        return button;
    }

    private void ConfigureGridStyle()
    {
        _booksGrid.BorderStyle = BorderStyle.None;
        _booksGrid.BackgroundColor = Color.White;
        _booksGrid.RowHeadersVisible = false;
        _booksGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        _booksGrid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(245, 249, 252);
        _booksGrid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(31, 78, 121);
        _booksGrid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
        _booksGrid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        _booksGrid.EnableHeadersVisualStyles = false;

        _booksGrid.Columns.Clear();
        _booksGrid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(Book.Id), HeaderText = "ID", FillWeight = 40 });
        _booksGrid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(Book.Title), HeaderText = "Titre", FillWeight = 150 });
        _booksGrid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(Book.Author), HeaderText = "Auteur", FillWeight = 120 });
        _booksGrid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(Book.Isbn), HeaderText = "ISBN", FillWeight = 90 });
        _booksGrid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(Book.PublicationYear), HeaderText = "Annee", FillWeight = 60 });
        _booksGrid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(Book.Genre), HeaderText = "Genre", FillWeight = 80 });
        _booksGrid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(Book.ShelfSection), HeaderText = "Rayon", FillWeight = 55 });
        _booksGrid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(Book.ShelfNumber), HeaderText = "Etagere", FillWeight = 55 });
        _booksGrid.Columns.Add(new DataGridViewCheckBoxColumn { DataPropertyName = nameof(Book.IsAvailable), HeaderText = "Dispo", FillWeight = 55 });
        _booksGrid.Columns.Add(new DataGridViewButtonColumn
        {
            Name = "ActionColumn",
            HeaderText = "",
            Text = "...",
            UseColumnTextForButtonValue = true,
            FillWeight = 30
        });
    }

    private void ConfigureMenu()
    {
        _rowMenu.Items.Add("Modifier rapidement", null, (_, _) => UpdateBook());
        _rowMenu.Items.Add("Supprimer", null, (_, _) => DeleteBook());
        _rowMenu.Items.Add("Copier ISBN", null, (_, _) =>
        {
            if (_booksGrid.CurrentRow?.DataBoundItem is Book book)
            {
                Clipboard.SetText(book.Isbn);
            }
        });
    }

    private void BooksGridOnCellContentClick(object? sender, DataGridViewCellEventArgs e)
    {
        if (e.RowIndex < 0)
        {
            return;
        }

        if (_booksGrid.Columns[e.ColumnIndex].Name != "ActionColumn")
        {
            return;
        }

        _booksGrid.CurrentCell = _booksGrid.Rows[e.RowIndex].Cells[e.ColumnIndex];
        var cellDisplayRect = _booksGrid.GetCellDisplayRectangle(e.ColumnIndex, e.RowIndex, true);
        var menuPosition = new Point(cellDisplayRect.Left + 20, cellDisplayRect.Bottom + 6);
        _rowMenu.Show(_booksGrid, menuPosition);
    }

    private void LoadBooks()
    {
        if (_repository is null)
        {
            InitializeRepository();
            if (_repository is null)
            {
                return;
            }
        }

        TryExecute(() =>
        {
            var books = _repository!.GetAll();
            UpdateGridData(books);
        });
    }

    private void SearchBooks()
    {
        if (_repository is null)
        {
            MessageBox.Show("Connexion MySQL indisponible. Verifie appsettings.json et ton serveur MySQL.", "Information");
            return;
        }

        TryExecute(() =>
        {
            var books = _repository!.Search(_titleBox.Text, _authorBox.Text, _genreBox.Text, _isbnBox.Text);
            UpdateGridData(books);
        });
    }

    private void UpdateGridData(List<Book> books)
    {
        _currentBooks.Clear();
        _currentBooks.AddRange(books);
        _bindingSource.DataSource = _currentBooks.ToList();
        _selectedLabel.Text = $"{_currentBooks.Count} livre(s) affiche(s)";
        if (_currentBooks.Count == 0)
        {
            _coverPreview.Image = null;
        }
    }

    private void AddBook()
    {
        if (_repository is null)
        {
            MessageBox.Show("Connexion MySQL indisponible. Verifie appsettings.json et ton serveur MySQL.", "Information");
            return;
        }

        if (!TryBuildBook(out var book))
        {
            return;
        }

        TryExecute(() =>
        {
            _repository!.Add(book);
            LoadBooks();
            ClearInputs();
        });
    }

    private void UpdateBook()
    {
        if (_repository is null)
        {
            MessageBox.Show("Connexion MySQL indisponible. Verifie appsettings.json et ton serveur MySQL.", "Information");
            return;
        }

        if (_booksGrid.CurrentRow?.DataBoundItem is not Book selected)
        {
            MessageBox.Show("Selectionne d'abord un livre a modifier.", "Information");
            return;
        }

        if (!TryBuildBook(out var book))
        {
            return;
        }

        book.Id = selected.Id;

        TryExecute(() =>
        {
            _repository!.Update(book);
            LoadBooks();
        });
    }

    private void DeleteBook()
    {
        if (_repository is null)
        {
            MessageBox.Show("Connexion MySQL indisponible. Verifie appsettings.json et ton serveur MySQL.", "Information");
            return;
        }

        if (_booksGrid.CurrentRow?.DataBoundItem is not Book selected)
        {
            MessageBox.Show("Selectionne d'abord un livre a supprimer.", "Information");
            return;
        }

        var confirm = MessageBox.Show(
            $"Supprimer '{selected.Title}' ?",
            "Confirmation",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning);

        if (confirm != DialogResult.Yes)
        {
            return;
        }

        TryExecute(() =>
        {
            _repository!.Delete(selected.Id);
            LoadBooks();
            ClearInputs();
        });
    }

    private bool TryBuildBook(out Book book)
    {
        book = new Book();

        if (string.IsNullOrWhiteSpace(_titleBox.Text) ||
            string.IsNullOrWhiteSpace(_authorBox.Text) ||
            string.IsNullOrWhiteSpace(_isbnBox.Text) ||
            string.IsNullOrWhiteSpace(_genreBox.Text) ||
            string.IsNullOrWhiteSpace(_sectionBox.Text) ||
            string.IsNullOrWhiteSpace(_shelfBox.Text))
        {
            MessageBox.Show("Tous les champs texte sont obligatoires.", "Erreur");
            return false;
        }

        book.Title = _titleBox.Text.Trim();
        book.Author = _authorBox.Text.Trim();
        book.Isbn = _isbnBox.Text.Trim();
        book.PublicationYear = (int)_yearBox.Value;
        book.Genre = _genreBox.Text.Trim();
        book.ShelfSection = _sectionBox.Text.Trim();
        book.ShelfNumber = _shelfBox.Text.Trim();
        book.IsAvailable = _availabilityBox.Checked;
        return true;
    }

    private void LoadSelectionInInputs()
    {
        if (_booksGrid.CurrentRow?.DataBoundItem is not Book book)
        {
            return;
        }

        _titleBox.Text = book.Title;
        _authorBox.Text = book.Author;
        _isbnBox.Text = book.Isbn;
        _yearBox.Value = Math.Clamp(book.PublicationYear, 0, 3000);
        _genreBox.Text = book.Genre;
        _sectionBox.Text = book.ShelfSection;
        _shelfBox.Text = book.ShelfNumber;
        _availabilityBox.Checked = book.IsAvailable;
        _selectedLabel.Text = $"{book.Title} - {book.Author}";
        _ = UpdateBookCoverAsync(book);
    }

    private async Task UpdateBookCoverAsync(Book book)
    {
        _coverCts?.Cancel();
        _coverCts?.Dispose();
        _coverCts = new CancellationTokenSource();
        var token = _coverCts.Token;
        var expectedIsbn = book.Isbn;

        _coverPreview.Image?.Dispose();
        _coverPreview.Image = BuildCoverImage(book.Title, "Chargement...");

        var downloadedCover = await _bookCoverService.TryGetCoverByIsbnAsync(book.Isbn, token);
        if (token.IsCancellationRequested || !IsSameBookStillSelected(expectedIsbn))
        {
            downloadedCover?.Dispose();
            return;
        }

        if (downloadedCover is not null)
        {
            _coverPreview.Image?.Dispose();
            _coverPreview.Image = downloadedCover;
            return;
        }

        _coverPreview.Image?.Dispose();
        _coverPreview.Image = BuildCoverImage(book.Title, book.Genre);
    }

    private bool IsSameBookStillSelected(string expectedIsbn)
    {
        if (_booksGrid.CurrentRow?.DataBoundItem is not Book current)
        {
            return false;
        }

        return string.Equals(current.Isbn, expectedIsbn, StringComparison.OrdinalIgnoreCase);
    }

    private static Image BuildCoverImage(string title, string genre)
    {
        var bitmap = new Bitmap(180, 240);
        using var graphics = Graphics.FromImage(bitmap);
        graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        graphics.Clear(Color.FromArgb(32, 58, 95));

        using var accent = new SolidBrush(Color.FromArgb(246, 196, 80));
        graphics.FillRectangle(accent, 0, 0, bitmap.Width, 26);

        using var whiteBrush = new SolidBrush(Color.White);
        using var titleFont = new Font("Segoe UI", 13F, FontStyle.Bold);
        using var genreFont = new Font("Segoe UI", 9F, FontStyle.Italic);
        using var initialsFont = new Font("Segoe UI", 44F, FontStyle.Bold);

        var initials = string.Join("", title.Split(' ', StringSplitOptions.RemoveEmptyEntries).Take(2).Select(x => char.ToUpperInvariant(x[0])));
        graphics.DrawString(initials, initialsFont, whiteBrush, new PointF(18, 72));
        graphics.DrawString(title, titleFont, whiteBrush, new RectangleF(12, 14, 160, 55));
        graphics.DrawString(genre, genreFont, whiteBrush, new RectangleF(12, 204, 160, 28));
        return bitmap;
    }

    private void ClearInputs()
    {
        _titleBox.Clear();
        _authorBox.Clear();
        _isbnBox.Clear();
        _yearBox.Value = 0;
        _genreBox.Clear();
        _sectionBox.Clear();
        _shelfBox.Clear();
        _availabilityBox.Checked = false;
    }

    private static void TryExecute(Action action)
    {
        try
        {
            action();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
