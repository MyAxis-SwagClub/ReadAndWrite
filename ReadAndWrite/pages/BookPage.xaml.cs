using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.Data.SqlClient;

namespace ReadAndWrite.Pages
{
    public partial class BookPage : Page
    {
        private int _bookId;

        public BookPage(int bookId)
        {
            InitializeComponent();
            _bookId = bookId;
            Loaded += (s, e) => LoadBook();
        }

        private void LoadBook()
        {
            // Загружаем данные книги
            var book = DatabaseHelper.ExecuteQuery($@"
                SELECT b.Title, b.Description,
                       u.DisplayName AS AuthorName,
                       ISNULL(AVG(CAST(r.Rating AS FLOAT)), 0) AS AvgRating,
                       STRING_AGG(g.GenreName, ', ') AS Genres
                FROM Books b
                JOIN Users u ON b.AuthorId = u.UserId
                LEFT JOIN Reviews r ON b.BookId = r.BookId
                LEFT JOIN BookGenres bg ON b.BookId = bg.BookId
                LEFT JOIN Genres g ON bg.GenreId = g.GenreId
                WHERE b.BookId = {_bookId}
                GROUP BY b.Title, b.Description, u.DisplayName");

            if (book.Rows.Count == 0) return;

            var row = book.Rows[0];
            TxtTitle.Text = row["Title"].ToString();
            TxtAuthor.Text = "Автор: " + row["AuthorName"].ToString();
            TxtGenres.Text = "Жанры: " + row["Genres"]?.ToString();
            TxtRating.Text = $"⭐ {(double)row["AvgRating"]:F1}";
            TxtDescription.Text = row["Description"]?.ToString();

            LoadReviews();
        }

        private void LoadReviews()
        {
            var reviews = DatabaseHelper.ExecuteQuery($@"
                SELECT u.DisplayName, r.Rating, r.ReviewText, r.CreatedAt
                FROM Reviews r
                JOIN Users u ON r.UserId = u.UserId
                WHERE r.BookId = {_bookId}
                ORDER BY r.CreatedAt DESC");

            ReviewsPanel.Children.Clear();

            if (reviews.Rows.Count == 0)
            {
                ReviewsPanel.Children.Add(new TextBlock
                {
                    Text = "Отзывов пока нет",
                    Foreground = Brushes.Gray,
                    Margin = new Thickness(0, 5, 0, 5)
                });
                return;
            }

            foreach (DataRow row in reviews.Rows)
            {
                var card = new Border
                {
                    Background = Brushes.White,
                    BorderBrush = new SolidColorBrush(Color.FromRgb(221, 225, 231)),
                    BorderThickness = new Thickness(1),
                    CornerRadius = new CornerRadius(6),
                    Padding = new Thickness(12),
                    Margin = new Thickness(0, 0, 0, 8)
                };

                var stack = new StackPanel();

                stack.Children.Add(new TextBlock
                {
                    Text = $"{row["DisplayName"]}  ⭐ {row["Rating"]}",
                    FontWeight = FontWeights.Bold,
                    FontSize = 13,
                    Margin = new Thickness(0, 0, 0, 4)
                });

                stack.Children.Add(new TextBlock
                {
                    Text = row["ReviewText"]?.ToString(),
                    TextWrapping = TextWrapping.Wrap,
                    FontSize = 13,
                    Foreground = new SolidColorBrush(Color.FromRgb(80, 80, 80))
                });

                card.Child = stack;
                ReviewsPanel.Children.Add(card);
            }
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }

        private void BtnAddToList_Click(object sender, RoutedEventArgs e)
        {
            if (CmbReadingList.SelectedIndex <= 0) return;

            string section = (CmbReadingList.SelectedItem as ComboBoxItem)
                ?.Content.ToString();

            try
            {
                // Удаляем старую запись если есть
                DatabaseHelper.ExecuteNonQuery(
                    "DELETE FROM ReadingLists WHERE UserId = @uid AND BookId = @bid",
                    new SqlParameter[]
                    {
                        new SqlParameter("@uid", CurrentUser.UserId),
                        new SqlParameter("@bid", _bookId)
                    });

                // Добавляем новую
                DatabaseHelper.ExecuteNonQuery(
                    "INSERT INTO ReadingLists (UserId, BookId, Section) " +
                    "VALUES (@uid, @bid, @section)",
                    new SqlParameter[]
                    {
                        new SqlParameter("@uid", CurrentUser.UserId),
                        new SqlParameter("@bid", _bookId),
                        new SqlParameter("@section", section)
                    });

                MessageBox.Show("Книга добавлена в список!");
            }
            catch
            {
                MessageBox.Show("Ошибка при добавлении в список");
            }
        }

        private void BtnSendReview_Click(object sender, RoutedEventArgs e)
        {
            string text = TxtReview.Text.Trim();
            if (string.IsNullOrEmpty(text) || CmbRating.SelectedItem == null)
            {
                MessageBox.Show("Заполните отзыв и выберите оценку");
                return;
            }

            int rating = int.Parse((CmbRating.SelectedItem as ComboBoxItem)
                ?.Content.ToString());

            try
            {
                DatabaseHelper.ExecuteNonQuery(
                    "INSERT INTO Reviews (BookId, UserId, ReviewText, Rating) " +
                    "VALUES (@bid, @uid, @text, @rating)",
                    new SqlParameter[]
                    {
                        new SqlParameter("@bid", _bookId),
                        new SqlParameter("@uid", CurrentUser.UserId),
                        new SqlParameter("@text", text),
                        new SqlParameter("@rating", rating)
                    });

                TxtReview.Text = "";
                CmbRating.SelectedIndex = -1;
                LoadReviews();
                MessageBox.Show("Отзыв отправлен!");
            }
            catch
            {
                MessageBox.Show("Ошибка при отправке отзыва");
            }
        }
    }
}