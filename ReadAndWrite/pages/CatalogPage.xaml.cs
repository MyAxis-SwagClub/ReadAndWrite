using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ReadAndWrite.Pages
{
    public partial class CatalogPage : Page
    {
        public CatalogPage()
        {
            InitializeComponent();
            Loaded += (s, e) =>
            {
                LoadGenres();
                LoadBooks();
            };
        }

        private void LoadGenres()
        {
            CmbGenre.Items.Clear();
            CmbGenre.Items.Add(new ComboBoxItem
            {
                Content = "Все жанры",
                IsSelected = true
            });

            var genres = DatabaseHelper.ExecuteQuery(
                "SELECT GenreId, GenreName FROM Genres ORDER BY GenreName");

            foreach (DataRow row in genres.Rows)
            {
                CmbGenre.Items.Add(new ComboBoxItem
                {
                    Content = row["GenreName"].ToString(),
                    Tag = row["GenreId"]
                });
            }
        }

        private void LoadBooks(string search = "", string sortBy = "Title", int genreId = 0)
        {
            string query = @"
                SELECT b.BookId, b.Title,
                       u.DisplayName AS AuthorName,
                       ISNULL(AVG(CAST(r.Rating AS FLOAT)), 0) AS AvgRating,
                       STRING_AGG(g.GenreName, ', ') AS Genres
                FROM Books b
                JOIN Users u ON b.AuthorId = u.UserId
                LEFT JOIN Reviews r ON b.BookId = r.BookId
                LEFT JOIN BookGenres bg ON b.BookId = bg.BookId
                LEFT JOIN Genres g ON bg.GenreId = g.GenreId
                WHERE b.IsFrozen = 0";

            if (!string.IsNullOrEmpty(search))
                query += $" AND (b.Title LIKE '%{search}%'" +
                         $" OR u.DisplayName LIKE '%{search}%')";

            if (genreId > 0)
                query += $" AND bg.GenreId = {genreId}";

            query += " GROUP BY b.BookId, b.Title, u.DisplayName";
            query += sortBy == "Rating"
                ? " ORDER BY AvgRating DESC"
                : " ORDER BY b.Title";

            var books = DatabaseHelper.ExecuteQuery(query);
            BooksPanel.Children.Clear();

            if (books.Rows.Count == 0)
            {
                BooksPanel.Children.Add(new TextBlock
                {
                    Text = "Книги не найдены",
                    FontSize = 16,
                    Margin = new Thickness(20)
                });
                return;
            }

            foreach (DataRow row in books.Rows)
                BooksPanel.Children.Add(CreateBookCard(row));
        }

        private Border CreateBookCard(DataRow row)
        {
            int bookId = (int)row["BookId"];
            string title = row["Title"].ToString();
            string author = row["AuthorName"].ToString();
            double rating = (double)row["AvgRating"];
            string genres = row["Genres"]?.ToString() ?? "";

            var card = new Border
            {
                Width = 200,
                Margin = new Thickness(10),
                BorderBrush = new SolidColorBrush(Color.FromRgb(220, 225, 230)),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(8),
                Background = Brushes.White,
                Effect = new System.Windows.Media.Effects.DropShadowEffect
                {
                    Color = Colors.Gray,
                    BlurRadius = 6,
                    Opacity = 0.15,
                    ShadowDepth = 2
                }
            };

            var stack = new StackPanel();

            // Обложка
            stack.Children.Add(new Border
            {
                Height = 130,
                Background = new SolidColorBrush(Color.FromRgb(41, 128, 185)),
                CornerRadius = new CornerRadius(8, 8, 0, 0),
                Child = new TextBlock
                {
                    Text = "📖",
                    FontSize = 40,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                }
            });

            var info = new StackPanel { Margin = new Thickness(10) };

            info.Children.Add(new TextBlock
            {
                Text = title,
                FontWeight = FontWeights.Bold,
                FontSize = 13,
                TextWrapping = TextWrapping.Wrap,
                Foreground = new SolidColorBrush(Color.FromRgb(44, 62, 80))
            });

            info.Children.Add(new TextBlock
            {
                Text = author,
                Foreground = Brushes.Gray,
                FontSize = 12,
                Margin = new Thickness(0, 4, 0, 2)
            });

            info.Children.Add(new TextBlock
            {
                Text = $"⭐ {rating:F1}",
                FontSize = 12,
                Foreground = new SolidColorBrush(Color.FromRgb(243, 156, 18)),
                Margin = new Thickness(0, 0, 0, 4)
            });

            info.Children.Add(new TextBlock
            {
                Text = genres,
                Foreground = Brushes.SlateGray,
                FontSize = 11,
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 0, 0, 8)
            });

            var btnOpen = new Button
            {
                Content = "Открыть →",
                Background = new SolidColorBrush(Color.FromRgb(41, 128, 185)),
                Foreground = Brushes.White,
                BorderThickness = new Thickness(0),
                Padding = new Thickness(0, 7, 0, 7),
                FontSize = 13,
                Tag = bookId
            };
            btnOpen.Click += BtnOpenBook_Click;
            info.Children.Add(btnOpen);

            stack.Children.Add(info);
            card.Child = stack;
            return card;
        }

        private void BtnOpenBook_Click(object sender, RoutedEventArgs e)
        {
            int bookId = (int)((Button)sender).Tag;
            NavigationService.Navigate(new BookPage(bookId));
        }

        private void BtnSearch_Click(object sender, RoutedEventArgs e)
            => ApplyFilters();

        private void CmbSort_Changed(object sender, SelectionChangedEventArgs e)
        {
            if (CmbGenre == null || BooksPanel == null) return;
            ApplyFilters();
        }

        private void CmbGenre_Changed(object sender, SelectionChangedEventArgs e)
        {
            if (BooksPanel == null) return;
            ApplyFilters();
        }

        private void ApplyFilters()
        {
            string search = TxtSearch.Text.Trim();
            string sort = (CmbSort.SelectedItem as ComboBoxItem)?
                .Content.ToString() == "По оценке" ? "Rating" : "Title";

            int genreId = 0;
            if (CmbGenre.SelectedItem is ComboBoxItem item && item.Tag != null)
                genreId = (int)item.Tag;

            LoadBooks(search, sort, genreId);
        }
    }
}