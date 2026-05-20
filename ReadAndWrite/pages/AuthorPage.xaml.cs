using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.Data.SqlClient;

namespace ReadAndWrite.Pages
{
    public partial class AuthorPage : Page
    {
        public AuthorPage()
        {
            InitializeComponent();
            Loaded += (s, e) =>
            {
                LoadGenres();
                LoadMyBooks();
            };
        }

        private void LoadGenres()
        {
            var genres = DatabaseHelper.ExecuteQuery(
                "SELECT GenreId, GenreName FROM Genres ORDER BY GenreName");

            CmbGenre.Items.Clear();
            foreach (DataRow row in genres.Rows)
            {
                CmbGenre.Items.Add(new ComboBoxItem
                {
                    Content = row["GenreName"].ToString(),
                    Tag = row["GenreId"]
                });
            }
        }

        private void LoadMyBooks()
        {
            var books = DatabaseHelper.ExecuteQuery($@"
                SELECT b.BookId, b.Title, b.IsFrozen,
                       ISNULL(AVG(CAST(r.Rating AS FLOAT)), 0) AS AvgRating,
                       COUNT(r.ReviewId) AS ReviewCount
                FROM Books b
                LEFT JOIN Reviews r ON b.BookId = r.BookId
                WHERE b.AuthorId = {CurrentUser.UserId}
                GROUP BY b.BookId, b.Title, b.IsFrozen
                ORDER BY b.Title");

            BooksPanel.Children.Clear();

            if (books.Rows.Count == 0)
            {
                BooksPanel.Children.Add(new TextBlock
                {
                    Text = "У вас пока нет книг",
                    FontSize = 14,
                    Foreground = Brushes.Gray,
                    Margin = new Thickness(0, 10, 0, 0)
                });
                return;
            }

            foreach (DataRow row in books.Rows)
            {
                bool isFrozen = (bool)row["IsFrozen"];

                var card = new Border
                {
                    Background = isFrozen
                        ? new SolidColorBrush(Color.FromRgb(253, 237, 236))
                        : Brushes.White,
                    CornerRadius = new CornerRadius(4),
                    Padding = new Thickness(15),
                    Margin = new Thickness(0, 0, 0, 8)
                };

                var grid = new Grid();
                grid.ColumnDefinitions.Add(new ColumnDefinition
                { Width = new GridLength(1, GridUnitType.Star) });
                grid.ColumnDefinitions.Add(new ColumnDefinition
                { Width = GridLength.Auto });

                var info = new StackPanel();

                info.Children.Add(new TextBlock
                {
                    Text = row["Title"].ToString() + (isFrozen ? "  🔒 Заморожена" : ""),
                    FontSize = 14,
                    FontWeight = FontWeights.Bold,
                    Foreground = isFrozen ? Brushes.Red
                        : new SolidColorBrush(Color.FromRgb(26, 82, 118))
                });

                info.Children.Add(new TextBlock
                {
                    Text = $"⭐ {(double)row["AvgRating"]:F1}  •  " +
                           $"Отзывов: {row["ReviewCount"]}",
                    FontSize = 12,
                    Foreground = Brushes.Gray,
                    Margin = new Thickness(0, 4, 0, 0)
                });

                Grid.SetColumn(info, 0);
                grid.Children.Add(info);

                var buttons = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    VerticalAlignment = VerticalAlignment.Center
                };

                var btnDelete = new Button
                {
                    Content = "Удалить",
                    Background = new SolidColorBrush(Color.FromRgb(192, 57, 43)),
                    Foreground = Brushes.White,
                    BorderThickness = new Thickness(0),
                    Padding = new Thickness(10, 5, 10, 5),
                    Tag = row["BookId"]
                };
                btnDelete.Click += BtnDelete_Click;
                buttons.Children.Add(btnDelete);

                Grid.SetColumn(buttons, 1);
                grid.Children.Add(buttons);

                card.Child = grid;
                BooksPanel.Children.Add(card);
            }
        }

        private void BtnPublish_Click(object sender, RoutedEventArgs e)
        {
            string title = TxtTitle.Text.Trim();
            string description = TxtDescription.Text.Trim();

            if (string.IsNullOrEmpty(title))
            {
                MessageBox.Show("Введите название книги");
                return;
            }

            if (CmbGenre.SelectedItem == null)
            {
                MessageBox.Show("Выберите жанр");
                return;
            }

            int genreId = (int)((ComboBoxItem)CmbGenre.SelectedItem).Tag;

            try
            {
                // Добавляем книгу
                int bookId = (int)DatabaseHelper.ExecuteScalar(
                    "INSERT INTO Books (Title, Description, AuthorId, IsFrozen) " +
                    "VALUES (@title, @desc, @author, 0); " +
                    "SELECT SCOPE_IDENTITY();",
                    new SqlParameter[]
                    {
                        new SqlParameter("@title", title),
                        new SqlParameter("@desc", description),
                        new SqlParameter("@author", CurrentUser.UserId)
                    });

                // Привязываем жанр
                DatabaseHelper.ExecuteNonQuery(
                    "INSERT INTO BookGenres (BookId, GenreId) VALUES (@bid, @gid)",
                    new SqlParameter[]
                    {
                        new SqlParameter("@bid", bookId),
                        new SqlParameter("@gid", genreId)
                    });

                TxtTitle.Text = "";
                TxtDescription.Text = "";
                CmbGenre.SelectedIndex = -1;
                LoadMyBooks();
                MessageBox.Show("Книга опубликована!");
            }
            catch
            {
                MessageBox.Show("Ошибка при публикации");
            }
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            int bookId = (int)((Button)sender).Tag;
            var result = MessageBox.Show("Удалить книгу?", "Подтверждение",
                MessageBoxButton.YesNo);

            if (result != MessageBoxResult.Yes) return;

            try
            {
                DatabaseHelper.ExecuteNonQuery(
                    "DELETE FROM BookGenres WHERE BookId = @id;" +
                    "DELETE FROM Reviews WHERE BookId = @id;" +
                    "DELETE FROM ReadingLists WHERE BookId = @id;" +
                    "DELETE FROM Books WHERE BookId = @id;",
                    new SqlParameter[]
                    {
                        new SqlParameter("@id", bookId)
                    });
                LoadMyBooks();
            }
            catch
            {
                MessageBox.Show("Ошибка при удалении");
            }
        }
    }
}