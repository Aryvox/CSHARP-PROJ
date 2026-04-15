namespace WeatherApp;

public class MainForm : Form
{
    private readonly OpenWeatherService _weatherService;
    private readonly FavoritesService _favoritesService = new();
    private readonly List<string> _favorites = [];

    private readonly TextBox _cityBox = new() { Width = 260 };
    private readonly Button _searchButton = new() { Text = "Rechercher", Width = 140 };
    private readonly Button _addFavoriteButton = new() { Text = "Ajouter aux favoris", Width = 190 };
    private readonly Button _removeFavoriteButton = new() { Text = "Supprimer favori", Width = 175 };
    private readonly ListBox _favoritesList = new() { Width = 230, Height = 520 };
    private readonly DataGridView _forecastGrid = new()
    {
        Dock = DockStyle.Fill,
        ReadOnly = true,
        AllowUserToAddRows = false,
        AutoGenerateColumns = false
    };
    private readonly PictureBox _iconBox = new() { Width = 100, Height = 100, SizeMode = PictureBoxSizeMode.Zoom };
    private readonly Label _statusLabel = new() { AutoSize = true, ForeColor = Color.DimGray };
    private readonly Label _cityTitleLabel = new() { AutoSize = true, Font = new Font("Segoe UI", 12F, FontStyle.Bold) };
    private readonly Label _tempLabel = new() { AutoSize = true, Font = new Font("Segoe UI", 17F, FontStyle.Bold) };
    private readonly Label _detailsLabel = new() { AutoSize = true, ForeColor = Color.DimGray };

    public MainForm()
    {
        Text = "Application Meteo - OpenWeather";
        Width = 1200;
        Height = 780;
        StartPosition = FormStartPosition.CenterScreen;
        Font = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point);
        BackColor = Color.WhiteSmoke;

        _weatherService = new OpenWeatherService(AppConfig.Load());

        BuildLayout();
        HookEvents();
        ConfigureGridStyle();
        StyleButtons();
        LoadFavorites();
        UpdateStatus("Saisis une ville puis clique sur Rechercher.");
    }

    private void BuildLayout()
    {
        var topPanel = new Panel { Dock = DockStyle.Top, Height = 96, Padding = new Padding(8) };
        var searchGroup = new GroupBox { Text = "Recherche meteo", Dock = DockStyle.Fill, BackColor = Color.White };
        var topFlow = new FlowLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(10), WrapContents = false, AutoScroll = true };
        topFlow.Controls.AddRange([
            new Label { Text = "Ville", AutoSize = true, Padding = new Padding(0, 10, 6, 0), Font = new Font("Segoe UI", 10F, FontStyle.Bold) },
            _cityBox,
            _searchButton,
            _addFavoriteButton,
            _removeFavoriteButton
        ]);
        searchGroup.Controls.Add(topFlow);
        topPanel.Controls.Add(searchGroup);

        var leftPanel = new Panel { Dock = DockStyle.Left, Width = 280, Padding = new Padding(8, 0, 8, 8) };
        var favoritesGroup = new GroupBox { Text = "Favoris", Dock = DockStyle.Fill, BackColor = Color.White };
        favoritesGroup.Controls.Add(_favoritesList);
        favoritesGroup.Controls.Add(new Label
        {
            Text = "Double-clic sur une ville pour lancer la recherche",
            Dock = DockStyle.Top,
            Height = 28,
            TextAlign = ContentAlignment.MiddleLeft
        });
        _favoritesList.Dock = DockStyle.Fill;
        leftPanel.Controls.Add(favoritesGroup);

        var centerPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(0, 0, 8, 8) };
        var summaryGroup = new GroupBox { Dock = DockStyle.Top, Height = 160, Text = "Meteo actuelle (premiere prevision)", BackColor = Color.White };
        var summaryLayout = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight, Padding = new Padding(10), WrapContents = false };
        var textPanel = new FlowLayoutPanel { Width = 560, Height = 120, FlowDirection = FlowDirection.TopDown, WrapContents = false };
        textPanel.Controls.AddRange([_cityTitleLabel, _tempLabel, _detailsLabel, _statusLabel]);
        summaryLayout.Controls.Add(_iconBox);
        summaryLayout.Controls.Add(textPanel);
        summaryGroup.Controls.Add(summaryLayout);

        var tableGroup = new GroupBox { Dock = DockStyle.Fill, Text = "Previsions sur plusieurs jours", BackColor = Color.White, Padding = new Padding(8) };
        tableGroup.Controls.Add(_forecastGrid);

        centerPanel.Controls.Add(tableGroup);
        centerPanel.Controls.Add(summaryGroup);

        _cityTitleLabel.Text = "Aucune ville selectionnee";
        _tempLabel.Text = "--.- °C";
        _detailsLabel.Text = "Nuages: -- | Humidite: -- | Vent: --";

        Controls.Add(centerPanel);
        Controls.Add(leftPanel);
        Controls.Add(topPanel);
    }

    private void HookEvents()
    {
        _searchButton.Click += async (_, _) => await SearchWeatherAsync();
        _cityBox.KeyDown += async (_, e) =>
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                await SearchWeatherAsync();
            }
        };
        _addFavoriteButton.Click += (_, _) => AddCurrentCityToFavorites();
        _removeFavoriteButton.Click += (_, _) => RemoveSelectedFavorite();
        _favoritesList.DoubleClick += async (_, _) =>
        {
            if (_favoritesList.SelectedItem is string city)
            {
                _cityBox.Text = city;
                await SearchWeatherAsync();
            }
        };
    }

    private async Task SearchWeatherAsync()
    {
        var city = _cityBox.Text.Trim();
        if (string.IsNullOrWhiteSpace(city))
        {
            MessageBox.Show("Saisis une ville avant de lancer la recherche.", "Information");
            return;
        }

        try
        {
            UseWaitCursor = true;
            _searchButton.Enabled = false;
            UpdateStatus("Recherche en cours...");
            var items = await _weatherService.GetForecastAsync(city);
            _forecastGrid.DataSource = items.Select(x => new ForecastRow
            {
                Date = x.DateTime.ToString("ddd dd/MM HH:mm"),
                Temperature = $"{x.Temperature:0.0} °C",
                CloudCoverage = $"{x.CloudCoverage}%",
                Humidity = $"{x.Humidity}%",
                WindSpeed = $"{x.WindSpeed:0.0} m/s",
                Description = x.Description
            }).ToList();

            var first = items.FirstOrDefault();
            if (first is not null && !string.IsNullOrWhiteSpace(first.IconCode))
            {
                _iconBox.LoadAsync(first.IconUrl);
                _cityTitleLabel.Text = city;
                _tempLabel.Text = $"{first.Temperature:0.0} °C";
                _detailsLabel.Text = $"Nuages: {first.CloudCoverage}% | Humidite: {first.Humidity}% | Vent: {first.WindSpeed:0.0} m/s";
                UpdateStatus($"Previsions chargees ({items.Count} points).");
            }
            else
            {
                _iconBox.Image = null;
                _cityTitleLabel.Text = city;
                _tempLabel.Text = "--.- °C";
                _detailsLabel.Text = "Nuages: -- | Humidite: -- | Vent: --";
                UpdateStatus("Aucune donnee meteo detaillee disponible.");
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
            UpdateStatus("Erreur lors de la recuperation des donnees.");
        }
        finally
        {
            UseWaitCursor = false;
            _searchButton.Enabled = true;
        }
    }

    private void LoadFavorites()
    {
        _favorites.Clear();
        _favorites.AddRange(_favoritesService.LoadFavorites());
        RefreshFavoriteList();
    }

    private void AddCurrentCityToFavorites()
    {
        var city = _cityBox.Text.Trim();
        if (string.IsNullOrWhiteSpace(city))
        {
            MessageBox.Show("Saisis une ville pour l'ajouter en favori.", "Information");
            return;
        }

        if (_favorites.Any(x => x.Equals(city, StringComparison.OrdinalIgnoreCase)))
        {
            MessageBox.Show("Cette ville est deja en favori.", "Information");
            return;
        }

        _favorites.Add(city);
        SaveAndRefreshFavorites();
    }

    private void RemoveSelectedFavorite()
    {
        if (_favoritesList.SelectedItem is not string city)
        {
            MessageBox.Show("Selectionne une ville a supprimer.", "Information");
            return;
        }

        _favorites.RemoveAll(x => x.Equals(city, StringComparison.OrdinalIgnoreCase));
        SaveAndRefreshFavorites();
    }

    private void SaveAndRefreshFavorites()
    {
        _favoritesService.SaveFavorites(_favorites);
        RefreshFavoriteList();
    }

    private void RefreshFavoriteList()
    {
        _favoritesList.DataSource = null;
        _favoritesList.DataSource = _favorites.OrderBy(x => x).ToList();
    }

    private void ConfigureGridStyle()
    {
        _forecastGrid.RowHeadersVisible = false;
        _forecastGrid.BorderStyle = BorderStyle.None;
        _forecastGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        _forecastGrid.EnableHeadersVisualStyles = false;
        _forecastGrid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(0, 98, 146);
        _forecastGrid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
        _forecastGrid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        _forecastGrid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(244, 248, 252);
        _forecastGrid.Columns.Clear();
        _forecastGrid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(ForecastRow.Date), HeaderText = "Date", FillWeight = 150 });
        _forecastGrid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(ForecastRow.Temperature), HeaderText = "Temperature", FillWeight = 85 });
        _forecastGrid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(ForecastRow.CloudCoverage), HeaderText = "Nuages", FillWeight = 75 });
        _forecastGrid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(ForecastRow.Humidity), HeaderText = "Humidite", FillWeight = 75 });
        _forecastGrid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(ForecastRow.WindSpeed), HeaderText = "Vent", FillWeight = 80 });
        _forecastGrid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(ForecastRow.Description), HeaderText = "Description", FillWeight = 180 });
    }

    private void StyleButtons()
    {
        StyleButton(_searchButton, Color.FromArgb(0, 120, 170));
        StyleButton(_addFavoriteButton, Color.FromArgb(40, 167, 69));
        StyleButton(_removeFavoriteButton, Color.FromArgb(220, 53, 69));
    }

    private static void StyleButton(Button button, Color backColor)
    {
        button.FlatStyle = FlatStyle.Flat;
        button.FlatAppearance.BorderSize = 0;
        button.BackColor = backColor;
        button.ForeColor = Color.White;
        button.Height = 38;
        button.TextAlign = ContentAlignment.MiddleCenter;
        button.Padding = new Padding(8, 0, 8, 0);
        button.AutoEllipsis = false;
    }

    private void UpdateStatus(string message)
    {
        _statusLabel.Text = message;
    }

    private class ForecastRow
    {
        public string Date { get; set; } = string.Empty;
        public string Temperature { get; set; } = string.Empty;
        public string CloudCoverage { get; set; } = string.Empty;
        public string Humidity { get; set; } = string.Empty;
        public string WindSpeed { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }
}
